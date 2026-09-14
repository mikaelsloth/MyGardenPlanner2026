namespace MyGardenPlanner2026.Core.Entities.Gardens;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Invitation til at blive medlem af en have. Det rå invitations-token deles KUN med
/// modtageren via linket på oprettelsestidspunktet (se IInvitationTokenService) — der
/// persisteres udelukkende en SHA-256 hash (TokenHash), så et databaselæk aldrig
/// afslører et brugbart token.
/// </summary>
public class GardenInvitation : ISoftDelete
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public Guid GardenId { get; set; }
    public Garden? Garden { get; set; }

    public string InvitedByUserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>SHA-256 hash (64 lowercase hex-tegn) af det rå invitations-token.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public GardenAccessLevel TargetLayer { get; set; }
    public AccessCategory TargetCategory { get; set; }

    /// <summary>
    /// Loft for hvilket niveau den inviterede maksimalt må opgradere sig selv til via
    /// AllowSelfUpgrade. Kan aldrig overstige den inviterendes egne rettigheder —
    /// håndhæves i IOnboardingService.CreateInvitationAsync.
    /// </summary>
    public GardenAccessLevel MaxAllowedLayer { get; set; }
    public AccessCategory MaxAllowedCategory { get; set; }

    /// <summary>True hvis denne invitation trækker på inviterendes 1 gratis invitationskvote.</summary>
    public bool IsFreeSlot { get; set; }

    /// <summary>True hvis den inviterede selv må tilkøbe/opgradere op til MaxAllowedLayer/Category.</summary>
    public bool AllowSelfUpgrade { get; set; }

    public DateTimeOffset ExpiresUtc { get; set; }
    public bool IsAccepted { get; set; }
    public bool IsRevoked { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}