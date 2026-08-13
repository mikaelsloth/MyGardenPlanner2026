namespace MyGardenPlanner2026.Infrastructure.Data.Seed;

using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Hardkodet katalog baseret på Prismatrix.md, tabel 1.
/// Editor-kategorien er flagget IsFeatured for hvert Lag (bruges af landingssiden).
/// </summary>
public sealed class DefaultSubscriptionTierCatalog : ISubscriptionTierCatalog
{
    public IReadOnlyList<SubscriptionTier> GetDefaultTiers() =>
    [
        // Lag 1: Have Arkitekt
        Create(GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, 336m, 28m, 840m, false, 1,
            "Adgang til at oprette og redigere havetegninger.",
            ["Ubegrænsede havetegninger", "Fuld administratoradgang til stamdata"]),
        Create(GardenAccessLevel.HaveArkitekt, AccessCategory.Editor, 168m, 14m, 420m, true, 2,
            "Adgang til at oprette og redigere havetegninger.",
            ["Opret og redigér havetegninger", "Fuld adgang til stamdata (planter, materialer, butikker)"]),
        Create(GardenAccessLevel.HaveArkitekt, AccessCategory.ViewerPlus, 84m, 7m, 210m, false, 3,
            "Adgang til at oprette og redigere havetegninger.",
            ["Se op til 5 dokumenter pr. artefakt"]),
        Create(GardenAccessLevel.HaveArkitekt, AccessCategory.Viewer, 42m, 3.5m, 105m, false, 4,
            "Adgang til at oprette og redigere havetegninger.",
            ["Se op til 1 dokument pr. artefakt"]),

        // Lag 2: Bed Designer
        Create(GardenAccessLevel.BedDesigner, AccessCategory.Administrator, 240m, 20m, 600m, false, 5,
            "Adgang til at oprette bedforslag i en eksisterende have.",
            ["2 bedforslag pr. have", "Op til 25 bede pr. forslag"],
            ("Bedforslag", "2"), ("Bede pr. forslag", "25")),
        Create(GardenAccessLevel.BedDesigner, AccessCategory.Editor, 120m, 10m, 300m, true, 6,
            "Adgang til at oprette bedforslag i en eksisterende have.",
            ["2 bedforslag pr. have", "Op til 25 bede pr. forslag"],
            ("Bedforslag", "2"), ("Bede pr. forslag", "25")),
        Create(GardenAccessLevel.BedDesigner, AccessCategory.ViewerPlus, 60m, 5m, 150m, false, 7,
            "Adgang til at oprette bedforslag i en eksisterende have.",
            ["Se op til 5 dokumenter pr. artefakt"]),
        Create(GardenAccessLevel.BedDesigner, AccessCategory.Viewer, 30m, 2.5m, 75m, false, 8,
            "Adgang til at oprette bedforslag i en eksisterende have.",
            ["Se op til 1 dokument pr. artefakt"]),

        // Lag 3: Planlægger
        Create(GardenAccessLevel.Planlaegger, AccessCategory.Administrator, 192m, 16m, 480m, false, 9,
            "Adgang til at oprette konkrete bede, planlægge indhold og følge fremskridt.",
            ["50 planlagte bede pr. have"],
            ("Planlagte bede", "50")),
        Create(GardenAccessLevel.Planlaegger, AccessCategory.Editor, 96m, 8m, 240m, true, 10,
            "Adgang til at oprette konkrete bede, planlægge indhold og følge fremskridt.",
            ["50 planlagte bede pr. have"],
            ("Planlagte bede", "50")),
        Create(GardenAccessLevel.Planlaegger, AccessCategory.ViewerPlus, 48m, 4m, 120m, false, 11,
            "Adgang til at oprette konkrete bede, planlægge indhold og følge fremskridt.",
            ["Se op til 5 dokumenter pr. artefakt"]),
        Create(GardenAccessLevel.Planlaegger, AccessCategory.Viewer, 24m, 2m, 60m, false, 12,
            "Adgang til at oprette konkrete bede, planlægge indhold og følge fremskridt.",
            ["Se op til 1 dokument pr. artefakt"])
    ];

    private static SubscriptionTier Create(
        GardenAccessLevel level,
        AccessCategory category,
        decimal annual,
        decimal monthly,
        decimal perpetual,
        bool isFeatured,
        int displayOrder,
        string description,
        List<string> includedFeatures,
        params (string Key, string Value)[] featureLimits) => new()
        {
            Level = level,
            AccessCategory = category,
            Name = $"{level.ToDisplayName()} · {category}",
            Description = description,
            AnnualPrice = annual,
            MonthlyPrice = monthly,
            PerpetualPrice = perpetual,
            IsFeatured = isFeatured,
            DisplayOrder = displayOrder,
            IncludedFeatures = includedFeatures,
            FeatureLimits = featureLimits.ToDictionary(f => f.Key, f => f.Value)
        };
}