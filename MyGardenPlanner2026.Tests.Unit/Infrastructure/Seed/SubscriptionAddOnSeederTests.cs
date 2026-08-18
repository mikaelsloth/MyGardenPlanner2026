namespace MyGardenPlanner2026.Tests.Unit.Infrastructure.Seed;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using Xunit;

public class SubscriptionAddOnSeederTests : TestDbContext
{
    [Fact]
    public async Task SeedAsync_OnEmptyDatabase_InsertsAllFiveAddOns()
    {
        var seeder = new SubscriptionAddOnSeeder(CreateDbContextFactory(), new DefaultSubscriptionAddOnCatalog());

        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        using var context = CreateDbContext();
        var count = await context.SubscriptionAddOns.CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(5);
    }

    [Fact]
    public async Task SeedAsync_WhenDataAlreadyExists_DoesNotDuplicate()
    {
        var seeder = new SubscriptionAddOnSeeder(CreateDbContextFactory(), new DefaultSubscriptionAddOnCatalog());
        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        using var context = CreateDbContext();
        var count = await context.SubscriptionAddOns.CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(5);
    }
}