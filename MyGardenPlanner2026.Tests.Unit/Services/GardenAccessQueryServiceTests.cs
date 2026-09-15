namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Infrastructure.Services.Onboarding;
using Xunit;

public sealed class GardenAccessQueryServiceTests : TestDbContext
{
    [Fact]
    public async Task GetGardenSummaryAsync_ExistingGarden_ReturnsSummary()
    {
        var garden = new Garden { Name = "Testhave" };
        await using (var context = CreateDbContext())
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = new GardenAccessQueryService(CreateDbContextFactory());

        var result = await sut.GetGardenSummaryAsync(garden.Id, TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Testhave");
    }

    [Fact]
    public async Task GetGardenSummaryAsync_UnknownGarden_ReturnsNull()
    {
        var sut = new GardenAccessQueryService(CreateDbContextFactory());

        var result = await sut.GetGardenSummaryAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMembershipAsync_ExistingMembership_ReturnsDto()
    {
        var garden = new Garden { Name = "Testhave" };
        var membership = new GardenMembership
        {
            GardenId = garden.Id,
            UserId = "user-1",
            IsOwner = true,
            Layer = GardenAccessLevel.BedDesigner,
            Category = AccessCategory.Editor
        };
        await using (var context = CreateDbContext())
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.GardenMemberships.AddAsync(membership, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = new GardenAccessQueryService(CreateDbContextFactory());

        var result = await sut.GetMembershipAsync(garden.Id, "user-1", TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.IsOwner.Should().BeTrue();
        result.Layer.Should().Be(GardenAccessLevel.BedDesigner);
    }

    [Fact]
    public async Task GetMembersAsync_MultipleMembers_ReturnsOwnerFirst()
    {
        var garden = new Garden { Name = "Testhave" };
        var owner = new GardenMembership { GardenId = garden.Id, UserId = "owner", IsOwner = true, Layer = GardenAccessLevel.HaveArkitekt, Category = AccessCategory.Administrator };
        var member = new GardenMembership { GardenId = garden.Id, UserId = "member", IsOwner = false, Layer = GardenAccessLevel.Planlaegger, Category = AccessCategory.Viewer };
        await using (var context = CreateDbContext())
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.GardenMemberships.AddRangeAsync(member, owner);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = new GardenAccessQueryService(CreateDbContextFactory());

        var result = await sut.GetMembersAsync(garden.Id, TestContext.Current.CancellationToken);

        result.Should().HaveCount(2);
        result[0].IsOwner.Should().BeTrue();
    }

    [Fact]
    public async Task GetInvitationsAsync_ReturnsNewestFirst()
    {
        var garden = new Garden { Name = "Testhave" };
        var older = new GardenInvitation { GardenId = garden.Id, InvitedByUserId = "owner", Email = "a@example.com", TokenHash = "hash-a", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-2), ExpiresUtc = DateTimeOffset.UtcNow.AddDays(5) };
        var newer = new GardenInvitation { GardenId = garden.Id, InvitedByUserId = "owner", Email = "b@example.com", TokenHash = "hash-b", CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1), ExpiresUtc = DateTimeOffset.UtcNow.AddDays(5) };
        await using (var context = CreateDbContext())
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.GardenInvitations.AddRangeAsync(older, newer);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = new GardenAccessQueryService(CreateDbContextFactory());

        var result = await sut.GetInvitationsAsync(garden.Id, TestContext.Current.CancellationToken);

        result.Should().HaveCount(2);
        result[0].Email.Should().Be("b@example.com");
    }
}