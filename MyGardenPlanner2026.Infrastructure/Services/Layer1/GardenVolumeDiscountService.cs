namespace MyGardenPlanner2026.Infrastructure.Services.Layer1;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Henter persisterede volumenrabat-trin (de, admin redigerer) til offentlige sider. Bruges i stedet for
/// IGardenVolumeDiscountCatalog, som kun leverer seed-data, og for IGardenVolumeDiscountAdminService,
/// som bruger admin-databaseforbindelsen.
/// </summary>
public sealed class GardenVolumeDiscountService(
    IDbContextFactory<PlannerDbContext> contextFactory) : IGardenVolumeDiscountService
{
    public async Task<IReadOnlyList<GardenVolumeDiscountTierDto>> GetAllTiersAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var tiers = await context.GardenVolumeDiscountTiers
            .OrderBy(t => t.MinGardens)
            .ToListAsync(cancellationToken);

        return [.. tiers.Select(t => new GardenVolumeDiscountTierDto(
            t.Id, t.MinGardens, t.MaxGardens, t.PriceMultiplier))];
    }
}