namespace MyGardenPlanner2026.Core.Contracts.Account;

/// <summary>
/// Nulstiller DeletedByEmail på soft-slettede rækker i admin-schema (katalog, sikkerhedspolitikker,
/// JIT-anmodninger), hvor den slettede bruger er sletteren. Bruger-ID bevares (pseudonymt).
/// Kører efter kontosletningens commit og er best-effort: fejl logges og kaster ikke.
/// </summary>
public interface IAdminSoftDeleteActorScrubber
{
    /// <summary>Returnerer antal rensede rækker (0 ved fejl).</summary>
    Task<int> ClearActorEmailAsync(string userId, CancellationToken cancellationToken = default);
}