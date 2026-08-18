namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Beregner have-abonnementspris jf. Prismatrix.md, tabel 1-3.
/// Volumenrabatten (tabel 2) er en cyklus-agnostisk procentsats og anvendes derfor
/// på tværs af Annual/Monthly/Perpetual basispriser. Understøtter alle tre BillingCycle-værdier.
/// </summary>
public sealed class PricingCalculatorService(
    IDbContextFactory<PlannerDbContext> contextFactory) : IPricingCalculatorService
{
    private const decimal ArchivedGardenWeightForAdministrator = 0.25m;
    private const decimal ArchivedGardenWeightForOtherCategories = 1.0m;

    public async Task<PricingCalculationResultDto> CalculateAsync(
        PricingCalculationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ActiveGardens < 0 || request.ArchivedGardens < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Antal haver kan ikke være negativt.");
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var tier = await context.SubscriptionTiers
            .SingleOrDefaultAsync(
                t => t.Level == request.Level && t.AccessCategory == request.AccessCategory,
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"Intet abonnement fundet for {request.Level} / {request.AccessCategory}.");

        var basePricePerGarden = request.BillingCycle switch
        {
            BillingCycle.Annual => tier.AnnualPrice,
            BillingCycle.Monthly => tier.MonthlyPrice,
            BillingCycle.Perpetual => tier.PerpetualPrice,
            _ => throw new ArgumentOutOfRangeException(nameof(request), "Ukendt BillingCycle.")
        };

        var archivedWeight = request.AccessCategory == AccessCategory.Administrator
            ? ArchivedGardenWeightForAdministrator
            : ArchivedGardenWeightForOtherCategories;

        var weightedGardenCount = request.ActiveGardens + (request.ArchivedGardens * archivedWeight);

        var discountTiers = await context.GardenVolumeDiscountTiers
            .OrderByDescending(d => d.MinGardens)
            .ToListAsync(cancellationToken);

        var matchedTier = discountTiers.FirstOrDefault(d => weightedGardenCount >= d.MinGardens)
            ?? throw new InvalidOperationException("Ingen matchende volumenrabat-trappe fundet.");

        var gardenSubtotal = basePricePerGarden * matchedTier.PriceMultiplier * weightedGardenCount;

        var addOnLineItems = new List<AddOnLineItemDto>();
        var addOnsTotal = 0m;

        var requestedAddOnIds = request.AddOnQuantities
            .Where(kv => kv.Value > 0)
            .Select(kv => kv.Key)
            .ToList();

        if (requestedAddOnIds.Count > 0)
        {
            var addOns = await context.SubscriptionAddOns
                .Where(a => requestedAddOnIds.Contains(a.Id))
                .ToListAsync(cancellationToken);

            foreach (var addOnId in requestedAddOnIds)
            {
                var quantity = request.AddOnQuantities[addOnId];

                var addOn = addOns.SingleOrDefault(a => a.Id == addOnId)
                    ?? throw new InvalidOperationException($"Tilkøb med Id {addOnId} findes ikke.");

                // Add-ons gælder altid pr. have (ikke ganget med antal haver).
                var unitPrice = request.BillingCycle switch
                {
                    BillingCycle.Annual => addOn.AnnualPrice,
                    BillingCycle.Monthly => addOn.MonthlyPrice,
                    BillingCycle.Perpetual => addOn.PerpetualPrice,
                    _ => throw new ArgumentOutOfRangeException(nameof(request), "Ukendt BillingCycle.")
                };

                var lineTotal = unitPrice * quantity;
                addOnsTotal += lineTotal;

                addOnLineItems.Add(new AddOnLineItemDto(addOn.Id, addOn.Name, quantity, unitPrice, lineTotal));
            }
        }

        var total = gardenSubtotal + addOnsTotal;

        return new PricingCalculationResultDto(
            basePricePerGarden,
            weightedGardenCount,
            matchedTier.PriceMultiplier,
            gardenSubtotal,
            addOnLineItems,
            addOnsTotal,
            total);
    }
}