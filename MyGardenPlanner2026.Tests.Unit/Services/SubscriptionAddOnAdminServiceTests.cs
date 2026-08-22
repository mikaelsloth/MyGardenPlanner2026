namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public class SubscriptionAddOnAdminServiceTests : TestDbContext
{
    private async Task SeedAsync()
    {
        var seeder = new SubscriptionAddOnSeeder(CreateAdminDbContextFactory(), new DefaultSubscriptionAddOnCatalog());
        await seeder.SeedAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SaveAsync_UpdateExisting_ChangesNameAndPrices()
    {
        await SeedAsync();
        var service = new SubscriptionAddOnAdminService(CreateAdminDbContextFactory(), new DefaultSubscriptionAddOnCatalog());

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(a => a.Type == AddOnType.BedforslagNiveau2);

        await service.SaveAsync(
            new SubscriptionAddOnUpsertDto(
                existing.Id, AddOnType.BedforslagNiveau2, "Bedforslag (opdateret)", existing.UnitDescription,
                200m, 18m, 500m),
            TestContext.Current.CancellationToken);

        var updated = (await service.GetAllAsync(TestContext.Current.CancellationToken)).Single(a => a.Id == existing.Id);
        updated.Name.Should().Be("Bedforslag (opdateret)");
        updated.AnnualPrice.Should().Be(200m);
    }

    [Fact]
    public async Task SaveAsync_DuplicateType_ThrowsInvalidOperationException()
    {
        await SeedAsync();
        var service = new SubscriptionAddOnAdminService(CreateAdminDbContextFactory(), new DefaultSubscriptionAddOnCatalog());

        var act = async () => await service.SaveAsync(
            new SubscriptionAddOnUpsertDto(null, AddOnType.BedforslagNiveau2, "Duplikat", "Enhed", 1m, 1m, 1m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_RemovesAddOn()
    {
        await SeedAsync();
        var service = new SubscriptionAddOnAdminService(CreateAdminDbContextFactory(), new DefaultSubscriptionAddOnCatalog());

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(a => a.Type == AddOnType.ArtefaktpakkeB);

        await service.DeleteAsync(existing.Id, TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(4);
    }

    [Fact]
    public async Task ResetToDefaultAsync_RestoresFiveDefaultAddOns()
    {
        await SeedAsync();
        var service = new SubscriptionAddOnAdminService(CreateAdminDbContextFactory(), new DefaultSubscriptionAddOnCatalog());

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(a => a.Type == AddOnType.ArtefaktpakkeB);
        await service.DeleteAsync(existing.Id, TestContext.Current.CancellationToken);

        await service.ResetToDefaultAsync(TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(5);
    }
}