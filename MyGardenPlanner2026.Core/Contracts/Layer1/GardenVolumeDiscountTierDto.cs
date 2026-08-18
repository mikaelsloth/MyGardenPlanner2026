namespace MyGardenPlanner2026.Core.Contracts.Layer1;

public sealed record GardenVolumeDiscountTierDto(
    int Id,
    int MinGardens,
    int? MaxGardens,
    decimal PriceMultiplier);