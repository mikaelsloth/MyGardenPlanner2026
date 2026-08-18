namespace MyGardenPlanner2026.Core.Contracts.Layer1;

using MyGardenPlanner2026.Core.Entities.Layer1;

public interface ISubscriptionPricingService
{
    Task<IReadOnlyList<SubscriptionTierDto>> GetFeaturedTiersAsync(
        BillingCycle cycle,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubscriptionTierDto>> GetAllTiersAsync(
        BillingCycle cycle,
        CancellationToken cancellationToken = default);
}