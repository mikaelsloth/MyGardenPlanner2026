namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;

public sealed partial class SubscriptionAddOnAdminService(
    IAdminDbContextFactory contextFactory,
    ISubscriptionAddOnCatalog defaultCatalog,
    ILogger<SubscriptionAddOnAdminService> logger) : ISubscriptionAddOnAdminService
{
    [LoggerMessage(EventId = 1060, Level = LogLevel.Information, Message = "Tilkøb '{Id}' ({Type}) {Action}.")]
    static partial void AddOnSaved(ILogger logger, Guid Id, AddOnType Type, string Action);

    [LoggerMessage(EventId = 1061, Level = LogLevel.Information, Message = "Tilkøb kunne ikke gemmes: der findes allerede et tilkøb med typen '{Type}'.")]
    static partial void AddOnDuplicateType(ILogger logger, AddOnType Type);

    [LoggerMessage(EventId = 1062, Level = LogLevel.Information, Message = "Intet tilkøb fundet med Id '{Id}'.")]
    static partial void AddOnNotFound(ILogger logger, Guid Id);

    [LoggerMessage(EventId = 1063, Level = LogLevel.Information, Message = "Tilkøb '{Id}' slettet.")]
    static partial void AddOnDeleted(ILogger logger, Guid Id);

    [LoggerMessage(EventId = 1064, Level = LogLevel.Information, Message = "Tilkøbsmoduler nulstillet til standardkatalog.")]
    static partial void AddOnsResetToDefault(ILogger logger);

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
            .AnyAsync(a => a.Type == upsert.Type && a.Id != (upsert.Id ?? Guid.Empty), cancellationToken);

        if (duplicateTypeExists)
        {
            AddOnDuplicateType(logger, upsert.Type);
            throw new InvalidOperationException($"Der findes allerede et tilkøb med typen '{upsert.Type}'.");
        }

        SubscriptionAddOn? addOn;
        var isNew = upsert.Id is null;

        if (upsert.Id is Guid id)
        {
            addOn = await context.SubscriptionAddOns
                .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);

            if (addOn is null)
            {
                AddOnNotFound(logger, id);
                throw new InvalidOperationException($"Intet tilkøb fundet med Id {id}.");
            }

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
            await context.SubscriptionAddOns.AddAsync(addOn, CancellationToken.None);
        }

        await context.SaveChangesAsync(cancellationToken);

        AddOnSaved(logger, addOn.Id, addOn.Type, isNew ? "oprettet" : "opdateret");

        return ToDto(addOn);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var addOn = await context.SubscriptionAddOns
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (addOn is null)
        {
            AddOnNotFound(logger, id);
            throw new InvalidOperationException($"Intet tilkøb fundet med Id {id}.");
        }

        context.SubscriptionAddOns.Remove(addOn);
        await context.SaveChangesAsync(cancellationToken);

        AddOnDeleted(logger, id);
    }

    public async Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        context.SubscriptionAddOns.RemoveRange(context.SubscriptionAddOns);
        await context.SaveChangesAsync(cancellationToken);

        await context.SubscriptionAddOns.AddRangeAsync(defaultCatalog.GetDefaultAddOns(), CancellationToken.None);
        await context.SaveChangesAsync(cancellationToken);

        AddOnsResetToDefault(logger);
    }

    private static SubscriptionAddOnDto ToDto(SubscriptionAddOn addOn) =>
        new(addOn.Id, addOn.Type, addOn.Name, addOn.UnitDescription, addOn.AnnualPrice, addOn.MonthlyPrice, addOn.PerpetualPrice);
}