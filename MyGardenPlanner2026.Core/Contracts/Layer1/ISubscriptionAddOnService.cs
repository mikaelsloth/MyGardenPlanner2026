namespace MyGardenPlanner2026.Core.Contracts.Layer1;

public interface ISubscriptionAddOnService
{
    Task<IReadOnlyList<SubscriptionAddOnDto>> GetAllAddOnsAsync(CancellationToken cancellationToken = default);
}