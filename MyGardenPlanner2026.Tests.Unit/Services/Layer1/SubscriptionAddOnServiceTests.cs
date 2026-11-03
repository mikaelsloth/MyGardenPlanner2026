namespace MyGardenPlanner2026.Tests.Unit.Services.Layer1;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using MyGardenPlanner2026.Infrastructure.Interceptors;
using MyGardenPlanner2026.Infrastructure.Services;
using MyGardenPlanner2026.Infrastructure.Services.Layer1;
using NSubstitute;
using Xunit;

public class SubscriptionAddOnServiceTests : CatalogTestDbContext
{
    private readonly ILogger<SubscriptionAddOnAdminService> logger = Substitute.For<ILogger<SubscriptionAddOnAdminService>>();

    private SubscriptionAddOnService CreateService() => new(CreateDbContextFactory());

    private SubscriptionAddOnAdminService CreateServiceWithSoftDelete() =>
    new(
        CreateAdminDbContextFactoryWithInterceptors(
            new SoftDeleteInterceptor(
                FakeCurrentUser.Create("user-1", "admin@mygardenplanner.dk", "127.0.0.1"))),
        new DefaultSubscriptionAddOnCatalog(),
        logger);

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

    [Fact]
    public async Task SaveAsync_NonDuplicateDbUpdateException_IsRethrownUnchanged()
    {
        var service = new SubscriptionAddOnAdminService(
            CreateAdminDbContextFactoryWithInterceptors(new ThrowingSaveChangesInterceptor()),
            new DefaultSubscriptionAddOnCatalog(),
            logger);

        var act = async () => await service.SaveAsync(
            new SubscriptionAddOnUpsertDto(null, AddOnType.ArtefaktpakkeA, "Ny", "Enhed", 1m, 1m, 1m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SaveAsync_RaceOnType_ThrowsInvalidOperationException()
    {
        var racer = new BeforeFirstSaveInterceptor(async () =>
        {
            using var other = CreateDbContext();
            other.SubscriptionAddOns.Add(TestEntities.AddOn(AddOnType.ArtefaktpakkeA));
            await other.SaveChangesAsync(TestContext.Current.CancellationToken);
        });
        var service = new SubscriptionAddOnAdminService(
            CreateAdminDbContextFactoryWithInterceptors(racer),
            new DefaultSubscriptionAddOnCatalog(),
            logger);

        var act = async () => await service.SaveAsync(
            new SubscriptionAddOnUpsertDto(null, AddOnType.ArtefaktpakkeA, "Ny", "Enhed", 1m, 1m, 1m),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*ArtefaktpakkeA*");
    }
}