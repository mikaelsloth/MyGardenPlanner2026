namespace MyGardenPlanner2026.Core.Entities.Gardens;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// En have oprettet af en bruger — enten en betalt have eller en Sandkasse-have fra en
/// gratis prøveperiode (se IOnboardingService). Bede-navigation er bevidst udeladt indtil
/// Layer 2/3-domænet (jf. Architecture.md) er designet.
/// </summary>
public class Garden : ISoftDelete
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Placeholder-reference til havens adresse (se Address). Udbygges senere.</summary>
    public Guid? AddressId { get; set; }

    /// <summary>Placeholder-reference til havens kontaktperson (se Contact). Udbygges senere.</summary>
    public Guid? ContactId { get; set; }

    /// <summary>
    /// Arkiveret (forretningsstatus) — adskilt fra IsDeleted. Arkiverede haver vægtes
    /// forskelligt i volumenrabat-beregningen afhængigt af AccessCategory (se
    /// PricingCalculatorService).
    /// </summary>
    public bool Archived { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public ICollection<GardenMembership> Members { get; set; } = [];

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}