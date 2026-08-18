namespace MyGardenPlanner2026.Components.Pages;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Layer1;

public partial class PricingPage
{
    [Inject]
    private ISubscriptionAddOnCatalog AddOnCatalog { get; set; } = default!;

    private IReadOnlyList<SubscriptionAddOnDto> addOns = [];

    protected override void OnInitialized()
    {
        addOns = [.. AddOnCatalog.GetDefaultAddOns()
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new SubscriptionAddOnDto(a.Id, a.Type, a.Name, a.UnitDescription, a.AnnualPrice, a.MonthlyPrice, a.PerpetualPrice))];
    }
}