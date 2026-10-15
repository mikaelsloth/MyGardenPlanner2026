namespace MyGardenPlanner2026.Tests.UI;

using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Fælles builders til subscription-/pricing-DTO'er i UI-tests.
/// Funktionslister på tiers sættes med <c>with { IncludedFeatures = …, FeatureLimits = … }</c>.
/// </summary>
public static class SubscriptionTestData
{
    /// <summary>
    /// Beregningsresultat for én have uden rabat. Default: 100 kr. uden tilkøb.
    /// </summary>
    public static PricingCalculationResultDto PricingResult(
        decimal total = 100m, decimal addOnsTotal = 0m, decimal basePricePerGarden = 100m) =>
        new(basePricePerGarden, 1m, 1.0m, basePricePerGarden, [], addOnsTotal, total);

    /// <summary>Tilkøb "Bedforslag (Niveau 2)": 180 / 15 / 450 kr.</summary>
    public static SubscriptionAddOnDto BedforslagAddOn(Guid? id = null) =>
        new(id ?? Guid.NewGuid(), AddOnType.BedforslagNiveau2, "Bedforslag (Niveau 2)",
            "Pakke med 2 bedforslag", 180m, 15m, 450m);

    /// <summary>Tilkøb "Artefaktpakke A": 48 / 4 / 120 kr.</summary>
    public static SubscriptionAddOnDto ArtefaktpakkeAAddOn(Guid? id = null) =>
        new(id ?? Guid.NewGuid(), AddOnType.ArtefaktpakkeA, "Artefaktpakke A",
            "+25 Planter / Materialer / Opgavelister", 48m, 4m, 120m);

    /// <summary>
    /// Abonnementsniveau. Default: Bed Designer / Editor, 100 kr. årligt, ikke fremhævet, uden funktionslister.
    /// </summary>
    public static SubscriptionTierDto Tier(
        GardenAccessLevel level = GardenAccessLevel.BedDesigner,
        AccessCategory category = AccessCategory.Editor,
        decimal price = 100m,
        bool isFeatured = false,
        string? name = null,
        string description = "Beskrivelse",
        BillingCycle billingCycle = BillingCycle.Annual,
        Guid? id = null) =>
        new(id ?? Guid.NewGuid(), level, category, name ?? $"{level} · {category}", description,
            price, billingCycle, isFeatured, [], new Dictionary<string, string>());

    /// <summary>Volumenrabat-trin (DTO). Default: 2-5 haver, 90 % af basispris.</summary>
    public static GardenVolumeDiscountTierDto VolumeDiscountTier(
        int minGardens = 2, int? maxGardens = 5, decimal priceMultiplier = 0.90m, Guid? id = null) =>
        new(id ?? Guid.NewGuid(), minGardens, maxGardens, priceMultiplier);

    /// <summary>De syv standardtrin (Prismatrix tabel 2), sorteret efter MinGardens.</summary>
    public static IReadOnlyList<GardenVolumeDiscountTierDto> DefaultVolumeDiscountTiers() =>
    [
        VolumeDiscountTier(1, 1, 1.00m),
        VolumeDiscountTier(2, 5, 0.90m),
        VolumeDiscountTier(6, 10, 0.80m),
        VolumeDiscountTier(11, 50, 0.70m),
        VolumeDiscountTier(51, 100, 0.60m),
        VolumeDiscountTier(101, 200, 0.50m),
        VolumeDiscountTier(201, null, 0.40m),
    ];
}