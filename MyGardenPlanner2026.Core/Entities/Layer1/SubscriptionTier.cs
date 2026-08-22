namespace MyGardenPlanner2026.Core.Entities.Layer1;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Basisabonnement for én kombination af GardenAccessLevel og AccessCategory.
/// Svarer til én række i Prismatrix.md, tabel 1. Priser er DKK ekskl. moms.
/// </summary>
public class SubscriptionTier : ISoftDelete
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

    public List<string> IncludedFeatures { get; set; } = [];
    public Dictionary<string, string> FeatureLimits { get; set; } = [];

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}