namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.Interceptors;

public static class DatabaseServicesExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration, string? provider)
    {
        services.AddSingleton<SoftDeleteInterceptor>();

        services.AddDbContextFactory<PlannerDbContext>((sp, options) =>
        {
            ConfigureProvider(options, configuration, provider, admin: false);
            options.AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>());
        });

        services.AddSingleton<IAdminDbContextFactory>(sp =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<PlannerDbContext>();
            ConfigureProvider(optionsBuilder, configuration, provider, admin: true);
            optionsBuilder.AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>());

            var pooled = new PooledDbContextFactory<PlannerDbContext>(optionsBuilder.Options);
            return new AdminDbContextFactory(pooled);
        });

        services.AddDatabaseDeveloperPageExceptionFilter();

        return services;
    }

    private static void ConfigureProvider(
        DbContextOptionsBuilder options, IConfiguration configuration, string? provider, bool admin)
    {
        switch (provider)
        {
            case "SqlExpressConnection":
                var key = admin ? "AdminSqlExpressConnection" : "SqlExpressConnection";
                var connectionString = configuration.GetConnectionString(key)
                    ?? configuration.GetConnectionString("SqlExpressConnection");
                options.UseSqlServer(connectionString,
                    sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                break;
            case "SqliteConnection":
                options.UseSqlite(configuration.GetConnectionString("SqliteConnection"));
                break;
            default:
                throw new InvalidOperationException($"Unknown DatabaseProvider: \"{provider}\"");
        }
    }
}