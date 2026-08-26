namespace MyGardenPlanner2026.Configuration.Extensions;

using MyGardenPlanner2026.Configuration.Authorization;
using MyGardenPlanner2026.Core.Entities.Common;

public static class AuthorizationServicesExtensions
{
    public const string RequireGlobalAdminPolicy = "RequireGlobalAdmin";
    public const string SystemAdminRole = RoleNames.SystemAdmin;

    public static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(RequireGlobalAdminPolicy, policy => policy.RequireRole(SystemAdminRole))
            .AddPolicy(RequireGlobalAdminPolicy, policy => policy.RequireJitRole(SystemAdminRole));

        return services;
    }
}