namespace MyGardenPlanner2026.Tests.Unit.Internal_Tests;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Tests.Unit.Services.Onboarding;
using Xunit;

public sealed class OnboardingTestDbContextTests : OnboardingTestDbContext
{
    [Fact]
    public async Task SeedGardenAsync_PersistsGardenWithNameAndArchivedFlag()
    {
        var garden = await SeedGardenAsync("Min have", archived: true);

        await using var context = CreateDbContext();
        var saved = await context.Gardens.SingleAsync(g => g.Id == garden.Id, TestContext.Current.CancellationToken);

        saved.Name.Should().Be("Min have");
        saved.Archived.Should().BeTrue();
    }

    [Fact]
    public async Task SeedMembershipAsync_PersistsMembershipWithGivenRights()
    {
        var garden = await SeedGardenAsync();

        var membership = await SeedMembershipAsync(
            garden.Id, "user-1", GardenAccessLevel.BedDesigner, AccessCategory.Editor, isOwner: true);

        await using var context = CreateDbContext();
        var saved = await context.GardenMemberships.SingleAsync(m => m.Id == membership.Id, TestContext.Current.CancellationToken);

        saved.UserId.Should().Be("user-1");
        saved.IsOwner.Should().BeTrue();
        saved.Layer.Should().Be(GardenAccessLevel.BedDesigner);
        saved.Category.Should().Be(AccessCategory.Editor);
    }

    [Fact]
    public async Task SeedMembershipAsync_WithoutIsOwner_DefaultsToNonOwner()
    {
        var garden = await SeedGardenAsync();

        var membership = await SeedMembershipAsync(
            garden.Id, "user-1", GardenAccessLevel.Planlaegger, AccessCategory.Viewer);

        membership.IsOwner.Should().BeFalse();
    }

    [Fact]
    public async Task SeedEntitlementAsync_PersistsEntitlementWithBillingCycleAndValidTo()
    {
        var garden = await SeedGardenAsync();

        var entitlement = await SeedEntitlementAsync(
            garden.Id, "user-1", GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            BillingCycle.Annual, validToUtc: FixedNow.AddYears(1));

        await using var context = CreateDbContext();
        var saved = await context.UserEntitlements.SingleAsync(e => e.Id == entitlement.Id, TestContext.Current.CancellationToken);

        saved.BillingCycle.Should().Be(BillingCycle.Annual);
        saved.IsTrial.Should().BeFalse();
        saved.ValidToUtc.Should().Be(FixedNow.AddYears(1));
    }

    [Fact]
    public async Task CreateAccessQueryService_WithoutTimeProvider_UsesFixedNow()
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(garden.Id, "user-1", GardenAccessLevel.BedDesigner, AccessCategory.Editor);
        await SeedEntitlementAsync(
            garden.Id, "user-1", GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            BillingCycle.Monthly, validToUtc: FixedNow.AddDays(1));

        var sut = CreateAccessQueryService();

        (await sut.HasAnyGardenAccessAsync("user-1", TestContext.Current.CancellationToken)).Should().BeTrue();
    }

    [Fact]
    public async Task CreateAccessQueryService_WithCustomTimeProvider_UsesThatTime()
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(garden.Id, "user-1", GardenAccessLevel.BedDesigner, AccessCategory.Editor);
        await SeedEntitlementAsync(
            garden.Id, "user-1", GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            BillingCycle.Monthly, validToUtc: FixedNow.AddDays(1));

        var sut = CreateAccessQueryService(new TestTimeProvider(FixedNow.AddDays(2)));

        (await sut.HasAnyGardenAccessAsync("user-1", TestContext.Current.CancellationToken)).Should().BeFalse();
    }
}