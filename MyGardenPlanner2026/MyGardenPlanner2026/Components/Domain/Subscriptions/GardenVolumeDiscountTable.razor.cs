namespace MyGardenPlanner2026.Components.Domain.Subscriptions;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Layer1;

public partial class GardenVolumeDiscountTable
{
    [Inject]
    private IGardenVolumeDiscountCatalog Catalog { get; set; } = default!;

    private IReadOnlyList<GardenVolumeDiscountTierDto>? tiers;

    protected override void OnInitialized()
    {
        tiers = [.. Catalog.GetDefaultTiers()
            .OrderBy(t => t.MinGardens)
            .Select(t => new GardenVolumeDiscountTierDto(t.Id, t.MinGardens, t.MaxGardens, t.PriceMultiplier))];
    }

    private static string RangeLabel(GardenVolumeDiscountTierDto tier) =>
        tier.MaxGardens is null
            ? $"{tier.MinGardens}+ haver"
            : tier.MinGardens == tier.MaxGardens
                ? $"{tier.MinGardens} have"
                : $"{tier.MinGardens} - {tier.MaxGardens} haver";
}