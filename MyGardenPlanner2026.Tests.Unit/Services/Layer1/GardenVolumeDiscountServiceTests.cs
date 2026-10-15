namespace MyGardenPlanner2026.Tests.Unit.Services.Layer1;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using MyGardenPlanner2026.Infrastructure.Services.Layer1;
using Xunit;

public class GardenVolumeDiscountServiceTests : TestDbContext
{
    private GardenVolumeDiscountService CreateService() => new(CreateDbContextFactory());

    /// <summary>Erstatter tabellens indhold, så testen er uafhængig af evt. forudindlæste rækker.</summary>
    private async Task SeedAsync(params GardenVolumeDiscountTier[] tiers)
    {
        await using var context = CreateDbContext();
        context.GardenVolumeDiscountTiers.RemoveRange(context.GardenVolumeDiscountTiers);
        context.GardenVolumeDiscountTiers.AddRange(tiers);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetAllTiersAsync_OrdersByMinGardens_AndMapsAllFields()
    {
        await SeedAsync(
            new GardenVolumeDiscountTier { MinGardens = 51, MaxGardens = 100, PriceMultiplier = 0.60m, DisplayOrder = 3 },
            new GardenVolumeDiscountTier { MinGardens = 1, MaxGardens = 1, PriceMultiplier = 1.00m, DisplayOrder = 1 },
            new GardenVolumeDiscountTier { MinGardens = 201, MaxGardens = null, PriceMultiplier = 0.40m, DisplayOrder = 4 },
            new GardenVolumeDiscountTier { MinGardens = 2, MaxGardens = 5, PriceMultiplier = 0.90m, DisplayOrder = 2 });
        var service = CreateService();

        var result = await service.GetAllTiersAsync(TestContext.Current.CancellationToken);

        result.Select(t => t.MinGardens).Should().Equal(1, 2, 51, 201);
        result.Select(t => t.MaxGardens).Should().Equal(1, 5, 100, null);
        result.Select(t => t.PriceMultiplier).Should().Equal(1.00m, 0.90m, 0.60m, 0.40m);
        result.Select(t => t.Id).Should().OnlyHaveUniqueItems().And.NotContain(Guid.Empty);
    }

    [Fact]
    public async Task GetAllTiersAsync_WithDefaultCatalogTiers_ReturnsSevenTiersWithOpenLastTier()
    {
        await SeedAsync(new DefaultGardenVolumeDiscountCatalog().GetDefaultTiers().ToArray());
        var service = CreateService();

        var result = await service.GetAllTiersAsync(TestContext.Current.CancellationToken);

        result.Should().HaveCount(7);
        result[0].PriceMultiplier.Should().Be(1.00m);
        result[^1].MinGardens.Should().Be(201);
        result[^1].MaxGardens.Should().BeNull();
    }

    [Fact]
    public async Task GetAllTiersAsync_ReflectsUpdatedMultiplier_WithoutCaching()
    {
        await SeedAsync(new DefaultGardenVolumeDiscountCatalog().GetDefaultTiers().ToArray());
        var service = CreateService();

        await using (var context = CreateDbContext())
        {
            var tier = await context.GardenVolumeDiscountTiers
                .SingleAsync(t => t.MinGardens == 2, TestContext.Current.CancellationToken);
            tier.PriceMultiplier = 0.85m;
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var result = await service.GetAllTiersAsync(TestContext.Current.CancellationToken);

        result.Single(t => t.MinGardens == 2).PriceMultiplier.Should().Be(0.85m);
    }

    [Fact]
    public async Task GetAllTiersAsync_WhenNoTiers_ReturnsEmptyList()
    {
        await SeedAsync();
        var service = CreateService();

        var result = await service.GetAllTiersAsync(TestContext.Current.CancellationToken);

        result.Should().BeEmpty();
    }
}