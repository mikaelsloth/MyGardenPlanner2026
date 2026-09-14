namespace MyGardenPlanner2026.Core.Entities.Gardens;

using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Registrerer en brugers aktive rettigheder og tilkøbskvoter for én have — enten
/// betalt (BillingCycle sat, IsTrial == false) eller en gratis Sandkasse-prøveperiode
/// (IsTrial == true). Se IOnboardingService.CreateSandboxGardenAsync/ProvisionPaidGardenAsync.
/// </summary>
public class UserEntitlement : ISoftDelete
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public string UserId { get; set; } = string.Empty;

    public Guid GardenId { get; set; }
    public Garden? Garden { get; set; }

    public GardenAccessLevel Layer { get; set; }
    public AccessCategory Category { get; set; }

    /// <summary>Antal tilkøbte ekstra bedforslag (jf. AddOnType.BedforslagNiveau2).</summary>
    public int ExtraBedProposalsCount { get; set; }

    /// <summary>Antal tilkøbte ekstra planlagte bede (jf. AddOnType.PlanlagteBedeNiveau3).</summary>
    public int ExtraPlannedBedsCount { get; set; }

    /// <summary>Antal tilkøbte Artefaktpakke A-enheder.</summary>
    public int ExtraCategoryAArtifactsCount { get; set; }

    /// <summary>Antal tilkøbte Artefaktpakke B-enheder.</summary>
    public int ExtraCategoryBArtifactsCount { get; set; }

    public BillingCycle BillingCycle { get; set; }

    /// <summary>Udløbstidspunkt for entitlementet. Null for evigtgyldige (Perpetual) køb.</summary>
    public DateTimeOffset? ValidToUtc { get; set; }

    /// <summary>True hvis entitlementet stammer fra en gratis Sandkasse-prøveperiode.</summary>
    public bool IsTrial { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}