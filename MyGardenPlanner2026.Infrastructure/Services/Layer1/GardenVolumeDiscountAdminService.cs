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

    [LoggerMessage(EventId = 1151, Level = LogLevel.Information, Message = "Volumenrabat-trappe afvist: max ({MaxGardens}) er mindre end min ({MinGardens}).")]
    static partial void VolumeDiscountTierMaxBelowMin(ILogger logger, int MinGardens, int MaxGardens);

    [LoggerMessage(EventId = 1152, Level = LogLevel.Information, Message = "Volumenrabat-trappe fra {MinGardens} haver afvist: overlapper med trappen der starter ved {OtherMinGardens}.")]
    static partial void VolumeDiscountTierOverlap(ILogger logger, int MinGardens, int OtherMinGardens);

    [LoggerMessage(EventId = 1153, Level = LogLevel.Information, Message = "Volumenrabat-trappe fra {MinGardens} haver afvist: åben øvre grænse er kun tilladt på øverste trin.")]
    static partial void VolumeDiscountTierOpenNotTop(ILogger logger, int MinGardens);

    [LoggerMessage(EventId = 1154, Level = LogLevel.Information, Message = "Volumenrabat-trappen fra {PreviousMinGardens} haver er lukket automatisk ved {NewMaxGardens} haver.")]
    static partial void VolumeDiscountPreviousTopClosed(ILogger logger, int PreviousMinGardens, int NewMaxGardens);

    [LoggerMessage(EventId = 1155, Level = LogLevel.Information, Message = "Volumenrabat-trappen fra {MinGardens} haver er genåbnet (ingen øvre grænse) efter sletning af øverste trin.")]
    static partial void VolumeDiscountNewTopReopened(ILogger logger, int MinGardens);

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

        if (await DuplicateMinGardensExistsAsync(context, upsert.MinGardens, upsert.Id, cancellationToken))
        {
            VolumeDiscountTierDuplicateMinGardens(logger, upsert.MinGardens);
            throw new InvalidOperationException(DuplicateMessage(upsert.MinGardens));
        }

        var closedPreviousTop = await ValidateRangeAndClosePreviousTopAsync(context, upsert, cancellationToken);

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
        catch (DbUpdateException ex)
        {
            if (await DuplicateMinGardensExistsAsync(context, upsert.MinGardens, tier.Id, cancellationToken))
            {
                VolumeDiscountTierDuplicateMinGardens(logger, upsert.MinGardens);
                throw new InvalidOperationException(DuplicateMessage(upsert.MinGardens), ex);
            }

            throw;
        }

        if (closedPreviousTop is not null)
        {
            VolumeDiscountPreviousTopClosed(logger, closedPreviousTop.MinGardens, closedPreviousTop.MaxGardens!.Value);
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

        var wasOpenTop = tier.MaxGardens is null
            && !await context.GardenVolumeDiscountTiers
                .AnyAsync(t => t.MinGardens > tier.MinGardens, cancellationToken);

        GardenVolumeDiscountTier? reopened = null;
        if (wasOpenTop)
        {
            reopened = await context.GardenVolumeDiscountTiers
                .Where(t => t.Id != id)
                .OrderByDescending(t => t.MinGardens)
                .FirstOrDefaultAsync(cancellationToken);

            reopened?.MaxGardens = null;
        }

        context.GardenVolumeDiscountTiers.Remove(tier);
        await context.SaveChangesAsync(cancellationToken);

        await RenumberDisplayOrderAsync(context, cancellationToken);

        VolumeDiscountTierDeleted(logger, id);

        if (reopened is not null)
        {
            VolumeDiscountNewTopReopened(logger, reopened.MinGardens);
        }
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

    /// <summary>
    /// Validerer min/max-intervallet mod øvrige aktive trin. Er det nye trin åbent (Max = null),
    /// og er det forrige øverste trin også åbent, lukkes det (Max = nyMin - 1) i samme SaveChanges.
    /// Returnerer det lukkede trin, ellers null.
    /// </summary>
    private async Task<GardenVolumeDiscountTier?> ValidateRangeAndClosePreviousTopAsync(
        PlannerDbContext context, GardenVolumeDiscountTierUpsertDto upsert, CancellationToken cancellationToken)
    {
        if (upsert.MaxGardens is int max && max < upsert.MinGardens)
        {
            VolumeDiscountTierMaxBelowMin(logger, upsert.MinGardens, max);
            throw new InvalidOperationException(
                $"Max. haver ({max}) må ikke være mindre end min. haver ({upsert.MinGardens}).");
        }

        var others = await context.GardenVolumeDiscountTiers
            .Where(t => t.Id != (upsert.Id ?? Guid.Empty))
            .OrderBy(t => t.MinGardens)
            .ToListAsync(cancellationToken);

        GardenVolumeDiscountTier? previousTopToClose = null;

        if (upsert.MaxGardens is null)
        {
            if (others.Any(o => o.MinGardens > upsert.MinGardens))
            {
                VolumeDiscountTierOpenNotTop(logger, upsert.MinGardens);
                throw new InvalidOperationException(
                    "Kun trinnet med højeste min. haver kan have åben øvre grænse (tom max.).");
            }

            var previousTop = others.LastOrDefault();
            if (previousTop is { MaxGardens: null })
            {
                previousTopToClose = previousTop;
            }
        }

        var newMax = upsert.MaxGardens ?? int.MaxValue;

        foreach (var other in others)
        {
            var otherMax = ReferenceEquals(other, previousTopToClose)
                ? upsert.MinGardens - 1
                : other.MaxGardens ?? int.MaxValue;

            if (upsert.MinGardens <= otherMax && other.MinGardens <= newMax)
            {
                VolumeDiscountTierOverlap(logger, upsert.MinGardens, other.MinGardens);
                throw new InvalidOperationException(
                    $"Trinnet {FormatRange(upsert.MinGardens, upsert.MaxGardens)} overlapper med eksisterende trin {FormatRange(other.MinGardens, other.MaxGardens)}.");
            }
        }

        previousTopToClose?.MaxGardens = upsert.MinGardens - 1;

        return previousTopToClose;
    }

    private static string FormatRange(int min, int? max) =>
        max is null ? $"{min}+" : $"{min}-{max}";

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

    private static Task<bool> DuplicateMinGardensExistsAsync(
    PlannerDbContext context, int minGardens, Guid? excludeId, CancellationToken cancellationToken) =>
    context.GardenVolumeDiscountTiers
        .AnyAsync(t => t.MinGardens == minGardens && t.Id != (excludeId ?? Guid.Empty), cancellationToken);

    private static string DuplicateMessage(int minGardens) =>
        $"Der findes allerede en trappe der starter ved {minGardens} haver.";

    private static GardenVolumeDiscountTierDto ToDto(GardenVolumeDiscountTier tier) =>
        new(tier.Id, tier.MinGardens, tier.MaxGardens, tier.PriceMultiplier);
}