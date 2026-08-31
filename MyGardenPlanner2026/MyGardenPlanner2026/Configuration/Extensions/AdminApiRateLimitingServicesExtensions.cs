namespace MyGardenPlanner2026.Configuration.Extensions;

using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;

public static class AdminApiRateLimitingServicesExtensions
{
    public static IServiceCollection AddAdminApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AdminApiRateLimitOptions>(
            configuration.GetSection(AdminApiRateLimitOptions.SectionName));

        services.AddSingleton<IAdminActionRateLimiter, AdminActionRateLimiter>();

        return services;
    }
}