namespace MyGardenPlanner2026.Infrastructure.Data.Seed;

using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Trappe-satser fra Prismatrix.md, tabel 2 ("Trappe-sats %"-kolonnen).
/// 201-500 haver tolkes som åbent interval (MaxGardens = null), bekræftet.
/// </summary>
public sealed class DefaultGardenVolumeDiscountCatalog : IGardenVolumeDiscountCatalog
{
    public IReadOnlyList<GardenVolumeDiscountTier> GetDefaultTiers() =>
    [
        new() { MinGardens = 1,   MaxGardens = 1,   PriceMultiplier = 1.00m, DisplayOrder = 1 },
        new() { MinGardens = 2,   MaxGardens = 5,   PriceMultiplier = 0.90m, DisplayOrder = 2 },
        new() { MinGardens = 6,   MaxGardens = 10,  PriceMultiplier = 0.80m, DisplayOrder = 3 },
        new() { MinGardens = 11,  MaxGardens = 50,  PriceMultiplier = 0.70m, DisplayOrder = 4 },
        new() { MinGardens = 51,  MaxGardens = 100, PriceMultiplier = 0.60m, DisplayOrder = 5 },
        new() { MinGardens = 101, MaxGardens = 200, PriceMultiplier = 0.50m, DisplayOrder = 6 },
        new() { MinGardens = 201, MaxGardens = null, PriceMultiplier = 0.40m, DisplayOrder = 7 }
    ];
}