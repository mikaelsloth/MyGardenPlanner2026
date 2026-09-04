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
    public const string RequireRecentAuthenticationPolicy = "RequireRecentAuthentication";
    public const string RequireAnyAdminRolePolicy = "RequireAnyAdminRole";

    public const string SystemAdminRole = RoleNames.SystemAdmin;
    public const string DataAdminRole = RoleNames.DataAdmin;
    public const string PolicyAdminRole = RoleNames.PolicyAdmin;
    public const string AuditViewerRole = RoleNames.AuditViewer;

    /// <summary>
    /// Adgang til admin-området generelt (fx JIT-anmodningssiden) kræver mindst én af
    /// disse fire roller — enten direkte eller via en aktiv, godkendt JIT-eskalering.
    /// </summary>
    private static readonly string[] AllAdminRoles = [SystemAdminRole, DataAdminRole, PolicyAdminRole, AuditViewerRole];

    public static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, MfaAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, RequireRecentAuthenticationHandler>();   // NY

        services.AddAuthorizationBuilder()
            .AddPolicy(RequireGlobalAdminPolicy, policy => policy.RequireRole(SystemAdminRole))
            .AddPolicy(RequireGlobalAdminPolicy, policy => policy.RequireJitRole(SystemAdminRole).AddRequirements(new MfaRequirement()))
            .AddPolicy(RequireDataAdminPolicy, policy => policy.RequireJitRole(DataAdminRole).AddRequirements(new MfaRequirement()))
            .AddPolicy(RequirePolicyAdminPolicy, policy => policy.RequireJitRole(PolicyAdminRole).AddRequirements(new MfaRequirement()))
            .AddPolicy(RequireAuditViewerPolicy, policy => policy.RequireJitRole(AuditViewerRole).AddRequirements(new MfaRequirement()))
            .AddPolicy(RequireRecentAuthenticationPolicy, policy => policy.RequireAuthenticatedUser().AddRequirements(new RequireRecentAuthenticationRequirement()))
            .AddPolicy(RequireAnyAdminRolePolicy, policy => policy
                .AddRequirements(new AnyAdminRoleRequirement(AllAdminRoles))
                .AddRequirements(new MfaRequirement()));

        return services;
    }
}