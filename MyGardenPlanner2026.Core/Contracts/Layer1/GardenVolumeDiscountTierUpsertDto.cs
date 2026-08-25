namespace MyGardenPlanner2026.Core.Contracts.Layer1;

public sealed record GardenVolumeDiscountTierUpsertDto(
    Guid? Id,
    int MinGardens,
    int? MaxGardens,
    decimal PriceMultiplier);