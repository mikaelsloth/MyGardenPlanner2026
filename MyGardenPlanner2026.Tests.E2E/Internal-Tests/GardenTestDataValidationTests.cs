namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;

public sealed class GardenTestDataValidationTests
{
    private static Task<SeededGarden> Seed(GardenSeed seed) => GardenTestData.SeedAsync("Server=ignored", seed);

    [Fact]
    public async Task SeedAsync_NullSeed_Throws()
    {
        var act = () => Seed(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SeedAsync_BlankName_Throws(string name)
    {
        var act = () => Seed(new GardenSeed(name, [GardenMemberSeed.Owner("a@test.dk")]));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SeedAsync_WithoutMembers_Throws()
    {
        var act = () => Seed(new GardenSeed("Have", []));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SeedAsync_WithoutOwner_Throws()
    {
        var act = () => Seed(new GardenSeed("Have", [GardenMemberSeed.Member("a@test.dk")]));

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*ejer*");
    }

    [Fact]
    public async Task SeedAsync_DuplicateEmailsIgnoringCase_Throws()
    {
        var act = () => Seed(new GardenSeed("Have",
            [GardenMemberSeed.Owner("a@test.dk"), GardenMemberSeed.Member("A@TEST.DK")]));

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*én gang*");
    }

    [Fact]
    public void ExpectedOwnerEmailHash_TrimsAndLowercasesBeforeHashing() =>
    // SHA-256("abc") er et kendt testvektor.
    GardenTestData.ExpectedOwnerEmailHash("  ABC  ")
        .Should().Be("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");
}