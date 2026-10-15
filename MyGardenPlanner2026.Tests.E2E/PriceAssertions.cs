namespace MyGardenPlanner2026.Tests.E2E;

using FluentAssertions;
using Microsoft.Playwright;

/// <summary>
/// Poller en viste værdi, til den matcher det forventede tal (op til 5 sek.). Tåler, at elementet
/// midlertidigt er væk under (gen)indlæsning.
/// </summary>
public static class PriceAssertions
{
    public static async Task ExpectValueAsync(
        ILocator locator, decimal expected, Func<string, decimal?> parse, string description)
    {
        string? lastText = null;

        var matched = await BlazorInteraction.WaitUntilAsync(async () =>
        {
            try
            {
                lastText = await locator.InnerTextAsync(new() { Timeout = 1000 });
                return parse(lastText) == expected;
            }
            catch (PlaywrightException)
            {
                return false;
            }
        }, TimeSpan.FromSeconds(5));

        matched.Should().BeTrue($"{description} skulle være {expected}, men var '{lastText}'");
    }

    public static Task ExpectAmountAsync(ILocator locator, decimal expected, string description) =>
        ExpectValueAsync(locator, expected, AmountParser.Parse, description);
}