namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Data;

public static class DatabaseServicesExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString)); // UseSqlServer til produktion

        services.AddDatabaseDeveloperPageExceptionFilter();

        return services;
    }
}