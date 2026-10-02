namespace MyGardenPlanner2026.Tests.Unit.Services.Onboarding;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public sealed class GardenAccessQueryServiceOwnedGardenCountsTests : OnboardingTestDbContext
{
    [Fact]
    public async Task GetOwnedGardenCountsAsync_NoOwnedGardens_ReturnsZeroZero()
    {
        var sut = CreateAccessQueryService();

        var result = await sut.GetOwnedGardenCountsAsync("user-1", TestContext.Current.CancellationToken);

        result.ActiveCount.Should().Be(0);
        result.ArchivedCount.Should().Be(0);
    }

    [Fact]
    public async Task GetOwnedGardenCountsAsync_MixedActiveAndArchived_CountsCorrectly()
    {
        var activeGarden = await SeedGardenAsync("Aktiv have");
        var archivedGarden = await SeedGardenAsync("Arkiveret have", archived: true);
        await SeedMembershipAsync(
            activeGarden.Id, "user-1", GardenAccessLevel.BedDesigner, AccessCategory.Editor, isOwner: true);
        await SeedMembershipAsync(
            archivedGarden.Id, "user-1", GardenAccessLevel.BedDesigner, AccessCategory.Editor, isOwner: true);
        var sut = CreateAccessQueryService();

        var result = await sut.GetOwnedGardenCountsAsync("user-1", TestContext.Current.CancellationToken);

        result.ActiveCount.Should().Be(1);
        result.ArchivedCount.Should().Be(1);
    }

    [Fact]
    public async Task GetOwnedGardenCountsAsync_ExcludesNonOwnerMemberships()
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(garden.Id, "user-1", GardenAccessLevel.Planlaegger, AccessCategory.Viewer);
        var sut = CreateAccessQueryService();

        var result = await sut.GetOwnedGardenCountsAsync("user-1", TestContext.Current.CancellationToken);

        result.ActiveCount.Should().Be(0);
    }
}