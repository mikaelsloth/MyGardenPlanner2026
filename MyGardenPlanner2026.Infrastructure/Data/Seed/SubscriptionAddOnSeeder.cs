namespace MyGardenPlanner2026.Infrastructure.Data.Seed;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Layer1;

public sealed class SubscriptionAddOnSeeder(
    IDbContextFactory<PlannerDbContext> contextFactory,
    ISubscriptionAddOnCatalog catalog)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        if (await context.SubscriptionAddOns.AnyAsync(cancellationToken))
        {
            return;
        }

        context.SubscriptionAddOns.AddRange(catalog.GetDefaultAddOns());
        await context.SaveChangesAsync(cancellationToken);
    }
}