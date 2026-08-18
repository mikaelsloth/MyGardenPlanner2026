namespace MyGardenPlanner2026.Components.Domain.Subscriptions;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using System.Globalization;

public partial class PricingMatrixTable
{
    private static readonly CultureInfo DanishCulture = new("da-DK");
    private static readonly BillingCycle[] cycleOptions =
        [BillingCycle.Annual, BillingCycle.Monthly, BillingCycle.Perpetual];

    [Inject]
    private ISubscriptionPricingService PricingService { get; set; } = default!;

    private BillingCycle selectedCycle = BillingCycle.Annual;
    private IReadOnlyList<SubscriptionTierDto>? tiers;

    protected override async Task OnInitializedAsync() => await LoadTiersAsync();

    private async Task LoadTiersAsync() =>
        tiers = await PricingService.GetAllTiersAsync(selectedCycle);

    private async Task SelectCycleAsync(BillingCycle cycle)
    {
        if (selectedCycle == cycle)
        {
            return;
        }

        selectedCycle = cycle;
        tiers = null;
        await LoadTiersAsync();
    }

    private IEnumerable<IGrouping<GardenAccessLevel, SubscriptionTierDto>> GroupedTiers =>
        tiers?.GroupBy(t => t.Level).OrderBy(g => g.Key) ?? Enumerable.Empty<IGrouping<GardenAccessLevel, SubscriptionTierDto>>();

    private static string CycleLabel(BillingCycle cycle) => cycle switch
    {
        BillingCycle.Annual => "Årligt",
        BillingCycle.Monthly => "Månedligt",
        BillingCycle.Perpetual => "Engangsbeløb",
        _ => cycle.ToString()
    };

    private static string PriceSuffix(BillingCycle cycle) => cycle switch
    {
        BillingCycle.Annual => "/år",
        BillingCycle.Monthly => "/md.",
        BillingCycle.Perpetual => "(engang)",
        _ => string.Empty
    };
}