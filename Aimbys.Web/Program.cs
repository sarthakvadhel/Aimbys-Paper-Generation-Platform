using Aimbys.Application.Scheduling;
using Aimbys.Infrastructure;
using Aimbys.Infrastructure.Analytics;
using Aimbys.Infrastructure.Identity;
using Aimbys.Infrastructure.Retention;
using Aimbys.Infrastructure.Storage;
using Aimbys.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//
// `AutoValidateAntiforgeryToken` is registered globally so every unsafe
// HTTP method (POST/PUT/DELETE/PATCH) requires a valid anti-forgery token.
// A controller can opt out per-action with [IgnoreAntiforgeryToken] when
// genuinely needed (e.g. external webhooks).
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
});

// EF Core (SQL Server) baseline + ASP.NET Core Identity (with role support).
// The DbContext is registered even when no connection string is set so the
// host can still start; database calls fail at runtime with a SqlException
// pointing the operator at appsettings or user-secrets. See README
// "EF Core / SQL Server setup" and "Identity / first admin user" for local
// configuration.
builder.Services.AddInfrastructure(builder.Configuration);

if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString(DependencyInjection.ConnectionStringName)))
{
    var bootLogger = LoggerFactory
        .Create(b => b.AddConfiguration(builder.Configuration.GetSection("Logging")).AddConsole())
        .CreateLogger("Aimbys.Web.Bootstrap");
    bootLogger.LogWarning(
        "ConnectionStrings:{Name} is not configured. The app will start, but any database call will fail. " +
        "Run `dotnet user-secrets set ConnectionStrings:{Name} \"<your-sqlserver-conn-str>\"` in Aimbys.Web to fix.",
        DependencyInjection.ConnectionStringName,
        DependencyInjection.ConnectionStringName);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Authentication MUST come before Authorization. Identity wires the cookie
// scheme into the request via UseAuthentication.
app.UseAuthentication();
app.UseAuthorization();

// Subscription enforcement runs after auth so we know who the user is and
// can resolve their tenant. Placed before route mapping so a blocked tenant
// short-circuits to the suspension page instead of executing controller code.
app.UseSubscriptionEnforcement();

app.MapStaticAssets();

// Areas first so {area:exists} wins over the default route for /Admin/*.
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Idempotent role + (optional) admin user seed. Does NOT throw if the
// database is unreachable so the host can still serve the login page during
// a DB outage; see IdentitySeeder for details.
using (var scope = app.Services.CreateScope())
{
    // File-storage area folders are created at startup so the first upload
    // never has to race directory creation. Idempotent.
    var storageOptions = scope.ServiceProvider
        .GetRequiredService<IOptions<LocalFileStorageOptions>>().Value;
    if (!string.IsNullOrWhiteSpace(storageOptions.RootPath))
    {
        FileFolders.EnsureCreated(storageOptions.RootPath);
    }

    await IdentitySeeder.SeedAsync(scope.ServiceProvider);

    // Register the recurring retention-enforcement job. Idempotent &mdash;
    // ScheduleRecurringAsync upserts on JobKey, so restarts don't multiply
    // rows. Wrapped in a try/catch so a transient DB outage doesn't take
    // the host down; SchedulingHostedService logs that the job is missing
    // until the next successful registration on a later restart.
    try
    {
        var scheduler = scope.ServiceProvider.GetRequiredService<ISchedulingService>();
        await scheduler.ScheduleRecurringAsync(
            RetentionEnforcementJobHandler.Key,
            RetentionEnforcementJobHandler.DefaultCron);

        // ----- Analytics nightly jobs (Chunk 30) -------------------------
        await scheduler.ScheduleRecurringAsync(
            InstituteAnalyticsAggregator.Key,
            InstituteAnalyticsAggregator.DefaultCron);
        await scheduler.ScheduleRecurringAsync(
            StudentPerformanceAggregator.Key,
            StudentPerformanceAggregator.DefaultCron);
        await scheduler.ScheduleRecurringAsync(
            EvaluatorEfficiencyAggregator.Key,
            EvaluatorEfficiencyAggregator.DefaultCron);
        await scheduler.ScheduleRecurringAsync(
            LeaderboardRecomputer.Key,
            LeaderboardRecomputer.DefaultCron);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Aimbys.Web.Bootstrap");
        logger.LogWarning(ex,
            "Failed to register the recurring retention-enforcement job at startup. "
          + "It will be retried on the next restart.");
    }
}

app.Run();
