namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public class SubscriptionAddOnServiceTests : TestDbContext
{
    private async Task SeedAsync()
    {
        var seeder = new SubscriptionAddOnSeeder(CreateAdminDbContextFactory(), new DefaultSubscriptionAddOnCatalog());
        await seeder.SeedAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetAllAddOnsAsync_ReturnsFiveAddOns_WithNonZeroDistinctIds()
    {
        await SeedAsync();
        var service = new SubscriptionAddOnService(CreateDbContextFactory());

        var result = await service.GetAllAddOnsAsync(TestContext.Current.CancellationToken);

        result.Should().HaveCount(5);
        result.Select(a => a.Id).Should().OnlyHaveUniqueItems();
        result.Should().OnlyContain(a => a.Id != Guid.Empty);
    }

    [Fact]
    public async Task GetAllAddOnsAsync_OrdersByDisplayOrder()
    {
        await SeedAsync();
        var service = new SubscriptionAddOnService(CreateDbContextFactory());

        var result = await service.GetAllAddOnsAsync(TestContext.Current.CancellationToken);

        result.First().Name.Should().Be("Bedforslag (Niveau 2)");
        result.Last().Name.Should().Be("Artefaktpakke B");
    }
}