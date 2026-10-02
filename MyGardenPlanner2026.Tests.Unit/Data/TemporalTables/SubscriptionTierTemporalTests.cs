namespace MyGardenPlanner2026.Tests.Unit.Data.TemporalTables;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Trait("Category", "SqlServerIntegration")]
public class SubscriptionTierTemporalTests : TestSqlExpressDbContext
{
    [Fact]
    public async Task UpdatingAnnualPrice_IsRecoverable_ViaTemporalHistoryQuery()
    {
        using var context = CreateDbContext();

        var tier = TestEntities.Tier(name: "Temporal Test Tier");
        context.Add(tier);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var beforeUpdate = DateTime.UtcNow;
        await Task.Delay(50, TestContext.Current.CancellationToken);

        tier.AnnualPrice = 150m;
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var historicValue = await context.SubscriptionTiers
            .TemporalAsOf(beforeUpdate)
            .SingleAsync(t => t.Id == tier.Id, TestContext.Current.CancellationToken);

        historicValue.AnnualPrice.Should().Be(100m);

        var currentValue = await context.SubscriptionTiers
            .SingleAsync(t => t.Id == tier.Id, TestContext.Current.CancellationToken);

        currentValue.AnnualPrice.Should().Be(150m);
    }

    [Fact]
    public async Task TemporalAll_ReturnsBothOriginalAndUpdatedRow_AfterOneChange()
    {
        using var context = CreateDbContext();

        var addOn = TestEntities.AddOn(name: "Temporal AddOn Test");
        context.Add(addOn);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        addOn.AnnualPrice = 60m;
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var historyRows = await context.SubscriptionAddOns
            .TemporalAll()
            .Where(a => a.Id == addOn.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        historyRows.Should().HaveCountGreaterThanOrEqualTo(2);
        historyRows.Should().Contain(a => a.AnnualPrice == 48m);
        historyRows.Should().Contain(a => a.AnnualPrice == 60m);
    }
}