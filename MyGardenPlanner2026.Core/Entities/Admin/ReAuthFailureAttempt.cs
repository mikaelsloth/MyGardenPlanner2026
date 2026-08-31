namespace MyGardenPlanner2026.Core.Entities.Admin;

/// <summary>
/// Enkeltstående, midlertidig registrering af et fejlet MFA/re-auth-forsøg (§4.2).
/// Implementerer BEVIDST ikke ISoftDelete: i modsætning til AuditLog (den historiske
/// sandhed, må aldrig ændres) er denne tabel en ren tælle-mekanisme, der fysisk ryddes
/// ved et efterfølgende korrekt login (se IReAuthFailureTracker.ClearFailuresAsync) —
/// den er derfor bevidst undtaget fra SoftDeleteInterceptor og AuditLoggingInterceptor,
/// som begge kun trigges på ISoftDelete-entities.
/// </summary>
public class ReAuthFailureAttempt
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public string UserId { get; set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string? IpAddress { get; set; }
}