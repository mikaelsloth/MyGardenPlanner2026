namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Henter persisterede SubscriptionAddOn-rækker (med reelle DB-genererede Id'er).
/// Bruges af UI-komponenter i stedet for ISubscriptionAddOnCatalog, som kun
/// leverer seed-data uden Id'er.
/// </summary>
public sealed class SubscriptionAddOnService(
    IDbContextFactory<PlannerDbContext> contextFactory) : ISubscriptionAddOnService
{
    public async Task<IReadOnlyList<SubscriptionAddOnDto>> GetAllAddOnsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var addOns = await context.SubscriptionAddOns
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync(cancellationToken);

        return [.. addOns.Select(a => new SubscriptionAddOnDto(
            a.Id, a.Type, a.Name, a.UnitDescription, a.AnnualPrice, a.MonthlyPrice, a.PerpetualPrice))];
    }
}