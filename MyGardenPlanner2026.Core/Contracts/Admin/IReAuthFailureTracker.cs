namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Sporer fejlede MFA/re-auth-forsøg pr. bruger over et konfigurerbart glidende vindue
/// (§4.2, default 5 forsøg / 2 dage). Udsender en ISecurityAlertService-alarm, hver gang
/// antallet af fejl i vinduet er et multiplum af tærsklen (5, 10, 15 …). Fejl, der forældes
/// ud af vinduet, tæller ikke med. Tælleren nulstilles KUN eksplicit via ClearFailuresAsync
/// (kaldes ved korrekt login).
/// </summary>
public interface IReAuthFailureTracker
{
    /// <summary>
    /// Registrerer et fejlet forsøg. Returnerer true hvis dette forsøg udløste en
    /// sikkerhedsalarm (antal fejl i vinduet er et multiplum af tærsklen).
    /// </summary>
    Task<bool> RecordFailureAsync(string userId, string? ipAddress, CancellationToken cancellationToken = default);

    /// <summary>Rydder alle registrerede fejl for brugeren (kaldes ved korrekt login).</summary>
    Task ClearFailuresAsync(string userId, CancellationToken cancellationToken = default);
}