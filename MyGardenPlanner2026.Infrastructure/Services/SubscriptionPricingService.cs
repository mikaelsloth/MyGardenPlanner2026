namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Vælger, pr. GardenAccessLevel, den flaggede (IsFeatured) tier med højeste AccessCategory.
/// </summary>
public sealed class SubscriptionPricingService(IDbContextFactory<PlannerDbContext> contextFactory)
    : ISubscriptionPricingService
{
    public async Task<IReadOnlyList<SubscriptionTierDto>> GetFeaturedTiersAsync(
        BillingCycle cycle,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var featuredTiers = await context.SubscriptionTiers
            .Where(t => t.IsFeatured)
            .ToListAsync(cancellationToken);

        return [.. featuredTiers
            .GroupBy(t => t.Level)
            .Select(g => g.OrderByDescending(t => t.AccessCategory).First())
            .OrderBy(t => t.Level)
            .Select(t => t.ToDto(cycle))];
    }
}