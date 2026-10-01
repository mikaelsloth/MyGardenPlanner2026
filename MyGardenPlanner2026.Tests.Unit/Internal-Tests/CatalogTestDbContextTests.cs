namespace MyGardenPlanner2026.Tests.Unit.Internal_Tests;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Tests.Unit.Services.Layer1;
using Xunit;

public sealed class CatalogTestDbContextTests : CatalogTestDbContext
{
    private async Task<(int Tiers, int Discounts, int AddOns)> CountRowsAsync()
    {
        await using var context = CreateDbContext();
        return (
            await context.SubscriptionTiers.CountAsync(TestContext.Current.CancellationToken),
            await context.GardenVolumeDiscountTiers.CountAsync(TestContext.Current.CancellationToken),
            await context.SubscriptionAddOns.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SeedSubscriptionTiersAsync_InsertsOnlyTiers()
    {
        await SeedSubscriptionTiersAsync();

        (await CountRowsAsync()).Should().Be((12, 0, 0));
    }

    [Fact]
    public async Task SeedGardenVolumeDiscountsAsync_InsertsOnlyDiscountTiers()
    {
        await SeedGardenVolumeDiscountsAsync();

        (await CountRowsAsync()).Should().Be((0, 7, 0));
    }

    [Fact]
    public async Task SeedSubscriptionAddOnsAsync_InsertsOnlyAddOns()
    {
        await SeedSubscriptionAddOnsAsync();

        (await CountRowsAsync()).Should().Be((0, 0, 5));
    }

    [Fact]
    public async Task SeedAllCatalogsAsync_InsertsAllThreeCatalogs()
    {
        await SeedAllCatalogsAsync();

        (await CountRowsAsync()).Should().Be((12, 7, 5));
    }
}