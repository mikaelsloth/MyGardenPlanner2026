namespace MyGardenPlanner2026.Components.Pages;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Layer1;

public partial class PricingPage
{
    [Inject]
    private ISubscriptionAddOnService AddOnService { get; set; } = default!;

    private IReadOnlyList<SubscriptionAddOnDto> addOns = [];

    protected override async Task OnInitializedAsync()
    {
        addOns = await AddOnService.GetAllAddOnsAsync();
    }
}