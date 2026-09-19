namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Infrastructure.Services.Onboarding;
using Xunit;

public sealed class GardenAccessQueryServiceOwnedGardenCountsTests : TestDbContext
{
    [Fact]
    public async Task GetOwnedGardenCountsAsync_NoOwnedGardens_ReturnsZeroZero()
    {
        var testContext = CreateDbContextFactory();
        var sut = new GardenAccessQueryService(testContext, new TestTimeProvider(DateTimeOffset.Now));

        var result = await sut.GetOwnedGardenCountsAsync("user-1", TestContext.Current.CancellationToken);

        result.ActiveCount.Should().Be(0);
        result.ArchivedCount.Should().Be(0);
    }

    [Fact]
    public async Task GetOwnedGardenCountsAsync_MixedActiveAndArchived_CountsCorrectly()
    {
        var activeGarden = new Garden { Name = "Aktiv have", Archived = false };
        var archivedGarden = new Garden { Name = "Arkiveret have", Archived = true };

        using var context = CreateDbContext();
        {
            await context.Gardens.AddRangeAsync(activeGarden, archivedGarden);
            await context.GardenMemberships.AddRangeAsync(
                new GardenMembership { GardenId = activeGarden.Id, UserId = "user-1", IsOwner = true, Layer = GardenAccessLevel.BedDesigner, Category = AccessCategory.Editor },
                new GardenMembership { GardenId = archivedGarden.Id, UserId = "user-1", IsOwner = true, Layer = GardenAccessLevel.BedDesigner, Category = AccessCategory.Editor });
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = new GardenAccessQueryService(CreateDbContextFactory(), new TestTimeProvider(DateTimeOffset.Now));

        var result = await sut.GetOwnedGardenCountsAsync("user-1", TestContext.Current.CancellationToken);

        result.ActiveCount.Should().Be(1);
        result.ArchivedCount.Should().Be(1);
    }

    [Fact]
    public async Task GetOwnedGardenCountsAsync_ExcludesNonOwnerMemberships()
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

        var sut = new GardenAccessQueryService(CreateDbContextFactory(), new TestTimeProvider(DateTimeOffset.Now));

        var result = await sut.GetOwnedGardenCountsAsync("user-1", TestContext.Current.CancellationToken);

        result.ActiveCount.Should().Be(0);
    }
}