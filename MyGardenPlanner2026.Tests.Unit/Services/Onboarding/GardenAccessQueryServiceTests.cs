namespace MyGardenPlanner2026.Tests.Unit.Services.Onboarding;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using Xunit;

public sealed class GardenAccessQueryServiceTests : OnboardingTestDbContext
{
    [Fact]
    public async Task GetGardenSummaryAsync_ExistingGarden_ReturnsSummary()
    {
        var garden = await SeedGardenAsync();
        var sut = CreateAccessQueryService();

        var result = await sut.GetGardenSummaryAsync(garden.Id, TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Testhave");
    }

    [Fact]
    public async Task GetGardenSummaryAsync_UnknownGarden_ReturnsNull()
    {
        var sut = CreateAccessQueryService();

        var result = await sut.GetGardenSummaryAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMembershipAsync_ExistingMembership_ReturnsDto()
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(
            garden.Id, "user-1", GardenAccessLevel.BedDesigner, AccessCategory.Editor, isOwner: true);
        var sut = CreateAccessQueryService();

        var result = await sut.GetMembershipAsync(garden.Id, "user-1", TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.IsOwner.Should().BeTrue();
        result.Layer.Should().Be(GardenAccessLevel.BedDesigner);
    }

    [Fact]
    public async Task GetMembersAsync_MultipleMembers_ReturnsOwnerFirst()
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(garden.Id, "member", GardenAccessLevel.Planlaegger, AccessCategory.Viewer);
        await SeedMembershipAsync(
            garden.Id, "owner", GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, isOwner: true);
        var sut = CreateAccessQueryService();

        var result = await sut.GetMembersAsync(garden.Id, TestContext.Current.CancellationToken);

        result.Should().HaveCount(2);
        result[0].IsOwner.Should().BeTrue();
    }

    [Fact]
    public async Task GetInvitationsAsync_ReturnsNewestFirst()
    {
        var garden = await SeedGardenAsync();
        var older = new GardenInvitation { GardenId = garden.Id, InvitedByUserId = "owner", Email = "a@example.com", TokenHash = "hash-a", CreatedAtUtc = FixedNow.AddDays(-2), ExpiresUtc = FixedNow.AddDays(5) };
        var newer = new GardenInvitation { GardenId = garden.Id, InvitedByUserId = "owner", Email = "b@example.com", TokenHash = "hash-b", CreatedAtUtc = FixedNow.AddDays(-1), ExpiresUtc = FixedNow.AddDays(5) };
        await using (var context = CreateDbContext())
        {
            await context.GardenInvitations.AddRangeAsync(older, newer);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = CreateAccessQueryService();

        var result = await sut.GetInvitationsAsync(garden.Id, TestContext.Current.CancellationToken);

        result.Should().HaveCount(2);
        result[0].Email.Should().Be("b@example.com");
    }
}