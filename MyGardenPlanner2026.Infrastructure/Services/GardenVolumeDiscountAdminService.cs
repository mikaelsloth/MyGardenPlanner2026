namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;

public sealed class GardenVolumeDiscountAdminService(
    IDbContextFactory<PlannerDbContext> contextFactory,
    IGardenVolumeDiscountCatalog defaultCatalog) : IGardenVolumeDiscountAdminService
{
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

        GardenVolumeDiscountTier tier;

        if (upsert.Id is int id)
        {
            tier = await context.GardenVolumeDiscountTiers
                .SingleOrDefaultAsync(t => t.Id == id, cancellationToken)
                ?? throw new InvalidOperationException($"Ingen volumenrabat-trappe fundet med Id {id}.");

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
            context.GardenVolumeDiscountTiers.Add(tier);
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException(
                $"Der findes allerede en trappe der starter ved {upsert.MinGardens} haver.");
        }

        await RenumberDisplayOrderAsync(context, cancellationToken);

        return ToDto(tier);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var tier = await context.GardenVolumeDiscountTiers
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Ingen volumenrabat-trappe fundet med Id {id}.");

        context.GardenVolumeDiscountTiers.Remove(tier);
        await context.SaveChangesAsync(cancellationToken);

        await RenumberDisplayOrderAsync(context, cancellationToken);
    }

    public async Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        context.GardenVolumeDiscountTiers.RemoveRange(context.GardenVolumeDiscountTiers);
        await context.SaveChangesAsync(cancellationToken);

        context.GardenVolumeDiscountTiers.AddRange(defaultCatalog.GetDefaultTiers());
        await context.SaveChangesAsync(cancellationToken);
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