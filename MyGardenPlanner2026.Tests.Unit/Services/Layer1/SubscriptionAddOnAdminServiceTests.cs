namespace MyGardenPlanner2026.Tests.Unit.Services.Layer1;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using MyGardenPlanner2026.Infrastructure.Interceptors;
using MyGardenPlanner2026.Infrastructure.Services;
using NSubstitute;
using Xunit;

public class SubscriptionAddOnAdminServiceTests : CatalogTestDbContext
{
    private readonly ILogger<SubscriptionAddOnAdminService> logger = Substitute.For<ILogger<SubscriptionAddOnAdminService>>();

    private SubscriptionAddOnAdminService CreateService() =>
            new(CreateAdminDbContextFactory(), new DefaultSubscriptionAddOnCatalog(), logger);

    private SubscriptionAddOnAdminService CreateServiceWithSoftDelete() =>
    new(
        CreateAdminDbContextFactoryWithInterceptors(
            new SoftDeleteInterceptor(
                FakeCurrentUser.Create("user-1", "admin@mygardenplanner.dk", "127.0.0.1"))),
        new DefaultSubscriptionAddOnCatalog(),
        logger);

    [Fact]
    public async Task SaveAsync_UpdateExisting_ChangesNameAndPrices()
    {
        await SeedSubscriptionAddOnsAsync();
        var service = CreateService();

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
        await SeedSubscriptionAddOnsAsync();
        var service = CreateService();

        var act = async () => await service.SaveAsync(
            new SubscriptionAddOnUpsertDto(null, AddOnType.BedforslagNiveau2, "Duplikat", "Enhed", 1m, 1m, 1m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_RemovesAddOn()
    {
        await SeedSubscriptionAddOnsAsync();
        var service = CreateService();

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(a => a.Type == AddOnType.ArtefaktpakkeB);

        await service.DeleteAsync(existing.Id, TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(4);
    }

    [Fact]
    public async Task ResetToDefaultAsync_RestoresFiveDefaultAddOns()
    {
        await SeedSubscriptionAddOnsAsync();
        var service = CreateService();

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(a => a.Type == AddOnType.ArtefaktpakkeB);
        await service.DeleteAsync(existing.Id, TestContext.Current.CancellationToken);

        await service.ResetToDefaultAsync(TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(5);
    }

    [Fact]
    public async Task SaveAsync_AfterSoftDeletingAddOn_AllowsNewAddOnWithSameType()
    {
        await SeedSubscriptionAddOnsAsync();
        var service = CreateServiceWithSoftDelete();

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(a => a.Type == AddOnType.ArtefaktpakkeB);
        await service.DeleteAsync(existing.Id, TestContext.Current.CancellationToken);

        var created = await service.SaveAsync(
            new SubscriptionAddOnUpsertDto(null, AddOnType.ArtefaktpakkeB, "Ny pakke B", "Enhed", 30m, 3m, 70m),
            TestContext.Current.CancellationToken);

        created.Id.Should().NotBe(existing.Id);
        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(5);
        all.Single(a => a.Type == AddOnType.ArtefaktpakkeB).Name.Should().Be("Ny pakke B");

        using var verifyContext = CreateDbContext();
        var raw = await verifyContext.SubscriptionAddOns
            .IgnoreQueryFilters()
            .SingleAsync(a => a.Id == existing.Id, TestContext.Current.CancellationToken);
        raw.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_DuplicateTypeAmongActiveAddOns_StillThrows_WhenSoftDeleteIsActive()
    {
        await SeedSubscriptionAddOnsAsync();
        var service = CreateServiceWithSoftDelete();

        var act = async () => await service.SaveAsync(
            new SubscriptionAddOnUpsertDto(null, AddOnType.BedforslagNiveau2, "Duplikat", "Enhed", 1m, 1m, 1m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ResetToDefaultAsync_AfterSoftDelete_RestoresFiveActiveAddOns()
    {
        await SeedSubscriptionAddOnsAsync();
        var service = CreateServiceWithSoftDelete();

        var existing = (await service.GetAllAsync(TestContext.Current.CancellationToken))
            .Single(a => a.Type == AddOnType.ArtefaktpakkeB);
        await service.DeleteAsync(existing.Id, TestContext.Current.CancellationToken);

        await service.ResetToDefaultAsync(TestContext.Current.CancellationToken);

        var all = await service.GetAllAsync(TestContext.Current.CancellationToken);
        all.Should().HaveCount(5);

        using var verifyContext = CreateDbContext();
        var deletedCount = await verifyContext.SubscriptionAddOns
            .IgnoreQueryFilters()
            .CountAsync(a => a.IsDeleted, TestContext.Current.CancellationToken);
        deletedCount.Should().Be(5);
    }
}