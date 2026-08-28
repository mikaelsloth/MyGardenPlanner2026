namespace MyGardenPlanner2026.Components.Domain.Subscriptions;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Layer1;

public partial class PricingFeatureMatrix
{
    [Parameter, EditorRequired]
    public List<SubscriptionTierDto> Tiers { get; set; } = [];

    private IReadOnlyList<string> FeatureKeys =>
        [.. Tiers
            .SelectMany(t => t.FeatureLimits.Keys)
            .Distinct()
            .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)];

    private static string GetLimitValue(SubscriptionTierDto tier, string featureKey) =>
        tier.FeatureLimits.TryGetValue(featureKey, out var value) ? value : "–";
}