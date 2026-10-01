namespace MyGardenPlanner2026.Tests.Unit.Services.Layer1;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Services.Layer1;
using Xunit;

public class SubscriptionPricingServiceTests : CatalogTestDbContext
{
    private SubscriptionPricingService CreateService() => new(CreateDbContextFactory());

    [Fact]
    public async Task GetFeaturedTiersAsync_ReturnsOneTierPerLevel_OrderedByLevel()
    {
        await SeedSubscriptionTiersAsync();
        var service = CreateService();

        var result = await service.GetFeaturedTiersAsync(BillingCycle.Annual, TestContext.Current.CancellationToken);

        result.Should().HaveCount(3);
        result.Select(t => t.Level).Should().ContainInOrder(
            GardenAccessLevel.HaveArkitekt, GardenAccessLevel.BedDesigner, GardenAccessLevel.Planlaegger);
    }

    [Fact]
    public async Task GetFeaturedTiersAsync_ReturnsEditorCategory_ForEachLevel()
    {
        await SeedSubscriptionTiersAsync();
        var service = CreateService();

        var result = await service.GetFeaturedTiersAsync(BillingCycle.Annual, TestContext.Current.CancellationToken);

        result.Should().OnlyContain(t => t.AccessCategory == AccessCategory.Editor);
    }

    [Fact]
    public async Task GetFeaturedTiersAsync_UsesRequestedBillingCyclePrice()
    {
        await SeedSubscriptionTiersAsync();
        var service = CreateService();

        var result = await service.GetFeaturedTiersAsync(BillingCycle.Monthly, TestContext.Current.CancellationToken);

        var haveArkitektTier = result.Single(t => t.Level == GardenAccessLevel.HaveArkitekt);
        haveArkitektTier.Price.Should().Be(14m);
        haveArkitektTier.BillingCycle.Should().Be(BillingCycle.Monthly);
    }

    [Fact]
    public async Task GetAllTiersAsync_Returns12Tiers()
    {
        await SeedSubscriptionTiersAsync();
        var service = CreateService();

        var result = await service.GetAllTiersAsync(BillingCycle.Annual, TestContext.Current.CancellationToken);

        result.Should().HaveCount(12);
    }
}