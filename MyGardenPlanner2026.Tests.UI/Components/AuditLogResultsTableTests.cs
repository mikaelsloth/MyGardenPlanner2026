namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public class AuditLogResultsTableTests : BunitContext
{
    private static AuditLogEntryDto Entry(long id = 1, AuditAction action = AuditAction.Update) => new(
        id, "user-1", "user1@example.com", "127.0.0.1", action,
        "SubscriptionTier", "abc", "{\"Name\":\"Old\"}", "{\"Name\":\"New\"}",
        new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void NullResult_RendersSkeletonCard()
    {
        var cut = Render<AuditLogResultsTable>(p => p.Add(x => x.Result, null));

        cut.Find(".skeleton-card").Should().NotBeNull();
    }

    [Fact]
    public void EmptyItems_RendersEmptyState()
    {
        var result = new AuditLogQueryResultDto([], 0, 1, 25);

        var cut = Render<AuditLogResultsTable>(p => p.Add(x => x.Result, result));

        cut.Find(".empty-state").Should().NotBeNull();
        cut.FindAll("tbody tr").Should().BeEmpty();
    }

    [Fact]
    public void ItemsPresent_RendersOneRowPerEntry()
    {
        var result = new AuditLogQueryResultDto([Entry(1), Entry(2)], 2, 1, 25);

        var cut = Render<AuditLogResultsTable>(p => p.Add(x => x.Result, result));

        cut.FindAll("tbody tr").Should().HaveCount(2);
    }

    [Theory]
    [InlineData(AuditAction.Create, "badge-primary")]
    [InlineData(AuditAction.Update, "badge-accent")]
    [InlineData(AuditAction.Delete, "badge-danger-soft")]
    public void ActionBadge_UsesCorrectCssClassPerAction(AuditAction action, string expectedClass)
    {
        var result = new AuditLogQueryResultDto([Entry(action: action)], 1, 1, 25);

        var cut = Render<AuditLogResultsTable>(p => p.Add(x => x.Result, result));

        cut.Find(".badge-role").ClassList.Should().Contain(expectedClass);
    }

    [Fact]
    public void ClickingDetaljerButton_InvokesOnViewDetailsWithCorrectEntry()
    {
        var entry = Entry(7);
        var result = new AuditLogQueryResultDto([entry], 1, 1, 25);
        AuditLogEntryDto? received = null;

        var cut = Render<AuditLogResultsTable>(p => p
            .Add(x => x.Result, result)
            .Add(x => x.OnViewDetails, e => received = e));

        cut.Find("tbody tr button").Click();

        received.Should().Be(entry);
    }

    [Fact]
    public void FirstPage_ForrigeButtonIsDisabled()
    {
        var result = new AuditLogQueryResultDto([Entry()], 100, 1, 25);

        var cut = Render<AuditLogResultsTable>(p => p.Add(x => x.Result, result));

        cut.FindAll(".audit-log-pagination button")[0].HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void LastPage_NaesteButtonIsDisabled()
    {
        var result = new AuditLogQueryResultDto([Entry()], 100, 4, 25);

        var cut = Render<AuditLogResultsTable>(p => p.Add(x => x.Result, result));

        cut.FindAll(".audit-log-pagination button")[1].HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void ClickingNaeste_InvokesOnPageChangedWithIncrementedPageNumber()
    {
        var result = new AuditLogQueryResultDto([Entry()], 100, 2, 25);
        int? receivedPage = null;

        var cut = Render<AuditLogResultsTable>(p => p
            .Add(x => x.Result, result)
            .Add(x => x.OnPageChanged, page => receivedPage = page));

        cut.FindAll(".audit-log-pagination button")[1].Click();

        receivedPage.Should().Be(3);
    }

    [Fact]
    public void ClickingForrige_InvokesOnPageChangedWithDecrementedPageNumber()
    {
        var result = new AuditLogQueryResultDto([Entry()], 100, 2, 25);
        int? receivedPage = null;

        var cut = Render<AuditLogResultsTable>(p => p
            .Add(x => x.Result, result)
            .Add(x => x.OnPageChanged, page => receivedPage = page));

        cut.FindAll(".audit-log-pagination button")[0].Click();

        receivedPage.Should().Be(1);
    }
}