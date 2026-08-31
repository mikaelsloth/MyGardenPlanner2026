namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Sporer fejlede MFA/re-auth-forsøg pr. bruger over et konfigurerbart glidende vindue
/// (§4.2, default 5 forsøg / 2 dage). Udsender ÉN ISecurityAlertService-alarm, når
/// tærsklen nås — ikke for hvert efterfølgende forsøg derudover, for at undgå spam.
/// Tælleren nulstilles KUN eksplicit via ClearFailuresAsync (kaldes ved korrekt login),
/// ikke automatisk over tid — enkelte forsøg forældes dog ud af det glidende vindue.
/// </summary>
public interface IReAuthFailureTracker
{
    /// <summary>
    /// Registrerer et fejlet forsøg. Returnerer true hvis dette forsøg netop krydsede
    /// tærsklen (og dermed udløste en sikkerhedsalarm).
    /// </summary>
    Task<bool> RecordFailureAsync(string userId, string? ipAddress, CancellationToken cancellationToken = default);

    /// <summary>Rydder alle registrerede fejl for brugeren (kaldes ved korrekt login).</summary>
    Task ClearFailuresAsync(string userId, CancellationToken cancellationToken = default);
}