namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.Extensions.DependencyInjection.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;

public static class ReAuthFailureTrackingServicesExtensions
{
    public static IServiceCollection AddReAuthFailureTracking(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ReAuthFailureTrackerOptions>(
            configuration.GetSection(ReAuthFailureTrackerOptions.SectionName));

        // TryAdd: undgår dobbeltregistrering med AddJitElevationServices/AddReAuthenticationServices/
        // AddSecurityAlertingServices, som alle registrerer TimeProvider.System.
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<IReAuthFailureTracker, ReAuthFailureTracker>();

        return services;
    }
}