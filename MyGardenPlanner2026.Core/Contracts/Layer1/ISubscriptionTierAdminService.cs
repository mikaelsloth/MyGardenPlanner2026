namespace MyGardenPlanner2026.Core.Contracts.Layer1;

public interface ISubscriptionTierAdminService
{
    Task<IReadOnlyList<SubscriptionTierAdminDto>> GetAllTiersAsync(CancellationToken cancellationToken = default);

    Task UpdateTierAsync(SubscriptionTierUpdateDto update, CancellationToken cancellationToken = default);
}