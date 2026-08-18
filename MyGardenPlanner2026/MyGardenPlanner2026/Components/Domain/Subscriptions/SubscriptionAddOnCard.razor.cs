namespace MyGardenPlanner2026.Components.Domain.Subscriptions;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using System.Globalization;

public partial class SubscriptionAddOnCard
{
    private static readonly CultureInfo DanishCulture = new("da-DK");

    [Parameter, EditorRequired]
    public SubscriptionAddOnDto AddOn { get; set; } = default!;

    private string FormattedAnnualPrice => AddOn.AnnualPrice.ToString("C2", DanishCulture);
    private string FormattedMonthlyPrice => AddOn.MonthlyPrice.ToString("C2", DanishCulture);
}