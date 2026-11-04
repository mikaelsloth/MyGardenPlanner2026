namespace MyGardenPlanner2026.Tests.Unit.Services.Layer1;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Layer1;
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

    [Fact]
    public async Task SaveAsync_UpdateToExistingMinGardens_ThrowsInvalidOperationException()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(t => t.MinGardens == 1);

        var act = async () => await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(existing.Id, 2, 1, 0.90m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*2 haver*");

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().ContainSingle(t => t.Id == existing.Id && t.MinGardens == 1);
    }

    [Fact]
    public async Task SaveAsync_NonDuplicateDbUpdateException_IsRethrownUnchanged()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = new GardenVolumeDiscountAdminService(
            CreateAdminDbContextFactoryWithInterceptors(new ThrowingSaveChangesInterceptor()),
            new DefaultGardenVolumeDiscountCatalog(),
            logger);

        var act = async () => await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 501, null, 0.30m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveAsync_RaceOnMinGardens_ThrowsInvalidOperationException()
    {
        await SeedGardenVolumeDiscountsAsync();
        var racer = new BeforeFirstSaveInterceptor(async () =>
        {
            using var other = CreateDbContext();
            other.GardenVolumeDiscountTiers.Add(
                new GardenVolumeDiscountTier { MinGardens = 501, MaxGardens = null, PriceMultiplier = 0.25m });
            await other.SaveChangesAsync(TestContext.Current.CancellationToken);
        });
        var service = new GardenVolumeDiscountAdminService(
            CreateAdminDbContextFactoryWithInterceptors(racer),
            new DefaultGardenVolumeDiscountCatalog(),
            logger);

        var act = async () => await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 501, null, 0.30m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*501 haver*");
    }

    [Fact]
    public async Task SaveAsync_MaxBelowMin_ThrowsInvalidOperationException()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

        var act = async () => await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 600, 599, 0.30m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*må ikke være mindre end*");
        (await service.GetAllAsync(TestContext.Current.CancellationToken)).Should().HaveCount(7);
    }

    [Fact]
    public async Task SaveAsync_NewTierOverlapsExistingTier_ThrowsInvalidOperationException()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

        var act = async () => await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 4, 4, 0.85m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*overlapper*");
        (await service.GetAllAsync(TestContext.Current.CancellationToken)).Should().HaveCount(7);
    }

    [Fact]
    public async Task SaveAsync_UpdateExtendsIntoNextTier_ThrowsAndLeavesTierUnchanged()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(t => t.MinGardens == 2);

        var act = async () => await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(existing.Id, 2, 6, 0.90m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*overlapper*");

        var after = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(t => t.Id == existing.Id);
        after.MaxGardens.Should().Be(5);
    }

    [Fact]
    public async Task SaveAsync_OpenTierBelowHigherTier_ThrowsInvalidOperationException()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(t => t.MinGardens == 101);

        var act = async () => await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(existing.Id, 101, null, 0.50m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*højeste min. haver*");
    }

    [Fact]
    public async Task SaveAsync_NewTierWithMaxAboveOpenTier_ThrowsInvalidOperationException()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

        var act = async () => await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 501, 600, 0.30m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*overlapper*");
    }

    [Fact]
    public async Task SaveAsync_NewOpenTopTier_ClosesPreviousOpenTopTier()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

        await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 501, null, 0.30m),
            TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(8);
        all.Single(t => t.MinGardens == 201).MaxGardens.Should().Be(500);
        all.Single(t => t.MinGardens == 501).MaxGardens.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_SecondOpenTopTier_ClosesOnlyTheNewestPreviousTop()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

        await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 501, null, 0.30m),
            TestContext.Current.CancellationToken);
        await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 1001, null, 0.25m),
            TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Single(t => t.MinGardens == 201).MaxGardens.Should().Be(500);
        all.Single(t => t.MinGardens == 501).MaxGardens.Should().Be(1000);
        all.Single(t => t.MinGardens == 1001).MaxGardens.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_UpdateLeavingGapToNextTier_Succeeds()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(t => t.MinGardens == 2);

        await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(existing.Id, 2, 4, 0.90m),
            TestContext.Current.CancellationToken);

        var after = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(t => t.Id == existing.Id);
        after.MaxGardens.Should().Be(4);
    }

    [Fact]
    public async Task SaveAsync_SoftDeletedTierDoesNotCountAsOverlap()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateServiceWithSoftDelete();

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(t => t.MinGardens == 2);
        await service.DeleteAsync(existing.Id, TestContext.Current.CancellationToken);

        var created = await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 3, 4, 0.85m),
            TestContext.Current.CancellationToken);

        created.MinGardens.Should().Be(3);
    }

    [Fact]
    public async Task DeleteAsync_OpenTopTier_ReopensNewTopTier()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

        var created = await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 501, null, 0.30m),
            TestContext.Current.CancellationToken);

        await service.DeleteAsync(created.Id, TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(7);
        all.Single(t => t.MinGardens == 201).MaxGardens.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_OpenTopTier_ReopensNewTopTier_WhenSoftDeleteIsActive()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateServiceWithSoftDelete();

        var created = await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(null, 501, null, 0.30m),
            TestContext.Current.CancellationToken);

        await service.DeleteAsync(created.Id, TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(7);
        all.Single(t => t.MinGardens == 201).MaxGardens.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ClosedTopTier_DoesNotReopenPreviousTier()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

        var top = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(t => t.MinGardens == 201);
        await service.SaveAsync(
            new GardenVolumeDiscountTierUpsertDto(top.Id, 201, 300, 0.40m),
            TestContext.Current.CancellationToken);

        await service.DeleteAsync(top.Id, TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Single(t => t.MinGardens == 101).MaxGardens.Should().Be(200);
    }

    [Fact]
    public async Task DeleteAsync_NonTopTier_LeavesOtherTiersUnchanged()
    {
        await SeedGardenVolumeDiscountsAsync();
        var service = CreateService();

        var middle = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(t => t.MinGardens == 51);

        await service.DeleteAsync(middle.Id, TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Single(t => t.MinGardens == 101).MaxGardens.Should().Be(200);
        all.Single(t => t.MinGardens == 201).MaxGardens.Should().BeNull();
    }
}