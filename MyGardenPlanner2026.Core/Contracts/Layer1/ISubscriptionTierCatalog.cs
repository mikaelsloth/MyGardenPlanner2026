namespace MyGardenPlanner2026.Core.Contracts.Layer1;

using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Kilde til default SubscriptionTier-data brugt til at seede en tom database.
/// En fremtidig admin-side kan erstatte implementeringen uden at røre seederen.
/// </summary>
public interface ISubscriptionTierCatalog
{
    IReadOnlyList<SubscriptionTier> GetDefaultTiers();
}