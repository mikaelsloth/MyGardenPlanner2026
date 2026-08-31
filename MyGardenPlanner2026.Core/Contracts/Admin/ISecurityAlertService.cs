namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Udsender sikkerhedsalarmer (§4.2) ved kritiske sikkerhedshændelser. Implementeringen
/// logger strukturerede events (Windows Event Log/Serilog-klar) og sender e-mail til
/// den konfigurerede admin-sikkerhedsliste (Smtp:AdminSecurityEmails).
/// </summary>
public interface ISecurityAlertService
{
    /// <summary>Kaldes når en bruger har haft gentagne fejlede MFA/re-auth-forsøg inden for den konfigurerede periode.</summary>
    Task AlertFailedReAuthAsync(string userId, string ip, CancellationToken cancellationToken = default);

    /// <summary>Kaldes når en JIT-admin-rolle er blevet godkendt/aktiveret for en bruger.</summary>
    Task AlertJitRequestedAsync(string requesterId, string role, CancellationToken cancellationToken = default);

    /// <summary>Kaldes når en kritisk sikkerhedspolicy er blevet ændret.</summary>
    Task AlertPolicyChangedAsync(string userId, string policyName, CancellationToken cancellationToken = default);
}