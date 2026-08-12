namespace MyGardenPlanner2026.Core.Entities.Layer1;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Basisabonnement for én kombination af GardenAccessLevel og AccessCategory.
/// Svarer til én række i Prismatrix.md, tabel 1. Priser er DKK ekskl. moms.
/// </summary>
public class SubscriptionTier
{
    public int Id { get; set; }

    public GardenAccessLevel Level { get; set; }
    public AccessCategory AccessCategory { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public decimal AnnualPrice { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal PerpetualPrice { get; set; }

    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }

    /// <summary>Danske feature-tekster til checkliste, i visningsrækkefølge.</summary>
    public List<string> IncludedFeatures { get; set; } = [];

    /// <summary>
    /// Kvotegrænser for artefakter på dette Lag (fx "Planlagte bede" -> "50").
    /// Ens på tværs af AccessCategory for samme Level i v1 (bevidst denormaliseret).
    /// </summary>
    public Dictionary<string, string> FeatureLimits { get; set; } = [];
}