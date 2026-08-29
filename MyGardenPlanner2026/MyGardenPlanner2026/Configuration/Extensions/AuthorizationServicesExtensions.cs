namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.AspNetCore.Authorization;
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
        services.AddScoped<IAuthorizationHandler, MfaAuthorizationHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy(RequireGlobalAdminPolicy, policy => policy.RequireRole(SystemAdminRole))
            .AddPolicy(RequireGlobalAdminPolicy, policy => policy.RequireJitRole(SystemAdminRole).AddRequirements(new MfaRequirement()))
            .AddPolicy(RequireDataAdminPolicy, policy => policy.RequireJitRole(DataAdminRole).AddRequirements(new MfaRequirement()))
            .AddPolicy(RequirePolicyAdminPolicy, policy => policy.RequireJitRole(PolicyAdminRole).AddRequirements(new MfaRequirement()))
            .AddPolicy(RequireAuditViewerPolicy, policy => policy.RequireJitRole(AuditViewerRole).AddRequirements(new MfaRequirement()));

        return services;
    }
}