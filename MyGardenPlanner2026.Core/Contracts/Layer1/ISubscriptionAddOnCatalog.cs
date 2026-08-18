namespace MyGardenPlanner2026.Core.Contracts.Layer1;

using MyGardenPlanner2026.Core.Entities.Layer1;

public interface ISubscriptionAddOnCatalog
{
    IReadOnlyList<SubscriptionAddOn> GetDefaultAddOns();
}