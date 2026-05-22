using Aimbys.Application.Authorization;
using Aimbys.Application.Notifications;
using Aimbys.Application.Notifications.Projections;
using Aimbys.Domain.Events;
using Aimbys.Application.Storage;
using Aimbys.Infrastructure.Authorization;
using Aimbys.Infrastructure.Identity;
using Aimbys.Infrastructure.Notifications;
using Aimbys.Infrastructure.Persistence;
using Aimbys.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications = Aimbys.Infrastructure.Notifications;
using Projections = Aimbys.Application.Notifications.Projections;
using Microsoft.Extensions.Hosting;

namespace Aimbys.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Configuration key used for the SQL Server connection string.
    /// </summary>
    public const string ConnectionStringName = "Default";

    /// <summary>
    /// Registers the <see cref="AppDbContext"/> against SQL Server using the
    /// connection string at <c>ConnectionStrings:Default</c>, plus ASP.NET
    /// Core Identity (with role support) backed by the same context.
    ///
    /// The DbContext is registered even when the connection string is empty so
    /// that the Web host can start without a database. Calls into the context
    /// will fail at runtime with a clear SqlException describing the missing
    /// configuration.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString ?? string.Empty,
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        });

        services
            .AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                // Sensible local-dev defaults; tighten in production via config.
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.User.RequireUniqueEmail = true;

                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.Cookie.Name = "Aimbys.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        });

        // Bind admin-seed configuration so the seeder can opt in cleanly.
        services.Configure<DefaultAdminOptions>(
            configuration.GetSection(DefaultAdminOptions.SectionName));

        // Permission guard: the only sanctioned route for checking teacher
        // permission flags. Scoped to align with AppDbContext + UserManager.
        services.AddScoped<IPermissionGuard, PermissionGuard>();

        // Domain events + notifications (Chunk 10).
        services.AddScoped<Notifications.DomainEventCollector>();
        services.AddScoped<IDomainEventDispatcher, Notifications.DomainEventDispatcher>();
        services.AddScoped<INotificationService, Notifications.NotificationService>();
        services.AddSingleton<INotificationChannel, Notifications.LoggingNotificationChannel>();

        // Register all 8 notification projections.
        services.AddScoped<INotificationProjection<PaperSubmittedEvent>, Projections.PaperSubmittedProjection>();
        services.AddScoped<INotificationProjection<PaperApprovedEvent>, Projections.PaperApprovedProjection>();
        services.AddScoped<INotificationProjection<EvaluationAssignedEvent>, Projections.EvaluationAssignedProjection>();
        services.AddScoped<INotificationProjection<ModerationReturnedEvent>, Projections.ModerationReturnedProjection>();
        services.AddScoped<INotificationProjection<ExamScheduledEvent>, Projections.ExamScheduledProjection>();
        services.AddScoped<INotificationProjection<ResultPublishedEvent>, Projections.ResultPublishedProjection>();
        services.AddScoped<INotificationProjection<InstituteApprovedEvent>, Projections.InstituteApprovedProjection>();
        services.AddScoped<INotificationProjection<UserSuspendedEvent>, Projections.UserSuspendedProjection>();
        // Tenancy resolver. Scoped because it queries AppDbContext.
        services.AddScoped<IInstituteScope, InstituteScope>();

        // Local file storage. Bind options from `FileStorage` section and
        // default RootPath to `<ContentRoot>/uploads` when unset so a fresh
        // checkout works without configuration.
        services
            .AddOptions<LocalFileStorageOptions>()
            .Bind(configuration.GetSection(LocalFileStorageOptions.SectionName))
            .PostConfigure<IHostEnvironment>((opts, env) =>
            {
                if (string.IsNullOrWhiteSpace(opts.RootPath))
                {
                    opts.RootPath = Path.Combine(env.ContentRootPath, "uploads");
                }
            });

        // The single concrete is registered once and exposed under both the
        // generic and local-specific interfaces so consumers get the same
        // scoped instance regardless of which they injected.
        services.AddScoped<LocalFileStorageService>();
        services.AddScoped<ILocalFileStorageService>(sp => sp.GetRequiredService<LocalFileStorageService>());
        services.AddScoped<IFileStorageService>(sp => sp.GetRequiredService<LocalFileStorageService>());

        return services;
    }
}
