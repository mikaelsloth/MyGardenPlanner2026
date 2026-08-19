namespace MyGardenPlanner2026.Core.Contracts.Layer1;

public interface IGardenVolumeDiscountAdminService
{
    Task<IReadOnlyList<GardenVolumeDiscountTierDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<GardenVolumeDiscountTierDto> SaveAsync(GardenVolumeDiscountTierUpsertDto upsert, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}