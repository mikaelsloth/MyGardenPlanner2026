namespace MyGardenPlanner2026.Components.Domain.Subscriptions;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Layer1;
using System.Globalization;

public partial class PricingCard
{
    private static readonly CultureInfo DanishCulture = new("da-DK");

    [Parameter, EditorRequired]
    public SubscriptionTierDto Tier { get; set; } = default!;

    [Parameter]
    public EventCallback<Guid> OnSelectPlan { get; set; }

    private string FormattedPrice => Tier.Price.ToString("C2", DanishCulture);

    private string PriceSuffix => Tier.BillingCycle switch
    {
        BillingCycle.Annual => "/år",
        BillingCycle.Monthly => "/md.",
        BillingCycle.Perpetual => "(engangsbeløb)",
        _ => string.Empty
    };

    private async Task HandleSelectAsync() => await OnSelectPlan.InvokeAsync(Tier.Id);
}