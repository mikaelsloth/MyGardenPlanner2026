namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Infrastructure.Data;

public static class DatabaseServicesExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration, string? provider)
    {
        services.AddDbContextFactory<PlannerDbContext>(options =>
        {
            switch (provider)
            {
                case "SqlExpressConnection":
                    options.UseSqlServer(
                        configuration.GetConnectionString("SqlExpressConnection"),
                        sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    break;
                case "SqliteConnection":
                    options.UseSqlite(
                        configuration.GetConnectionString("SqliteConnection"));
                    break;
                default:
                    throw new InvalidOperationException($"Unknown DatabaseProvider: \"{provider}\"");
            }
        });
        services.AddDatabaseDeveloperPageExceptionFilter();

        return services;
    }
}