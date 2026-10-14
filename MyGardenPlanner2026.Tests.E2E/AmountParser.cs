namespace MyGardenPlanner2026.Tests.E2E;

using System.Globalization;

/// <summary>
/// Udleder et beløb fra en vist pris (C2, fx "175,00 kr. /år"). Cifrene sammenlignes, så
/// testen er uafhængig af kultur, valutategn, tusindtalsseparator og ikke-brydende mellemrum.
/// </summary>
public static class AmountParser
{
    public static decimal? Parse(string text)
    {
        var digits = new string([.. text.Where(char.IsDigit)]);

        return digits.Length == 0
            ? null
            : decimal.Parse(digits, CultureInfo.InvariantCulture) / 100m;
    }
}