namespace MyGardenPlanner2026.Tests.Unit.Services.Layer1;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using MyGardenPlanner2026.Infrastructure.Interceptors;
using MyGardenPlanner2026.Infrastructure.Services;
using NSubstitute;
using Xunit;

public class GardenVolumeDiscountAdminServiceTests : CatalogTestDbContext
{
    private readonly ILogger<GardenVolumeDiscountAdminService> logger = Substitute.For<ILogger<GardenVolumeDiscountAdminService>>();

    private GardenVolumeDiscountAdminService CreateService() =>
            new(CreateAdminDbContextFactory(), new DefaultGardenVolumeDiscountCatalog(), logger);

    private GardenVolumeDiscountAdminService CreateServiceWithSoftDelete() =>
    new(
        CreateAdminDbContextFactoryWithInterceptors(
            new SoftDeleteInterceptor(
                FakeCurrentUser.Create("user-1", "admin@mygardenplanner.dk", "127.0.0.1"))),
        new DefaultGardenVolumeDiscountCatalog(),
        logger);

    [Fact]
    public async Task SaveAsync_WithNullId_CreatesNewTier()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

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
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

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
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

        var act = async () => await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 1, 1, 1.00m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_RemovesTier()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken)).First(t => t.MinGardens == 201);

        await service.DeleteAsync(existing.Id, TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(6);
    }

    [Fact]
    public async Task ResetToDefaultAsync_RestoresSevenDefaultTiers()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

        await service.SaveAsync(new GardenVolumeDiscountTierUpsertDto(null, 501, null, 0.30m), TestContext.Current.CancellationToken);
        await service.ResetToDefaultAsync(TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(7);
    }

    [Fact]
    public async Task SaveAsync_AfterSoftDeletingTier_AllowsNewTierWithSameMinGardens()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateServiceWithSoftDelete();

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(t => t.MinGardens == 201);
        await service.DeleteAsync(existing.Id, TestContext.Current.CancellationToken);

        var created = await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 201, null, 0.35m),
            TestContext.Current.CancellationToken);

        created.Id.Should().NotBe(existing.Id);
        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(7);
        all.Single(t => t.MinGardens == 201).PriceMultiplier.Should().Be(0.35m);

        using var verifyContext = CreateDbContext();
        var raw = await verifyContext.GardenVolumeDiscountTiers
            .IgnoreQueryFilters()
            .SingleAsync(t => t.Id == existing.Id, TestContext.Current.CancellationToken);
        raw.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_DuplicateMinGardensAmongActiveTiers_StillThrows_WhenSoftDeleteIsActive()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateServiceWithSoftDelete();

        var act = async () => await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 1, 1, 1.00m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ResetToDefaultAsync_AfterSoftDelete_RestoresSevenActiveTiers()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateServiceWithSoftDelete();

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(t => t.MinGardens == 201);
        await service.DeleteAsync(existing.Id, TestContext.Current.CancellationToken);

        await service.ResetToDefaultAsync(TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(7);

        using var verifyContext = CreateDbContext();
        var deletedCount = await verifyContext.GardenVolumeDiscountTiers
            .IgnoreQueryFilters()
            .CountAsync(t => t.IsDeleted, TestContext.Current.CancellationToken);
        deletedCount.Should().Be(7);
    }
}