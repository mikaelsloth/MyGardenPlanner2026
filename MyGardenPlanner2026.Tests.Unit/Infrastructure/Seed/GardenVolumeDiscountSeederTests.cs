namespace MyGardenPlanner2026.Tests.Unit.Infrastructure.Seed;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using Xunit;

public class GardenVolumeDiscountSeederTests : TestDbContext
{
    [Fact]
    public async Task SeedAsync_OnEmptyDatabase_InsertsAllSevenTiers()
    {
        var seeder = new GardenVolumeDiscountSeeder(CreateDbContextFactory(), new DefaultGardenVolumeDiscountCatalog());

        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        using var context = CreateDbContext();
        var count = await context.GardenVolumeDiscountTiers.CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(7);
    }

    [Fact]
    public async Task SeedAsync_WhenDataAlreadyExists_DoesNotDuplicate()
    {
        var seeder = new GardenVolumeDiscountSeeder(CreateDbContextFactory(), new DefaultGardenVolumeDiscountCatalog());
        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        using var context = CreateDbContext();
        var count = await context.GardenVolumeDiscountTiers.CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(7);
    }

    [Fact]
    public async Task SeedAsync_LastTier_HasNullMaxGardens()
    {
        var seeder = new GardenVolumeDiscountSeeder(CreateDbContextFactory(), new DefaultGardenVolumeDiscountCatalog());
        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        using var context = CreateDbContext();
        var lastTier = await context.GardenVolumeDiscountTiers
            .OrderByDescending(t => t.MinGardens)
            .FirstAsync(TestContext.Current.CancellationToken);

        lastTier.MinGardens.Should().Be(201);
        lastTier.MaxGardens.Should().BeNull();
    }
}