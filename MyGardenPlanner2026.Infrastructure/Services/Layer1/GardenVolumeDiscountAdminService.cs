namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;

public sealed partial class GardenVolumeDiscountAdminService(
    IAdminDbContextFactory contextFactory,
    IGardenVolumeDiscountCatalog defaultCatalog,
    ILogger<GardenVolumeDiscountAdminService> logger) : IGardenVolumeDiscountAdminService
{
    [LoggerMessage(EventId = 1055, Level = LogLevel.Information, Message = "Volumenrabat-trappe '{Id}' {Action} (MinGardens={MinGardens}).")]
    static partial void VolumeDiscountTierSaved(ILogger logger, Guid Id, string Action, int MinGardens);

    [LoggerMessage(EventId = 1056, Level = LogLevel.Information, Message = "Ingen volumenrabat-trappe fundet med Id '{Id}'.")]
    static partial void VolumeDiscountTierNotFound(ILogger logger, Guid Id);

    [LoggerMessage(EventId = 1057, Level = LogLevel.Information, Message = "Volumenrabat-trappe kunne ikke gemmes: der findes allerede en trappe der starter ved {MinGardens} haver.")]
    static partial void VolumeDiscountTierDuplicateMinGardens(ILogger logger, int MinGardens);

    [LoggerMessage(EventId = 1058, Level = LogLevel.Information, Message = "Volumenrabat-trappe '{Id}' slettet.")]
    static partial void VolumeDiscountTierDeleted(ILogger logger, Guid Id);

    [LoggerMessage(EventId = 1059, Level = LogLevel.Information, Message = "Volumenrabat-trapper nulstillet til standardkatalog.")]
    static partial void VolumeDiscountsResetToDefault(ILogger logger);

    public async Task<IReadOnlyList<GardenVolumeDiscountTierDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var tiers = await context.GardenVolumeDiscountTiers
            .OrderBy(t => t.MinGardens)
            .ToListAsync(cancellationToken);

        return [.. tiers.Select(ToDto)];
    }

    public async Task<GardenVolumeDiscountTierDto> SaveAsync(
        GardenVolumeDiscountTierUpsertDto upsert,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(upsert);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        GardenVolumeDiscountTier? tier;
        var isNew = upsert.Id is null;

        if (upsert.Id is Guid id)
        {
            tier = await context.GardenVolumeDiscountTiers
                .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (tier is null)
            {
                VolumeDiscountTierNotFound(logger, id);
                throw new InvalidOperationException($"Ingen volumenrabat-trappe fundet med Id {id}.");
            }

            tier.MinGardens = upsert.MinGardens;
            tier.MaxGardens = upsert.MaxGardens;
            tier.PriceMultiplier = upsert.PriceMultiplier;
        }
        else
        {
            tier = new GardenVolumeDiscountTier
            {
                MinGardens = upsert.MinGardens,
                MaxGardens = upsert.MaxGardens,
                PriceMultiplier = upsert.PriceMultiplier
            };
            await context.GardenVolumeDiscountTiers.AddAsync(tier, CancellationToken.None);
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            VolumeDiscountTierDuplicateMinGardens(logger, upsert.MinGardens);
            throw new InvalidOperationException(
                $"Der findes allerede en trappe der starter ved {upsert.MinGardens} haver.");
        }

        await RenumberDisplayOrderAsync(context, cancellationToken);

        VolumeDiscountTierSaved(logger, tier.Id, isNew ? "oprettet" : "opdateret", tier.MinGardens);

        return ToDto(tier);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var tier = await context.GardenVolumeDiscountTiers
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tier is null)
        {
            VolumeDiscountTierNotFound(logger, id);
            throw new InvalidOperationException($"Ingen volumenrabat-trappe fundet med Id {id}.");
        }

        context.GardenVolumeDiscountTiers.Remove(tier);
        await context.SaveChangesAsync(cancellationToken);

        await RenumberDisplayOrderAsync(context, cancellationToken);

        VolumeDiscountTierDeleted(logger, id);
    }

    public async Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        context.GardenVolumeDiscountTiers.RemoveRange(context.GardenVolumeDiscountTiers);
        await context.SaveChangesAsync(cancellationToken);

        await context.GardenVolumeDiscountTiers.AddRangeAsync(defaultCatalog.GetDefaultTiers(), CancellationToken.None);
        await context.SaveChangesAsync(cancellationToken);

        VolumeDiscountsResetToDefault(logger);
    }

    private static async Task RenumberDisplayOrderAsync(PlannerDbContext context, CancellationToken cancellationToken)
    {
        var ordered = await context.GardenVolumeDiscountTiers
            .OrderBy(t => t.MinGardens)
            .ToListAsync(cancellationToken);

        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].DisplayOrder = i + 1;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static GardenVolumeDiscountTierDto ToDto(GardenVolumeDiscountTier tier) =>
        new(tier.Id, tier.MinGardens, tier.MaxGardens, tier.PriceMultiplier);
}