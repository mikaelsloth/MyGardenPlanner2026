namespace MyGardenPlanner2026.Tests.E2E;

/// <summary>
/// Hardcodet adgangsmatrix for admin-siderne (Tjekliste I): hvilke sider en persona reelt har adgang til.
/// Forventningerne er specifikationen og udledes ikke af produktionskoden. Bruges af I1 (navigation viser
/// præcis de tilladte links) og I2 (komplementet giver AccessDenied). JIT-adgang er åben for alle fire
/// admin-roller (RequireAnyAdminRole). Rækkefølgen i <see cref="AllPages"/> er navigationens rækkefølge.
/// </summary>
public static class AdminAccessMatrix
{
    public sealed record AdminPage(string Route, string Title, string Heading, string LinkText);

    public sealed record PersonaAccess(string Persona, AdminPage Start, IReadOnlyList<AdminPage> Allowed);

    public static readonly AdminPage Subscriptions = new(
        E2ERoutes.AdminSubscriptions, "Administrer abonnementer – MyGardenPlanner", "Administrer abonnementer", "Abonnementer");

    public static readonly AdminPage SecurityPolicies = new(
        E2ERoutes.AdminSecurityPolicies, "Sikkerhedspolicies – MyGardenPlanner", "Sikkerhedspolicies", "Sikkerhedspolicies");

    public static readonly AdminPage JitRequests = new(
        E2ERoutes.AdminJitRequests, "JIT-adgang – MyGardenPlanner", "JIT-adgang", "JIT-adgang");

    public static readonly AdminPage AuditLog = new(
        E2ERoutes.AdminAuditLog, "AuditLog – MyGardenPlanner", "AuditLog", "AuditLog");

    public static IReadOnlyList<AdminPage> AllPages { get; } =
        [Subscriptions, SecurityPolicies, JitRequests, AuditLog];

    private static readonly PersonaAccess[] Matrix =
    [
        new(SmokeTestPersonas.Admin, Subscriptions, [Subscriptions, JitRequests]),
        new(SmokeTestPersonas.DataAdmin, JitRequests, [JitRequests]),
        new(SmokeTestPersonas.PolicyAdmin, SecurityPolicies, [SecurityPolicies, JitRequests]),
        new(SmokeTestPersonas.Auditor, AuditLog, [JitRequests, AuditLog]),
    ];

    public static IReadOnlyList<string> Personas { get; } = [.. Matrix.Select(m => m.Persona)];

    public static PersonaAccess For(string persona) =>
        Matrix.SingleOrDefault(m => m.Persona == persona)
        ?? throw new ArgumentOutOfRangeException(nameof(persona), persona, "Personaen er ikke i adgangsmatricen.");

    /// <summary>Sider personaen IKKE har adgang til (komplementet til Allowed).</summary>
    public static IReadOnlyList<AdminPage> DeniedPages(string persona)
    {
        var allowed = For(persona).Allowed;
        return [.. AllPages.Where(p => !allowed.Contains(p))];
    }
}