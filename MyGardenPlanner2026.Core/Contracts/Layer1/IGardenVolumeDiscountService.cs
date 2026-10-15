namespace MyGardenPlanner2026.Core.Contracts.Layer1;

public interface IGardenVolumeDiscountService
{
    Task<IReadOnlyList<GardenVolumeDiscountTierDto>> GetAllTiersAsync(CancellationToken cancellationToken = default);
}