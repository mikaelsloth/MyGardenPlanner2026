namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Udsteder og validerer korttidsholdbare, signerede tokens der beviser, at brugeren
/// netop har gennemført step-up re-autentificering i sin Blazor Server-circuit, til
/// brug for AuditLog-eksport-endpointet. Nødvendigt fordi IReAuthenticationService er
/// circuit-scoped og derfor ikke direkte tilgængeligt fra en almindelig HTTP GET-request
/// mod minimal API-endpointet (som kører i sit eget DI-scope).
/// </summary>
public interface IAuditLogExportTokenService
{
    /// <summary>Udsteder et token bundet til den angivne bruger, tidsstemplet med nutid.</summary>
    string IssueToken(string userId);

    /// <summary>
    /// Validerer tokenet: signatur, at det matcher <paramref name="userId"/>, og at det
    /// ikke er ældre end den konfigurerede ReAuthenticationPolicy.MaxAgeMinutes.
    /// <paramref name="failureReason"/> er sat ved false, til brug i fejlbeskeder.
    /// </summary>
    bool TryValidateToken(string token, string userId, out string? failureReason);
}