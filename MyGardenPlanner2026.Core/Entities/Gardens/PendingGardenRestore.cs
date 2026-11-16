namespace MyGardenPlanner2026.Core.Entities.Gardens;

/// <summary>
/// "Gendannelsesticket" for en have, der er slettet ved kontosletning (R10.1b). Haven er
/// soft-slettet og bevares, til ExpiresUtc, hvorefter den slettes fysisk. Matches mod en
/// senere registreret bruger via OwnerEmailHash (SHA-256 af normaliseret e-mail, se
/// RestoreEmailHasher) — ingen e-mail i klartekst. Fysisk slettet ved gendannelse eller udløb
/// (og via cascade, hvis haven slettes fysisk) — ikke ISoftDelete, jf. CheckoutDraft.
/// </summary>
public class PendingGardenRestore
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public Guid GardenId { get; set; }

    /// <summary>SHA-256 hash (64 lowercase hex-tegn) af ejerens normaliserede e-mail.</summary>
    public string OwnerEmailHash { get; set; } = string.Empty;

    /// <summary>Ejerens tidligere bruger-ID (til de beholdte medlemskabs-/entitlement-rækker).</summary>
    public string FormerUserId { get; set; } = string.Empty;

    public DateTimeOffset DeletedAtUtc { get; set; }
    public DateTimeOffset ExpiresUtc { get; set; }
}