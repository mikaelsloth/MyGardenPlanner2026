namespace MyGardenPlanner2026.Components.Pages;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Layer1;

public partial class LandingPage
{
    [Inject]
    private ISubscriptionPricingService PricingService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private IReadOnlyList<SubscriptionTierDto>? featuredTiers;

    protected override async Task OnInitializedAsync()
    {
        featuredTiers = await PricingService.GetFeaturedTiersAsync(BillingCycle.Annual);
    }

    private void HandleSelectPlan(Guid tierId) => NavigationManager.NavigateTo("/pricing");
}