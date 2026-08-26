namespace MyGardenPlanner2026.Configuration.Extensions;

using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;

public static class JitServicesExtensions
{
    public static IServiceCollection AddJitElevationServices(this IServiceCollection services)
    {
        services.AddScoped<IJitElevationService, JitElevationService>();

        return services;
    }
}