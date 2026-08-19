namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;

public sealed class SubscriptionAddOnAdminService(
    IDbContextFactory<PlannerDbContext> contextFactory,
    ISubscriptionAddOnCatalog defaultCatalog) : ISubscriptionAddOnAdminService
{
    public async Task<IReadOnlyList<SubscriptionAddOnDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var addOns = await context.SubscriptionAddOns
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync(cancellationToken);

        return [.. addOns.Select(ToDto)];
    }

    public async Task<SubscriptionAddOnDto> SaveAsync(
        SubscriptionAddOnUpsertDto upsert,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(upsert);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var duplicateTypeExists = await context.SubscriptionAddOns
            .AnyAsync(a => a.Type == upsert.Type && a.Id != (upsert.Id ?? 0), cancellationToken);

        if (duplicateTypeExists)
        {
            throw new InvalidOperationException($"Der findes allerede et tilkøb med typen '{upsert.Type}'.");
        }

        SubscriptionAddOn addOn;

        if (upsert.Id is int id)
        {
            addOn = await context.SubscriptionAddOns
                .SingleOrDefaultAsync(a => a.Id == id, cancellationToken)
                ?? throw new InvalidOperationException($"Intet tilkøb fundet med Id {id}.");

            addOn.Type = upsert.Type;
            addOn.Name = upsert.Name;
            addOn.UnitDescription = upsert.UnitDescription;
            addOn.AnnualPrice = upsert.AnnualPrice;
            addOn.MonthlyPrice = upsert.MonthlyPrice;
            addOn.PerpetualPrice = upsert.PerpetualPrice;
        }
        else
        {
            var maxDisplayOrder = await context.SubscriptionAddOns
                .Select(a => (int?)a.DisplayOrder)
                .MaxAsync(cancellationToken) ?? 0;

            addOn = new SubscriptionAddOn
            {
                Type = upsert.Type,
                Name = upsert.Name,
                UnitDescription = upsert.UnitDescription,
                AnnualPrice = upsert.AnnualPrice,
                MonthlyPrice = upsert.MonthlyPrice,
                PerpetualPrice = upsert.PerpetualPrice,
                DisplayOrder = maxDisplayOrder + 1
            };
            context.SubscriptionAddOns.Add(addOn);
        }

        await context.SaveChangesAsync(cancellationToken);

        return ToDto(addOn);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var addOn = await context.SubscriptionAddOns
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Intet tilkøb fundet med Id {id}.");

        context.SubscriptionAddOns.Remove(addOn);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        context.SubscriptionAddOns.RemoveRange(context.SubscriptionAddOns);
        await context.SaveChangesAsync(cancellationToken);

        context.SubscriptionAddOns.AddRange(defaultCatalog.GetDefaultAddOns());
        await context.SaveChangesAsync(cancellationToken);
    }

    private static SubscriptionAddOnDto ToDto(SubscriptionAddOn addOn) =>
        new(addOn.Id, addOn.Type, addOn.Name, addOn.UnitDescription, addOn.AnnualPrice, addOn.MonthlyPrice, addOn.PerpetualPrice);
}