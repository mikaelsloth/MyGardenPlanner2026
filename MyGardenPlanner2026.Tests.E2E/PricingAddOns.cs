namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Page-object for tilkøbskortene (SubscriptionAddOnCard) på /pricing. Kortet findes via overskriften med
/// exact match, fordi "Bedforslag (Niveau 2)" er en delstreng af "Bede i Bedforslag (Niveau 2)".
/// </summary>
public static class PricingAddOns
{
    public static ILocator Cards(IPage page) => page.Locator(".addon-card");

    public static ILocator Card(IPage page, string name) =>
        Cards(page).Filter(new() { Has = page.GetByRole(AriaRole.Heading, new() { Name = name, Exact = true }) });

    public static ILocator Price(IPage page, string name, BillingCycle cycle) =>
        Card(page, name).Locator(".addon-card-prices span").Filter(new() { HasText = Suffix(cycle) });

    public static Task ExpectPriceAsync(IPage page, string name, BillingCycle cycle, decimal expected) =>
        PriceAssertions.ExpectAmountAsync(Price(page, name, cycle), expected, $"{cycle}-prisen for tilkøbet '{name}'");

    private static string Suffix(BillingCycle cycle) => cycle switch
    {
        BillingCycle.Annual => "/år",
        BillingCycle.Monthly => "/md.",
        BillingCycle.Perpetual => "(engang)",
        _ => throw new ArgumentOutOfRangeException(nameof(cycle), cycle, "Ukendt betalingscyklus."),
    };
}