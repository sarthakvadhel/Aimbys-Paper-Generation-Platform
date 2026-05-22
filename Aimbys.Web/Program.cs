using Aimbys.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// EF Core (SQL Server) baseline. The DbContext is registered even when no
// connection string is set so the host can still start; database calls fail
// at runtime with a SqlException pointing the operator at appsettings or
// user-secrets. See README "EF Core / SQL Server setup" for local config.
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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
