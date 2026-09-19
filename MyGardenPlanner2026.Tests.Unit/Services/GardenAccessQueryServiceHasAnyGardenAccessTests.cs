namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Services.Onboarding;
using Xunit;

public sealed class GardenAccessQueryServiceHasAnyGardenAccessTests : TestDbContext
{
    private static readonly DateTimeOffset FixedNow = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HasAnyGardenAccessAsync_NoMembership_ReturnsFalse()
    {
        var sut = new GardenAccessQueryService(CreateDbContextFactory(), new TestTimeProvider(FixedNow));

        var result = await sut.HasAnyGardenAccessAsync("user-1", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasAnyGardenAccessAsync_MembershipButNoEntitlement_ReturnsFalse()
    {
        var garden = new Garden { Name = "Testhave" };
        using var context = CreateDbContext();
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.GardenMemberships.AddAsync(new GardenMembership
            {
                GardenId = garden.Id,
                UserId = "user-1",
                IsOwner = false,
                Layer = GardenAccessLevel.Planlaegger,
                Category = AccessCategory.Viewer
            }, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = new GardenAccessQueryService(CreateDbContextFactory(), new TestTimeProvider(FixedNow));

        var result = await sut.HasAnyGardenAccessAsync("user-1", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasAnyGardenAccessAsync_MembershipAndActiveTrialEntitlement_ReturnsTrue()
    {
        var garden = new Garden { Name = "Sandkasse-have" };
        using var context = CreateDbContext();
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.GardenMemberships.AddAsync(new GardenMembership
            {
                GardenId = garden.Id,
                UserId = "user-1",
                IsOwner = true,
                Layer = GardenAccessLevel.HaveArkitekt,
                Category = AccessCategory.Administrator
            }, TestContext.Current.CancellationToken);
            await context.UserEntitlements.AddAsync(new UserEntitlement
            {
                UserId = "user-1",
                GardenId = garden.Id,
                Layer = GardenAccessLevel.HaveArkitekt,
                Category = AccessCategory.Administrator,
                BillingCycle = BillingCycle.Monthly,
                IsTrial = true,
                ValidToUtc = FixedNow.AddDays(30)
            }, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = new GardenAccessQueryService(CreateDbContextFactory(), new TestTimeProvider(FixedNow));

        var result = await sut.HasAnyGardenAccessAsync("user-1", TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAnyGardenAccessAsync_MembershipAndPerpetualEntitlement_ReturnsTrue()
    {
        var garden = new Garden { Name = "Betalt have" };
        using var context = CreateDbContext();
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.GardenMemberships.AddAsync(new GardenMembership
            {
                GardenId = garden.Id,
                UserId = "user-1",
                IsOwner = true,
                Layer = GardenAccessLevel.BedDesigner,
                Category = AccessCategory.Editor
            }, TestContext.Current.CancellationToken);
            await context.UserEntitlements.AddAsync(new UserEntitlement
            {
                UserId = "user-1",
                GardenId = garden.Id,
                Layer = GardenAccessLevel.BedDesigner,
                Category = AccessCategory.Editor,
                BillingCycle = BillingCycle.Perpetual,
                IsTrial = false,
                ValidToUtc = null
            }, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = new GardenAccessQueryService(CreateDbContextFactory(), new TestTimeProvider(FixedNow));

        var result = await sut.HasAnyGardenAccessAsync("user-1", TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAnyGardenAccessAsync_MembershipAndOnlyExpiredEntitlement_ReturnsFalse()
    {
        var garden = new Garden { Name = "Udløbet have" };
        var timeProvider = new TestTimeProvider(FixedNow);

        using var context = CreateDbContext();
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.GardenMemberships.AddAsync(new GardenMembership
            {
                GardenId = garden.Id,
                UserId = "user-1",
                IsOwner = true,
                Layer = GardenAccessLevel.HaveArkitekt,
                Category = AccessCategory.Administrator
            }, TestContext.Current.CancellationToken);
            await context.UserEntitlements.AddAsync(new UserEntitlement
            {
                UserId = "user-1",
                GardenId = garden.Id,
                Layer = GardenAccessLevel.HaveArkitekt,
                Category = AccessCategory.Administrator,
                BillingCycle = BillingCycle.Monthly,
                IsTrial = true,
                ValidToUtc = FixedNow.AddDays(30)
            }, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        timeProvider.Advance(TimeSpan.FromDays(31));

        var sut = new GardenAccessQueryService(CreateDbContextFactory(), timeProvider);

        var result = await sut.HasAnyGardenAccessAsync("user-1", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasAnyGardenAccessAsync_InvitedNonOwnerMemberWithValidEntitlement_ReturnsTrue()
    {
        var garden = new Garden { Name = "Andens have" };
        using var context = CreateDbContext();
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.GardenMemberships.AddAsync(new GardenMembership
            {
                GardenId = garden.Id,
                UserId = "invited-user",
                IsOwner = false,
                Layer = GardenAccessLevel.Planlaegger,
                Category = AccessCategory.Viewer
            }, TestContext.Current.CancellationToken);
            await context.UserEntitlements.AddAsync(new UserEntitlement
            {
                UserId = "invited-user",
                GardenId = garden.Id,
                Layer = GardenAccessLevel.Planlaegger,
                Category = AccessCategory.Viewer,
                BillingCycle = BillingCycle.Annual,
                IsTrial = false,
                ValidToUtc = FixedNow.AddYears(1)
            }, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = new GardenAccessQueryService(CreateDbContextFactory(), new TestTimeProvider(FixedNow));

        var result = await sut.HasAnyGardenAccessAsync("invited-user", TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAnyGardenAccessAsync_MissingUserId_ThrowsArgumentException()
    {
        var sut = new GardenAccessQueryService(CreateDbContextFactory(), new TestTimeProvider(FixedNow));

        var act = () => sut.HasAnyGardenAccessAsync("");

        await act.Should().ThrowAsync<ArgumentException>();
    }
}