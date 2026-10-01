namespace MyGardenPlanner2026.Tests.Unit.Services.AuditLog;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.Services.AuditLog;
using Xunit;
using static MyGardenPlanner2026.Tests.Unit.Services.AuditLog.AuditLogTestData;

/// <summary>
/// Verificerer, at AuditLogQueryService's server-side (Database.IsSqlServer())
/// filtrerings-/sorterings-/pagineringssti oversættes korrekt til SQL. Kræver en
/// reel SQL Express-instans (se TestSqlExpressDbContext for konfiguration).
/// </summary>
[Trait("Category", "SqlServerIntegration")]
public sealed class AuditLogQuerySqlServerTests : TestSqlExpressDbContext
{
    /// <summary>
    /// Test-lokal adapter: TestSqlExpressDbContext eksponerer kun CreateDbContext()
    /// (én PlannerDbContext pr. kald, migreret én gang pr. testinstans). Denne
    /// wrapper opfylder IDbContextFactory&lt;PlannerDbContext&gt;, som
    /// AuditLogQueryService kræver — samme delegerings-mønster som AdminDbContextFactory.
    /// </summary>
    private sealed class DelegatingDbContextFactory(Func<PlannerDbContext> factory)
        : IDbContextFactory<PlannerDbContext>
    {
        public PlannerDbContext CreateDbContext() => factory();

        public Task<PlannerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(factory());
    }

    private readonly AuditLogQueryService sut;

    public AuditLogQuerySqlServerTests()
    {
        sut = new AuditLogQueryService(new DelegatingDbContextFactory(CreateDbContext));
    }

    private Task SeedAsync(params AuditLog[] logs) => AuditLogTestData.SeedAsync(CreateDbContext, logs);

    [Fact]
    public async Task SearchAsync_OnSqlServer_FilterByEntityNameAndAction_ReturnsOnlyMatchingRows()
    {
        await SeedAsync(
            Log(entityName: "SubscriptionTier", action: AuditAction.Update),
            Log(entityName: "SubscriptionTier", action: AuditAction.Delete),
            Log(entityName: "SubscriptionAddOn", action: AuditAction.Update));

        var result = await sut.SearchAsync(
            EmptyFilter with { EntityName = "SubscriptionTier", Action = AuditAction.Update },
            TestContext.Current.CancellationToken);

        result.TotalCount.Should().Be(1);
        result.Items.Single().EntityName.Should().Be("SubscriptionTier");
    }

    [Fact]
    public async Task SearchAsync_OnSqlServer_PaginationAndSorting_ReturnsCorrectSlice()
    {
        var now = DateTimeOffset.UtcNow;
        for (var i = 0; i < 5; i++)
        {
            await SeedAsync(Log(entityId: i.ToString(), timestampUtc: now.AddMinutes(i)));
        }

        var page1 = await sut.SearchAsync(
            EmptyFilter with { PageNumber = 1, PageSize = 2, SortDescending = true },
            TestContext.Current.CancellationToken);

        page1.Items.Should().HaveCount(2);
        page1.TotalCount.Should().Be(5);
        page1.Items[0].EntityId.Should().Be("4");
    }

    [Fact]
    public async Task CountAsync_OnSqlServer_MatchesFilteredRowCount_IgnoringPaging()
    {
        for (var i = 0; i < 4; i++)
        {
            await SeedAsync(Log(entityName: "SubscriptionAddOn"));
        }

        var count = await sut.CountAsync(
            EmptyFilter with { EntityName = "SubscriptionAddOn", PageNumber = 1, PageSize = 1 },
            TestContext.Current.CancellationToken);

        count.Should().Be(4);
    }
}