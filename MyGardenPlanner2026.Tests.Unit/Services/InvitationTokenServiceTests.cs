namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Infrastructure.Services.Onboarding;
using Xunit;

public sealed class InvitationTokenServiceTests
{
    private readonly InvitationTokenService sut = new();

    [Fact]
    public void GenerateToken_ReturnsRawTokenWithMatchingHash()
    {
        var token = sut.GenerateToken();

        sut.HashToken(token.RawToken).Should().Be(token.TokenHash);
    }

    [Fact]
    public void GenerateToken_CalledTwice_ProducesDifferentRawTokensAndHashes()
    {
        var first = sut.GenerateToken();
        var second = sut.GenerateToken();

        first.RawToken.Should().NotBe(second.RawToken);
        first.TokenHash.Should().NotBe(second.TokenHash);
    }

    [Fact]
    public void GenerateToken_RawToken_IsUrlSafeBase64WithoutPaddingOrPlusSlash()
    {
        var token = sut.GenerateToken();

        token.RawToken.Should().MatchRegex("^[A-Za-z0-9_-]+$");
        token.RawToken.Should().NotContain("=");
    }

    [Fact]
    public void GenerateToken_TokenHash_IsSixtyFourLowercaseHexCharacters()
    {
        var token = sut.GenerateToken();

        token.TokenHash.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void HashToken_SameInput_ProducesSameHash()
    {
        const string rawToken = "some-raw-token-value";

        sut.HashToken(rawToken).Should().Be(sut.HashToken(rawToken));
    }

    [Fact]
    public void HashToken_DifferentInput_ProducesDifferentHash()
    {
        sut.HashToken("token-a").Should().NotBe(sut.HashToken("token-b"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HashToken_NullOrWhitespace_ThrowsArgumentException(string? rawToken)
    {
        var act = () => sut.HashToken(rawToken!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateToken_OneThousandTokens_AreAllUnique()
    {
        var rawTokens = Enumerable.Range(0, 1000)
            .Select(_ => sut.GenerateToken().RawToken)
            .ToList();

        rawTokens.Distinct().Should().HaveCount(1000);
    }
}