namespace MyGardenPlanner2026.Tests.Unit.Services.AuditLog;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Services.AuditLog;
using Xunit;
using static MyGardenPlanner2026.Tests.Unit.Services.AuditLog.AuditLogTestData;

public sealed class AuditLogQueryServiceTests : TestDbContext
{
    private readonly AuditLogQueryService sut;

    public AuditLogQueryServiceTests()
    {
        sut = new AuditLogQueryService(CreateDbContextFactory());
    }

    private Task SeedAsync(params AuditLog[] logs) => AuditLogTestData.SeedAsync(CreateDbContext, logs);

    [Fact]
    public async Task SearchAsync_NoFilter_ReturnsAllRows()
    {
        await SeedAsync(Log(), Log(), Log());

        var result = await sut.SearchAsync(EmptyFilter, TestContext.Current.CancellationToken);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task SearchAsync_FilterByEntityName_ReturnsOnlyMatchingRows()
    {
        await SeedAsync(
            Log(entityName: "SubscriptionTier"),
            Log(entityName: "SubscriptionAddOn"));

        var result = await sut.SearchAsync(
            EmptyFilter with { EntityName = "SubscriptionTier" }, TestContext.Current.CancellationToken);

        result.TotalCount.Should().Be(1);
        result.Items.Single().EntityName.Should().Be("SubscriptionTier");
    }

    [Fact]
    public async Task SearchAsync_FilterByEntityId_ReturnsOnlyMatchingRows()
    {
        await SeedAsync(Log(entityId: "abc"), Log(entityId: "def"));

        var result = await sut.SearchAsync(
            EmptyFilter with { EntityId = "abc" }, TestContext.Current.CancellationToken);

        result.TotalCount.Should().Be(1);
        result.Items.Single().EntityId.Should().Be("abc");
    }

    [Fact]
    public async Task SearchAsync_FilterByUserId_ReturnsOnlyMatchingRows()
    {
        await SeedAsync(Log(userId: "user-1"), Log(userId: "user-2"));

        var result = await sut.SearchAsync(
            EmptyFilter with { UserId = "user-2" }, TestContext.Current.CancellationToken);

        result.TotalCount.Should().Be(1);
        result.Items.Single().UserId.Should().Be("user-2");
    }

    [Fact]
    public async Task SearchAsync_FilterByUserEmail_ReturnsOnlyMatchingRows()
    {
        await SeedAsync(
            Log(userEmail: "alice@example.com"),
            Log(userEmail: "bob@example.com"));

        var result = await sut.SearchAsync(
            EmptyFilter with { UserEmail = "bob@example.com" }, TestContext.Current.CancellationToken);

        result.TotalCount.Should().Be(1);
        result.Items.Single().UserEmail.Should().Be("bob@example.com");
    }

    [Fact]
    public async Task SearchAsync_FilterByAction_ReturnsOnlyMatchingRows()
    {
        await SeedAsync(
            Log(action: AuditAction.Create),
            Log(action: AuditAction.Delete));

        var result = await sut.SearchAsync(
            EmptyFilter with { Action = AuditAction.Delete }, TestContext.Current.CancellationToken);

        result.TotalCount.Should().Be(1);
        result.Items.Single().Action.Should().Be(AuditAction.Delete);
    }

    [Fact]
    public async Task SearchAsync_FromUtcAndToUtc_ReturnsOnlyRowsWithinWindow()
    {
        var now = DateTimeOffset.UtcNow;
        await SeedAsync(
            Log(timestampUtc: now.AddDays(-5)),
            Log(timestampUtc: now.AddDays(-1)),
            Log(timestampUtc: now.AddDays(1)));

        var result = await sut.SearchAsync(
            EmptyFilter with { FromUtc = now.AddDays(-2), ToUtc = now.AddHours(1) }, TestContext.Current.CancellationToken);

        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_CombinedFilters_AppliesAllConditions()
    {
        await SeedAsync(
            Log(entityName: "SubscriptionTier", action: AuditAction.Update, userId: "user-1"),
            Log(entityName: "SubscriptionTier", action: AuditAction.Delete, userId: "user-1"),
            Log(entityName: "SubscriptionAddOn", action: AuditAction.Update, userId: "user-1"));

        var result = await sut.SearchAsync(
            EmptyFilter with { EntityName = "SubscriptionTier", UserId = "user-1", Action = AuditAction.Update },
            TestContext.Current.CancellationToken);

        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_Pagination_ReturnsCorrectPageSlice()
    {
        for (var i = 0; i < 5; i++)
        {
            await SeedAsync(Log(entityId: i.ToString(), timestampUtc: DateTimeOffset.UtcNow.AddMinutes(i)));
        }

        var page1 = await sut.SearchAsync(
            EmptyFilter with { PageNumber = 1, PageSize = 2 }, TestContext.Current.CancellationToken);
        var page2 = await sut.SearchAsync(
            EmptyFilter with { PageNumber = 2, PageSize = 2 }, TestContext.Current.CancellationToken);

        page1.Items.Should().HaveCount(2);
        page2.Items.Should().HaveCount(2);
        page1.TotalCount.Should().Be(5);
        page2.TotalCount.Should().Be(5);
        page1.Items.Select(i => i.EntityId).Should().NotIntersectWith(page2.Items.Select(i => i.EntityId));
    }

    [Fact]
    public async Task SearchAsync_SortDescendingTrue_NewestFirst()
    {
        var now = DateTimeOffset.UtcNow;
        await SeedAsync(
            Log(entityId: "old", timestampUtc: now.AddMinutes(-10)),
            Log(entityId: "new", timestampUtc: now));

        var result = await sut.SearchAsync(
            EmptyFilter with { SortDescending = true }, TestContext.Current.CancellationToken);

        result.Items[0].EntityId.Should().Be("new");
    }

    [Fact]
    public async Task SearchAsync_SortDescendingFalse_OldestFirst()
    {
        var now = DateTimeOffset.UtcNow;
        await SeedAsync(
            Log(entityId: "old", timestampUtc: now.AddMinutes(-10)),
            Log(entityId: "new", timestampUtc: now));

        var result = await sut.SearchAsync(
            EmptyFilter with { SortDescending = false }, TestContext.Current.CancellationToken);

        result.Items[0].EntityId.Should().Be("old");
    }

    [Fact]
    public async Task SearchAsync_NoMatchingRows_ReturnsEmptyResultWithZeroTotalCount()
    {
        await SeedAsync(Log(entityName: "SubscriptionTier"));

        var result = await sut.SearchAsync(
            EmptyFilter with { EntityName = "NonExistentEntity" }, TestContext.Current.CancellationToken);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchAsync_PageNumberLessThanOne_ThrowsArgumentOutOfRangeException(int pageNumber)
    {
        var act = async () => await sut.SearchAsync(EmptyFilter with { PageNumber = pageNumber });

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchAsync_PageSizeLessThanOne_ThrowsArgumentOutOfRangeException(int pageSize)
    {
        var act = async () => await sut.SearchAsync(EmptyFilter with { PageSize = pageSize });

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task CountAsync_IgnoresPagingFields_ReturnsFullMatchCount()
    {
        for (var i = 0; i < 7; i++)
        {
            await SeedAsync(Log(entityName: "SubscriptionTier"));
        }

        var count = await sut.CountAsync(
            EmptyFilter with { EntityName = "SubscriptionTier", PageNumber = 3, PageSize = 2 },
            TestContext.Current.CancellationToken);

        count.Should().Be(7);
    }

    [Fact]
    public async Task CountAsync_FilterMatchesNothing_ReturnsZero()
    {
        await SeedAsync(Log(entityName: "SubscriptionTier"));

        var count = await sut.CountAsync(
            EmptyFilter with { EntityName = "NonExistentEntity" }, TestContext.Current.CancellationToken);

        count.Should().Be(0);
    }

    [Fact]
    public async Task GetDistinctEntityNamesAsync_ReturnsAlphabeticallySortedDistinctNames()
    {
        await SeedAsync(
            Log(entityName: "SubscriptionAddOn"),
            Log(entityName: "SubscriptionTier"),
            Log(entityName: "SubscriptionTier"));

        var names = await sut.GetDistinctEntityNamesAsync(TestContext.Current.CancellationToken);

        names.Should().Equal("SubscriptionAddOn", "SubscriptionTier");
    }

    [Fact]
    public async Task GetDistinctEntityNamesAsync_NoRows_ReturnsEmptyList()
    {
        var names = await sut.GetDistinctEntityNamesAsync(TestContext.Current.CancellationToken);

        names.Should().BeEmpty();
    }
}