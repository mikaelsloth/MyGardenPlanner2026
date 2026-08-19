namespace MyGardenPlanner2026.Core.Contracts.Layer1;

public interface ISubscriptionAddOnAdminService
{
    Task<IReadOnlyList<SubscriptionAddOnDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SubscriptionAddOnDto> SaveAsync(SubscriptionAddOnUpsertDto upsert, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}