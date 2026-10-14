namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;

public sealed class AmountParserTests
{
    [Theory]
    [InlineData("175,00 kr. /år", 175.00)]
    [InlineData("1.234,50 kr. /md.", 1234.50)]
    [InlineData("65,00\u00A0kr. (engang)", 65.00)]
    public void Parse_VistBeloeb_GiverBeloebetUafhaengigtAfKultur(string text, double expected)
    {
        AmountParser.Parse(text).Should().Be((decimal)expected);
    }

    [Fact]
    public void Parse_UdenCifre_GiverNull()
    {
        AmountParser.Parse("kr. /år").Should().BeNull();
    }
}