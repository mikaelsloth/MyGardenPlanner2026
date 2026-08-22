namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public class SubscriptionTierAdminServiceTests : TestDbContext
{
    private async Task SeedAsync()
    {
        var seeder = new SubscriptionTierSeeder(CreateAdminDbContextFactory(), new DefaultSubscriptionTierCatalog());
        await seeder.SeedAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetAllTiersAsync_Returns12Tiers()
    {
        await SeedAsync();
        var service = new SubscriptionTierAdminService(CreateAdminDbContextFactory());

        var result = await service.GetAllTiersAsync(TestContext.Current.CancellationToken);

        result.Should().HaveCount(12);
    }

    [Fact]
    public async Task UpdateTierAsync_UpdatesAllThreePricesAndPersists()
    {
        await SeedAsync();
        var service = new SubscriptionTierAdminService(CreateAdminDbContextFactory());

        var tiers = await service.GetAllTiersAsync(TestContext.Current.CancellationToken);
        var target = tiers.Single(t => t.Level == GardenAccessLevel.HaveArkitekt && t.AccessCategory == AccessCategory.Administrator);

        await service.UpdateTierAsync(
            new SubscriptionTierUpdateDto(target.Id, 400m, 35m, 1000m),
            TestContext.Current.CancellationToken);

        var updated = (await service.GetAllTiersAsync(TestContext.Current.CancellationToken)).Single(t => t.Id == target.Id);

        updated.AnnualPrice.Should().Be(400m);
        updated.MonthlyPrice.Should().Be(35m);
        updated.PerpetualPrice.Should().Be(1000m);
    }

    [Fact]
    public async Task UpdateTierAsync_NonExistentId_ThrowsInvalidOperationException()
    {
        await SeedAsync();
        var service = new SubscriptionTierAdminService(CreateAdminDbContextFactory());

        var act = async () => await service.UpdateTierAsync(
            new SubscriptionTierUpdateDto(999, 1m, 1m, 1m), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}