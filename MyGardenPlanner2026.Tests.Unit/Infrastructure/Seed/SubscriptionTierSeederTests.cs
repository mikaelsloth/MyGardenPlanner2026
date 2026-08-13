namespace MyGardenPlanner2026.Tests.Unit.Infrastructure.Seed;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using Xunit;

public class SubscriptionTierSeederTests : TestDbContext
{
    [Fact]
    public async Task SeedAsync_OnEmptyDatabase_InsertsAllTwelveTiers()
    {
        var seeder = new SubscriptionTierSeeder(CreateDbContextFactory(), new DefaultSubscriptionTierCatalog());

        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        using var context = CreateDbContext();
        var count = await context.SubscriptionTiers.CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(12);
    }

    [Fact]
    public async Task SeedAsync_WhenDataAlreadyExists_DoesNotDuplicate()
    {
        var seeder = new SubscriptionTierSeeder(CreateDbContextFactory(), new DefaultSubscriptionTierCatalog());
        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        using var context = CreateDbContext();
        var count = await context.SubscriptionTiers.CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(12);
    }

    [Fact]
    public async Task SeedAsync_FlagsExactlyOneFeaturedTierPerLevel()
    {
        var seeder = new SubscriptionTierSeeder(CreateDbContextFactory(), new DefaultSubscriptionTierCatalog());
        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        using var context = CreateDbContext();
        var featured = await context.SubscriptionTiers
            .Where(t => t.IsFeatured)
            .ToListAsync(TestContext.Current.CancellationToken);

        featured.Should().HaveCount(3);
        featured.Select(t => t.Level).Should().OnlyHaveUniqueItems();
    }
}