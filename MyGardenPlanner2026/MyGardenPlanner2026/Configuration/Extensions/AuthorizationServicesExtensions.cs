namespace MyGardenPlanner2026.Configuration.Extensions;

using MyGardenPlanner2026.Configuration.Authorization;
using MyGardenPlanner2026.Core.Entities.Common;

public static class AuthorizationServicesExtensions
{
    public const string RequireGlobalAdminPolicy = "RequireGlobalAdmin";
    public const string RequireDataAdminPolicy = "RequireDataAdmin";
    public const string RequirePolicyAdminPolicy = "RequirePolicyAdmin";
    public const string RequireAuditViewerPolicy = "RequireAuditViewer";

    public const string SystemAdminRole = RoleNames.SystemAdmin;
    public const string DataAdminRole = RoleNames.DataAdmin;
    public const string PolicyAdminRole = RoleNames.PolicyAdmin;
    public const string AuditViewerRole = RoleNames.AuditViewer;

    public static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(RequireGlobalAdminPolicy, policy => policy.RequireRole(SystemAdminRole))
            .AddPolicy(RequireGlobalAdminPolicy, policy => policy.RequireJitRole(SystemAdminRole))
            .AddPolicy(RequireDataAdminPolicy, policy => policy.RequireJitRole(DataAdminRole))
            .AddPolicy(RequirePolicyAdminPolicy, policy => policy.RequireJitRole(PolicyAdminRole))
            .AddPolicy(RequireAuditViewerPolicy, policy => policy.RequireJitRole(AuditViewerRole));

        return services;
    }
}