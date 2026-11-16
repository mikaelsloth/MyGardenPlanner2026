namespace MyGardenPlanner2026.Tests.Unit.Services.Account;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Account;
using Xunit;

public sealed class RestoreEmailHasherTests
{
    [Fact]
    public void Hash_ReturnsSha256AsLowercaseHex() => RestoreEmailHasher.Hash("abc")
            .Should().Be("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");

    [Fact]
    public void Hash_Returns64LowercaseHexCharacters() => RestoreEmailHasher.Hash("bruger@example.dk").Should().MatchRegex("^[0-9a-f]{64}$");

    [Theory]
    [InlineData(" Bruger@Example.DK ")]
    [InlineData("BRUGER@EXAMPLE.DK")]
    public void Hash_NormalizesCaseAndWhitespace(string email) => RestoreEmailHasher.Hash(email).Should().Be(RestoreEmailHasher.Hash("bruger@example.dk"));

    [Fact]
    public void Hash_DifferentEmails_GiveDifferentHashes() => RestoreEmailHasher.Hash("a@example.dk").Should().NotBe(RestoreEmailHasher.Hash("b@example.dk"));

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Hash_BlankEmail_Throws(string email)
    {
        var act = () => RestoreEmailHasher.Hash(email);

        act.Should().Throw<ArgumentException>();
    }
}