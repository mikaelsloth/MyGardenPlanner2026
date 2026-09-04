namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Configuration.Authorization;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;

public static class JitServicesExtensions
{
    public static IServiceCollection AddJitElevationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JitElevationPolicyOptions>(
            configuration.GetSection(JitElevationPolicyOptions.SectionName));

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IJitElevationService, JitElevationService>();
        services.AddScoped<IAuthorizationHandler, JitRoleAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, AnyAdminRoleAuthorizationHandler>();
        services.AddHostedService<RoleElevationExpirySweepService>();

        return services;
    }
}