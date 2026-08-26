namespace MyGardenPlanner2026.Configuration.Authorization;

using Microsoft.AspNetCore.Authorization;

public static class JitAuthorizationPolicyBuilderExtensions
{
    /// <summary>
    /// Tilføjer et JitRoleRequirement til policyen: adgang kræver enten direkte
    /// rolle-medlemskab, eller en aktiv, godkendt JIT-eskalering til samme rolle.
    /// Genanvendelig til fremtidige roller (DataAdmin, PolicyAdmin, AuditViewer m.fl.).
    /// </summary>
    public static AuthorizationPolicyBuilder RequireJitRole(this AuthorizationPolicyBuilder builder, string roleName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);

        builder.Requirements.Add(new JitRoleRequirement(roleName));
        return builder;
    }
}