namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Redigerer eksisterende SubscriptionTier-rækker (12 stk., Level × AccessCategory).
/// Opretter ALDRIG nye rækker — det er seederens ansvar.
/// </summary>
public sealed partial class SubscriptionTierAdminService(
    IAdminDbContextFactory contextFactory,
    ILogger<SubscriptionTierAdminService> logger) : ISubscriptionTierAdminService
{
    [LoggerMessage(EventId = 1053, Level = LogLevel.Information, Message = "Basispris for abonnement '{Id}' opdateret (Annual={AnnualPrice}, Monthly={MonthlyPrice}, Perpetual={PerpetualPrice}).")]
    static partial void TierUpdated(ILogger logger, Guid Id, decimal AnnualPrice, decimal MonthlyPrice, decimal PerpetualPrice);

    [LoggerMessage(EventId = 1054, Level = LogLevel.Information, Message = "Intet abonnement fundet med Id '{Id}' ved prisopdatering.")]
    static partial void TierUpdateNotFound(ILogger logger, Guid Id);

    public async Task<IReadOnlyList<SubscriptionTierAdminDto>> GetAllTiersAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var tiers = await context.SubscriptionTiers
            .OrderBy(t => t.Level)
            .ThenByDescending(t => t.AccessCategory)
            .ToListAsync(cancellationToken);

        return [.. tiers.Select(t => new SubscriptionTierAdminDto(
            t.Id, t.Level, t.AccessCategory, t.Name, t.AnnualPrice, t.MonthlyPrice, t.PerpetualPrice))];
    }

    public async Task UpdateTierAsync(
        SubscriptionTierUpdateDto update,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var tier = await context.SubscriptionTiers
            .SingleOrDefaultAsync(t => t.Id == update.Id, cancellationToken);

        if (tier is null)
        {
            TierUpdateNotFound(logger, update.Id);
            throw new InvalidOperationException($"Intet abonnement fundet med Id {update.Id}.");
        }

        tier.AnnualPrice = update.AnnualPrice;
        tier.MonthlyPrice = update.MonthlyPrice;
        tier.PerpetualPrice = update.PerpetualPrice;

        await context.SaveChangesAsync(cancellationToken);

        TierUpdated(logger, tier.Id, tier.AnnualPrice, tier.MonthlyPrice, tier.PerpetualPrice);
    }
}