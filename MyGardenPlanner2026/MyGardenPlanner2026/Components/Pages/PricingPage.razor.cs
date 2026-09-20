namespace MyGardenPlanner2026.Components.Pages;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Contracts.Onboarding;

public partial class PricingPage
{
    [Inject]
    private ISubscriptionAddOnService AddOnService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private IReadOnlyList<SubscriptionAddOnDto> addOns = [];

    protected override async Task OnInitializedAsync()
    {
        addOns = await AddOnService.GetAllAddOnsAsync();
    }

    private void HandleContinueToCheckout(PricingSelectionDto selection) =>
        NavigationManager.NavigateTo(
        $"/onboarding/checkout?level={selection.Level}&category={selection.Category}&cycle={selection.BillingCycle}");

}