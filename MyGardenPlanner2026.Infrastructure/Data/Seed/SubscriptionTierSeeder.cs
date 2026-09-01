namespace MyGardenPlanner2026.Infrastructure.Data.Seed;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Layer1;

/// <summary>
/// Idempotent seeder: indsætter kun default-tiers hvis tabellen er tom.
/// Erstattes senere af admin-CRUD; seederen kan da nedgraderes til kun dev/test-brug.
/// </summary>
public sealed class SubscriptionTierSeeder(
    IAdminDbContextFactory contextFactory,
    ISubscriptionTierCatalog catalog)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        if (await context.SubscriptionTiers.AnyAsync(cancellationToken))
        {
            return;
        }

        await context.SubscriptionTiers.AddRangeAsync(catalog.GetDefaultTiers(), CancellationToken.None);
        await context.SaveChangesAsync(cancellationToken);
    }
}