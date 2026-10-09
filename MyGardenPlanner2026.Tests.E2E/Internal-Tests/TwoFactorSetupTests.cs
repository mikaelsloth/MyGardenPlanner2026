namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;

public sealed class TwoFactorSetupTests
{
    [Theory]
    [InlineData("abcd efgh ijkl mnop", "ABCDEFGHIJKLMNOP")]
    [InlineData("  abcd\tefgh\r\n", "ABCDEFGH")]
    [InlineData("ABCDEFGH", "ABCDEFGH")]
    public void NormalizeKey_FjernerWhitespaceOgGiverStoreBogstaver(string formatted, string expected)
    {
        TwoFactorSetup.NormalizeKey(formatted).Should().Be(expected);
    }
}