namespace MyGardenPlanner2026.Core.Contracts.Layer1;

using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// UI-visningskontrakt for et basisabonnement.
/// Bygges via SubscriptionTierMapper fra en SubscriptionTier + valgt BillingCycle.
/// </summary>
public sealed record SubscriptionTierDto(
    int Id,
    GardenAccessLevel Level,
    AccessCategory AccessCategory,
    string Name,
    string Description,
    decimal Price,
    BillingCycle BillingCycle,
    bool IsFeatured,
    IReadOnlyList<string> IncludedFeatures,
    IReadOnlyDictionary<string, string> FeatureLimits);