namespace MyGardenPlanner2026.Core.Contracts.Layer1;

using MyGardenPlanner2026.Core.Entities.Layer1;

public static class SubscriptionTierMapper
{
    public static SubscriptionTierDto ToDto(this SubscriptionTier tier, BillingCycle cycle)
    {
        ArgumentNullException.ThrowIfNull(tier);

        var price = cycle switch
        {
            BillingCycle.Annual => tier.AnnualPrice,
            BillingCycle.Monthly => tier.MonthlyPrice,
            BillingCycle.Perpetual => tier.PerpetualPrice,
            _ => throw new ArgumentOutOfRangeException(nameof(cycle))
        };

        return new SubscriptionTierDto(
            tier.Id,
            tier.Level,
            tier.AccessCategory,
            tier.Name,
            tier.Description,
            price,
            cycle,
            tier.IsFeatured,
            tier.IncludedFeatures,
            tier.FeatureLimits);
    }
}