namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.Extensions.DependencyInjection.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;

public static class ReAuthenticationServicesExtensions
{
    public static IServiceCollection AddReAuthenticationServices(this IServiceCollection services)
    {
        // TryAdd: gør PR'en selvstændig, uanset om AddJitElevationServices har registreret TimeProvider først.
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IReAuthenticationService, ReAuthenticationService>();
        return services;
    }
}