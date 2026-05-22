using Aimbys.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    /// connection string at <c>ConnectionStrings:Default</c>.
    ///
    /// The DbContext is registered even when the connection string is empty so
    /// that the Web host can start without a database. Calls into the context
    /// will fail at runtime with a clear SqlException describing the missing
    /// configuration; this is intentional &mdash; chunk 2 of the migration plan
    /// only sets up the schema, it does not yet require a live database to
    /// serve a request.
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

        return services;
    }
}
