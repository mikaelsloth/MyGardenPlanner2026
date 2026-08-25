namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public class GardenVolumeDiscountAdminServiceTests : TestDbContext
{
    private async Task SeedAsync()
    {
        var seeder = new GardenVolumeDiscountSeeder(CreateAdminDbContextFactory(), new DefaultGardenVolumeDiscountCatalog());
        await seeder.SeedAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SaveAsync_WithNullId_CreatesNewTier()
    {
        await SeedAsync();
        var service = new GardenVolumeDiscountAdminService(CreateAdminDbContextFactory(), new DefaultGardenVolumeDiscountCatalog());

        var created = await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 501, null, 0.30m),
            TestContext.Current.CancellationToken);

        created.Id.Should().NotBe(Guid.Empty);
        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(8);
    }

    [Fact]
    public async Task SaveAsync_WithExistingId_UpdatesTier()
    {
        await SeedAsync();
        var service = new GardenVolumeDiscountAdminService(CreateAdminDbContextFactory(), new DefaultGardenVolumeDiscountCatalog());

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken)).First(t => t.MinGardens == 1);

        await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(existing.Id, 1, 1, 0.95m),
            TestContext.Current.CancellationToken);

        var updated = (await service.GetAllAsync(TestContext.Current.CancellationToken)).Single(t => t.Id == existing.Id);
        updated.PriceMultiplier.Should().Be(0.95m);
    }

    [Fact]
    public async Task SaveAsync_DuplicateMinGardens_ThrowsInvalidOperationException()
    {
        await SeedAsync();
        var service = new GardenVolumeDiscountAdminService(CreateAdminDbContextFactory(), new DefaultGardenVolumeDiscountCatalog());

        var act = async () => await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 1, 1, 1.00m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_RemovesTier()
    {
        await SeedAsync();
        var service = new GardenVolumeDiscountAdminService(CreateAdminDbContextFactory(), new DefaultGardenVolumeDiscountCatalog());

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken)).First(t => t.MinGardens == 201);

        await service.DeleteAsync(existing.Id, TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(6);
    }

    [Fact]
    public async Task ResetToDefaultAsync_RestoresSevenDefaultTiers()
    {
        await SeedAsync();
        var service = new GardenVolumeDiscountAdminService(CreateAdminDbContextFactory(), new DefaultGardenVolumeDiscountCatalog());

        await service.SaveAsync(new GardenVolumeDiscountTierUpsertDto(null, 501, null, 0.30m), TestContext.Current.CancellationToken);
        await service.ResetToDefaultAsync(TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(7);
    }
}