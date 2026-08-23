namespace MyGardenPlanner2026.Tests.Unit.Infrastructure;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Interceptors;
using NSubstitute;
using Xunit;

public class AuditLoggingInterceptorTests : TestDbContext
{
    private static ICurrentUserAccessor FakeUser()
    {
        var accessor = Substitute.For<ICurrentUserAccessor>();
        accessor.GetCurrent().Returns(new CurrentUserInfo("user-42", "mikael@example.dk", "10.0.0.5"));
        return accessor;
    }

    [Fact]
    public async Task AddingProtectedEntity_WritesCreateAuditLog()
    {
        var user = FakeUser();
        using var context = CreateDbContextWithInterceptors(
            new SoftDeleteInterceptor(user), new AuditLoggingInterceptor(user));

        var tier = new GardenVolumeDiscountTier { MinGardens = 301, MaxGardens = 400, PriceMultiplier = 0.45m };
        context.GardenVolumeDiscountTiers.Add(tier);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var log = await verifyContext.AuditLogs.SingleAsync(TestContext.Current.CancellationToken);

        log.Action.Should().Be(AuditAction.Create);
        log.EntityName.Should().Be(nameof(GardenVolumeDiscountTier));
        log.EntityId.Should().Be(tier.Id.ToString());
        log.UserEmail.Should().Be("mikael@example.dk");
        log.IpAddress.Should().Be("10.0.0.5");
        log.OldValues.Should().BeNull();
        log.NewValues.Should().Contain("301");
    }

    [Fact]
    public async Task UpdatingProtectedEntity_WritesUpdateAuditLogWithOldAndNewValues()
    {
        var user = FakeUser();
        using var context = CreateDbContextWithInterceptors(
            new SoftDeleteInterceptor(user), new AuditLoggingInterceptor(user));

        var addOn = new SubscriptionAddOn
        {
            Type = AddOnType.ArtefaktpakkeA,
            Name = "A",
            UnitDescription = "Enhed",
            AnnualPrice = 48m,
            MonthlyPrice = 4m,
            PerpetualPrice = 120m
        };
        context.SubscriptionAddOns.Add(addOn);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        addOn.AnnualPrice = 60m;
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var updateLog = await verifyContext.AuditLogs
            .Where(l => l.Action == AuditAction.Update)
            .SingleAsync(TestContext.Current.CancellationToken);

        updateLog.OldValues.Should().Contain("48");
        updateLog.NewValues.Should().Contain("60");
    }

    [Fact]
    public async Task RemovingProtectedEntity_WritesDeleteAuditLog_NotUpdate()
    {
        var user = FakeUser();
        using var context = CreateDbContextWithInterceptors(
            new SoftDeleteInterceptor(user), new AuditLoggingInterceptor(user));

        var addOn = new SubscriptionAddOn
        {
            Type = AddOnType.ArtefaktpakkeB,
            Name = "B",
            UnitDescription = "Enhed",
            AnnualPrice = 24m,
            MonthlyPrice = 2m,
            PerpetualPrice = 60m
        };
        context.SubscriptionAddOns.Add(addOn);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.SubscriptionAddOns.Remove(addOn);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var deleteLog = await verifyContext.AuditLogs
            .Where(l => l.Action == AuditAction.Delete)
            .SingleAsync(TestContext.Current.CancellationToken);

        deleteLog.EntityId.Should().Be(addOn.Id.ToString());
        deleteLog.NewValues.Should().BeNull();
        deleteLog.OldValues.Should().Contain("24");
    }

    [Fact]
    public async Task CreateThenUpdateThenDelete_ProducesExactlyThreeAuditLogsInOrder()
    {
        var user = FakeUser();
        using var context = CreateDbContextWithInterceptors(
            new SoftDeleteInterceptor(user), new AuditLoggingInterceptor(user));

        var tier = new GardenVolumeDiscountTier { MinGardens = 601, MaxGardens = 700, PriceMultiplier = 0.30m };
        context.GardenVolumeDiscountTiers.Add(tier);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        tier.PriceMultiplier = 0.25m;
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.GardenVolumeDiscountTiers.Remove(tier);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var logs = (await verifyContext.AuditLogs
            .Where(l => l.EntityId == tier.Id.ToString() && l.EntityName == nameof(GardenVolumeDiscountTier))
            .ToListAsync(TestContext.Current.CancellationToken))
            .OrderBy(l => l.TimestampUtc)
            .ToList();

        logs.Should().HaveCount(3);
        logs[0].Action.Should().Be(AuditAction.Create);
        logs[1].Action.Should().Be(AuditAction.Update);
        logs[2].Action.Should().Be(AuditAction.Delete);
    }
}