namespace MyGardenPlanner2026.Tests.Unit.Infrastructure;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Interceptors;
using NSubstitute;
using Xunit;

public class SoftDeleteInterceptorTests : TestDbContext
{
    private static ICurrentUserAccessor FakeUser()
    {
        var accessor = Substitute.For<ICurrentUserAccessor>();
        accessor.GetCurrent().Returns(new CurrentUserInfo("user-1", "admin@mygardenplanner.dk", "127.0.0.1"));
        return accessor;
    }

    [Fact]
    public async Task Remove_SoftDeletableEntity_SetsIsDeletedInsteadOfPhysicalDelete()
    {
        using var context = CreateDbContextWithInterceptors(new SoftDeleteInterceptor(FakeUser()));

        var addOn = new Core.Entities.Layer1.SubscriptionAddOn
        {
            Type = AddOnType.ArtefaktpakkeA,
            Name = "Test",
            UnitDescription = "Enhed",
            AnnualPrice = 1m,
            MonthlyPrice = 1m,
            PerpetualPrice = 1m
        };
        context.SubscriptionAddOns.Add(addOn);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.SubscriptionAddOns.Remove(addOn);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var raw = await verifyContext.SubscriptionAddOns
            .IgnoreQueryFilters()
            .SingleAsync(a => a.Id == addOn.Id, TestContext.Current.CancellationToken);

        raw.IsDeleted.Should().BeTrue();
        raw.DeletedAtUtc.Should().NotBeNull();
        raw.DeletedBy.Should().Be("admin@mygardenplanner.dk");
    }

    [Fact]
    public async Task GlobalQueryFilter_ExcludesSoftDeletedEntities_FromDefaultQueries()
    {
        using var context = CreateDbContextWithInterceptors(new SoftDeleteInterceptor(FakeUser()));

        var addOn = new Core.Entities.Layer1.SubscriptionAddOn
        {
            Type = AddOnType.ArtefaktpakkeB,
            Name = "Test2",
            UnitDescription = "Enhed",
            AnnualPrice = 1m,
            MonthlyPrice = 1m,
            PerpetualPrice = 1m
        };
        context.SubscriptionAddOns.Add(addOn);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.SubscriptionAddOns.Remove(addOn);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var visible = await verifyContext.SubscriptionAddOns
            .SingleOrDefaultAsync(a => a.Id == addOn.Id, TestContext.Current.CancellationToken);

        visible.Should().BeNull();
    }

    [Fact]
    public async Task ExistingEntities_AreUnaffectedByQueryFilter_WhenNotDeleted()
    {
        using var context = CreateDbContextWithInterceptors(new SoftDeleteInterceptor(FakeUser()));

        var tier = new Core.Entities.Layer1.GardenVolumeDiscountTier
        {
            MinGardens = 501,
            MaxGardens = null,
            PriceMultiplier = 0.35m
        };
        context.GardenVolumeDiscountTiers.Add(tier);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var visible = await verifyContext.GardenVolumeDiscountTiers
            .SingleOrDefaultAsync(t => t.Id == tier.Id, TestContext.Current.CancellationToken);

        visible.Should().NotBeNull();
    }
}