namespace MyGardenPlanner2026.Infrastructure.Data.Seed;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Layer1;

public sealed class GardenVolumeDiscountSeeder(
    IAdminDbContextFactory contextFactory,
    IGardenVolumeDiscountCatalog catalog)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        if (await context.GardenVolumeDiscountTiers.AnyAsync(cancellationToken))
        {
            return;
        }

        await context.GardenVolumeDiscountTiers.AddRangeAsync(catalog.GetDefaultTiers(), CancellationToken.None);
        await context.SaveChangesAsync(cancellationToken);
    }
}