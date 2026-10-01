namespace MyGardenPlanner2026.Tests.Unit.Services.Layer1;

using FluentAssertions;
using MyGardenPlanner2026.Infrastructure.Services.Layer1;
using Xunit;

public class SubscriptionAddOnServiceTests : CatalogTestDbContext
{
    private SubscriptionAddOnService CreateService() => new(CreateDbContextFactory());

    [Fact]
    public async Task GetAllAddOnsAsync_ReturnsFiveAddOns_WithNonZeroDistinctIds()
    {
        await SeedSubscriptionAddOnsAsync();
        var service = CreateService();

        var result = await service.GetAllAddOnsAsync(TestContext.Current.CancellationToken);

        result.Should().HaveCount(5);
        result.Select(a => a.Id).Should().OnlyHaveUniqueItems();
        result.Should().OnlyContain(a => a.Id != Guid.Empty);
    }

    [Fact]
    public async Task GetAllAddOnsAsync_OrdersByDisplayOrder()
    {
        await SeedSubscriptionAddOnsAsync();
        var service = CreateService();

        var result = await service.GetAllAddOnsAsync(TestContext.Current.CancellationToken);

        result[0].Name.Should().Be("Bedforslag (Niveau 2)");
        result[result.Count - 1].Name.Should().Be("Artefaktpakke B");
    }
}