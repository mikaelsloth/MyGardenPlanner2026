namespace MyGardenPlanner2026.Core.Entities.Gardens;

using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Midlertidig, server-persisteret "resume-handle" til en påbegyndt betalt
/// have-onboarding. Bruges til at overleve navigationen til /Account/Register eller
/// /Account/Login — disse er bevidst static SSR (se IdentityRedirectManager), hvilket
/// river den interaktive OnboardingStateContainer-cirkel ned. Ikke et bearer-token som
/// GardenInvitation: der er intet TokenHash, kun et opslags-Id, da draften blot
/// genoptager brugerens egen igangværende handling og ikke i sig selv beviser identitet.
/// Fysisk slettet ved forbrug eller udløb — ikke ISoftDelete (transient mekanisme, jf.
/// samme princip som ReAuthFailureAttempt).
/// </summary>
public class CheckoutDraft
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>Sat hvis draften oprettes af en allerede logget ind bruger; ellers null indtil provisionering.</summary>
    public string? UserId { get; set; }

    public string GardenName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public GardenAccessLevel Layer { get; set; }
    public AccessCategory Category { get; set; }
    public BillingCycle BillingCycle { get; set; }

    public Dictionary<Guid, int> AddOnQuantities { get; set; } = [];

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresUtc { get; set; }
}