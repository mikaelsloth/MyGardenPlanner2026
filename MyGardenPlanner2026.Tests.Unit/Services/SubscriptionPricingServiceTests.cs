namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public class SubscriptionPricingServiceTests : TestDbContext
{
    private async Task SeedAsync()
    {
        var seeder = new SubscriptionTierSeeder(CreateAdminDbContextFactory(), new DefaultSubscriptionTierCatalog());
        await seeder.SeedAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetFeaturedTiersAsync_ReturnsOneTierPerLevel_OrderedByLevel()
    {
        await SeedAsync();
        var service = new SubscriptionPricingService(CreateDbContextFactory());

        var result = await service.GetFeaturedTiersAsync(BillingCycle.Annual, TestContext.Current.CancellationToken);

        result.Should().HaveCount(3);
        result.Select(t => t.Level).Should().ContainInOrder(
            GardenAccessLevel.HaveArkitekt, GardenAccessLevel.BedDesigner, GardenAccessLevel.Planlaegger);
    }

    [Fact]
    public async Task GetFeaturedTiersAsync_ReturnsEditorCategory_ForEachLevel()
    {
        await SeedAsync();
        var service = new SubscriptionPricingService(CreateDbContextFactory());

        var result = await service.GetFeaturedTiersAsync(BillingCycle.Annual, TestContext.Current.CancellationToken);

        result.Should().OnlyContain(t => t.AccessCategory == AccessCategory.Editor);
    }

    [Fact]
    public async Task GetFeaturedTiersAsync_UsesRequestedBillingCyclePrice()
    {
        await SeedAsync();
        var service = new SubscriptionPricingService(CreateDbContextFactory());

        var result = await service.GetFeaturedTiersAsync(BillingCycle.Monthly, TestContext.Current.CancellationToken);

        var haveArkitektTier = result.Single(t => t.Level == GardenAccessLevel.HaveArkitekt);
        haveArkitektTier.Price.Should().Be(14m);
        haveArkitektTier.BillingCycle.Should().Be(BillingCycle.Monthly);
    }

    [Fact]
    public async Task GetAllTiersAsync_Returns12Tiers()
    {
        await SeedAsync();
        var service = new SubscriptionPricingService(CreateDbContextFactory());

        var result = await service.GetAllTiersAsync(BillingCycle.Annual, TestContext.Current.CancellationToken);

        result.Should().HaveCount(12);
    }
}