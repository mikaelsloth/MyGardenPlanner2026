namespace MyGardenPlanner2026.Tests.Unit.Interceptors;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Interceptors;
using NSubstitute;
using Xunit;

public class SoftDeleteInterceptorTests : TestDbContext
{
    private static ICurrentUserAccessor FakeUser() =>
        FakeCurrentUser.Create("user-1", "admin@mygardenplanner.dk", "127.0.0.1");

    [Fact]
    public async Task Remove_SoftDeletableEntity_SetsIsDeletedInsteadOfPhysicalDelete()
    {
        using var context = CreateDbContextWithInterceptors(new SoftDeleteInterceptor(FakeUser()));

        var addOn = TestEntities.AddOn();
        await context.SubscriptionAddOns.AddAsync(addOn, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.SubscriptionAddOns.Remove(addOn);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var raw = await verifyContext.SubscriptionAddOns
            .IgnoreQueryFilters()
            .SingleAsync(a => a.Id == addOn.Id, TestContext.Current.CancellationToken);

        raw.IsDeleted.Should().BeTrue();
        raw.DeletedAtUtc.Should().NotBeNull();
        raw.DeletedByUserId.Should().Be("user-1");
        raw.DeletedByEmail.Should().Be("admin@mygardenplanner.dk");
    }

    [Fact]
    public async Task GlobalQueryFilter_ExcludesSoftDeletedEntities_FromDefaultQueries()
    {
        using var context = CreateDbContextWithInterceptors(new SoftDeleteInterceptor(FakeUser()));

        var addOn = TestEntities.AddOn(AddOnType.ArtefaktpakkeB);
        await context.SubscriptionAddOns.AddAsync(addOn, TestContext.Current.CancellationToken);
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
        await context.GardenVolumeDiscountTiers.AddAsync(tier, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var visible = await verifyContext.GardenVolumeDiscountTiers
            .SingleOrDefaultAsync(t => t.Id == tier.Id, TestContext.Current.CancellationToken);

        visible.Should().NotBeNull();
    }

    private static ICurrentUserAccessor NoUser()
    {
        var accessor = Substitute.For<ICurrentUserAccessor>();
        accessor.GetCurrent().Returns(new CurrentUserInfo(null, null, null));
        return accessor;
    }

    private async Task<SubscriptionAddOn> ReadRawAsync(Guid id)
    {
        using var verifyContext = CreateDbContext();
        return await verifyContext.SubscriptionAddOns
            .IgnoreQueryFilters()
            .SingleAsync(a => a.Id == id, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Remove_WithoutUser_InSystemActorScope_WritesSystemLabel()
    {
        using var context = CreateDbContextWithInterceptors(new SoftDeleteInterceptor(NoUser()));
        var addOn = TestEntities.AddOn();
        await context.SubscriptionAddOns.AddAsync(addOn, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using (SystemActorContext.Begin<SoftDeleteInterceptorTests>())
        {
            context.SubscriptionAddOns.Remove(addOn);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var raw = await ReadRawAsync(addOn.Id);

        raw.DeletedByUserId.Should().Be(
            $"system-{nameof(SoftDeleteInterceptorTests)}.{nameof(Remove_WithoutUser_InSystemActorScope_WritesSystemLabel)}");
        raw.DeletedByEmail.Should().BeNull();
    }

    [Fact]
    public async Task Remove_WithoutUserAndWithoutScope_WritesSystemUnknown()
    {
        using var context = CreateDbContextWithInterceptors(new SoftDeleteInterceptor(NoUser()));
        var addOn = TestEntities.AddOn();
        await context.SubscriptionAddOns.AddAsync(addOn, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.SubscriptionAddOns.Remove(addOn);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var raw = await ReadRawAsync(addOn.Id);

        raw.DeletedByUserId.Should().Be(SystemActorContext.UnknownActor);
        raw.DeletedByEmail.Should().BeNull();
    }

    [Fact]
    public async Task Remove_WithUser_IgnoresSystemActorScope()
    {
        using var context = CreateDbContextWithInterceptors(new SoftDeleteInterceptor(FakeUser()));
        var addOn = TestEntities.AddOn();
        await context.SubscriptionAddOns.AddAsync(addOn, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using (SystemActorContext.Begin("Seeder", "Run"))
        {
            context.SubscriptionAddOns.Remove(addOn);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var raw = await ReadRawAsync(addOn.Id);

        raw.DeletedByUserId.Should().Be("user-1");
        raw.DeletedByEmail.Should().Be("admin@mygardenplanner.dk");
    }
}