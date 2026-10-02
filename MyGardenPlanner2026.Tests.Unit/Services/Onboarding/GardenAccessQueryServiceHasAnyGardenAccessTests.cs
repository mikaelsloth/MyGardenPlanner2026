namespace MyGardenPlanner2026.Tests.Unit.Services.Onboarding;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using Xunit;

public sealed class GardenAccessQueryServiceHasAnyGardenAccessTests : OnboardingTestDbContext
{
    [Fact]
    public async Task HasAnyGardenAccessAsync_NoMembership_ReturnsFalse()
    {
        var sut = CreateAccessQueryService();

        var result = await sut.HasAnyGardenAccessAsync("user-1", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasAnyGardenAccessAsync_MembershipButNoEntitlement_ReturnsFalse()
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(garden.Id, "user-1", GardenAccessLevel.Planlaegger, AccessCategory.Viewer);
        var sut = CreateAccessQueryService();

        var result = await sut.HasAnyGardenAccessAsync("user-1", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasAnyGardenAccessAsync_MembershipAndActiveTrialEntitlement_ReturnsTrue()
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(
            garden.Id, "user-1", GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, isOwner: true);
        await SeedEntitlementAsync(
            garden.Id, "user-1", GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator,
            BillingCycle.Monthly, isTrial: true, validToUtc: FixedNow.AddDays(30));
        var sut = CreateAccessQueryService();

        var result = await sut.HasAnyGardenAccessAsync("user-1", TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAnyGardenAccessAsync_MembershipAndPerpetualEntitlement_ReturnsTrue()
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(
            garden.Id, "user-1", GardenAccessLevel.BedDesigner, AccessCategory.Editor, isOwner: true);
        await SeedEntitlementAsync(
            garden.Id, "user-1", GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            BillingCycle.Perpetual, validToUtc: null);
        var sut = CreateAccessQueryService();

        var result = await sut.HasAnyGardenAccessAsync("user-1", TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAnyGardenAccessAsync_MembershipAndOnlyExpiredEntitlement_ReturnsFalse()
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(
            garden.Id, "user-1", GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, isOwner: true);
        await SeedEntitlementAsync(
            garden.Id, "user-1", GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator,
            BillingCycle.Monthly, isTrial: true, validToUtc: FixedNow.AddDays(30));

        var timeProvider = new TestTimeProvider(FixedNow);
        timeProvider.Advance(TimeSpan.FromDays(31));
        var sut = CreateAccessQueryService(timeProvider);

        var result = await sut.HasAnyGardenAccessAsync("user-1", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasAnyGardenAccessAsync_InvitedNonOwnerMemberWithValidEntitlement_ReturnsTrue()
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(garden.Id, "invited-user", GardenAccessLevel.Planlaegger, AccessCategory.Viewer);
        await SeedEntitlementAsync(
            garden.Id, "invited-user", GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            BillingCycle.Annual, validToUtc: FixedNow.AddYears(1));
        var sut = CreateAccessQueryService();

        var result = await sut.HasAnyGardenAccessAsync("invited-user", TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAnyGardenAccessAsync_MissingUserId_ThrowsArgumentException()
    {
        var sut = CreateAccessQueryService();

        var act = () => sut.HasAnyGardenAccessAsync("");

        await act.Should().ThrowAsync<ArgumentException>();
    }
}