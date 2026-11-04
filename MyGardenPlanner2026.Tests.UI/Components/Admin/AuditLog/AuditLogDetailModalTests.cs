namespace MyGardenPlanner2026.Tests.UI.Components.Admin.AuditLog;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Globalization;
using Xunit;

public class AuditLogDetailModalTests : BunitContext
{
    private static AuditLogEntryDto Entry(string? oldValues = "{\"Name\":\"Old\"}", string? newValues = "{\"Name\":\"New\"}") =>
        AuditLogTestData.Entry(entityId: "abc-123", oldValues: oldValues, newValues: newValues);

    [Fact]
    public void IsOpenFalse_RendersNothing()
    {
        var cut = Render<AuditLogDetailModal>(p => p
            .Add(x => x.IsOpen, false)
            .Add(x => x.Entry, Entry()));

        cut.Markup.Should().BeEmpty();
    }

    [Fact]
    public void IsOpenTrue_RendersEntityNameAndIdInTitle()
    {
        var cut = Render<AuditLogDetailModal>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.Entry, Entry()));

        cut.Find("#audit-log-detail-title").TextContent.Should().Contain("SubscriptionTier").And.Contain("abc-123");
    }

    [Fact]
    public void ValidJson_IsPrettyPrintedInBothColumns()
    {
        var cut = Render<AuditLogDetailModal>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.Entry, Entry()));

        var jsonBlocks = cut.FindAll(".audit-log-detail-json");

        jsonBlocks[0].TextContent.Should().Contain("Old");
        jsonBlocks[1].TextContent.Should().Contain("New");
    }

    [Fact]
    public void NullOldValues_ShowsNoDataPlaceholder()
    {
        var cut = Render<AuditLogDetailModal>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.Entry, Entry(oldValues: null)));

        cut.FindAll(".audit-log-detail-json")[0].TextContent.Should().Contain("(ingen data)");
    }

    [Fact]
    public void InvalidJson_FallsBackToRawText()
    {
        var cut = Render<AuditLogDetailModal>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.Entry, Entry(oldValues: "not-json")));

        cut.FindAll(".audit-log-detail-json")[0].TextContent.Should().Contain("not-json");
    }

    [Fact]
    public void ClickingLukButton_InvokesOnClose()
    {
        var closed = false;

        var cut = Render<AuditLogDetailModal>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.Entry, Entry())
            .Add(x => x.OnClose, () => closed = true));

        cut.Find(".confirm-dialog-actions button").Click();

        closed.Should().BeTrue();
    }

    [Fact]
    public void ClickingBackdrop_InvokesOnClose()
    {
        var closed = false;

        var cut = Render<AuditLogDetailModal>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.Entry, Entry())
            .Add(x => x.OnClose, () => closed = true));

        cut.Find(".confirm-dialog-backdrop").Click();

        closed.Should().BeTrue();
    }

    [Fact]
    public void DialogElement_HasStopPropagationAttribute_ToPreventBackdropCloseOnInnerClicks()
    {
        var closed = false;

        var cut = Render<AuditLogDetailModal>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.Entry, Entry())
            .Add(x => x.OnClose, () => closed = true));

        cut.Find(".audit-log-detail-dialog").HasAttribute("blazor:onclick:stoppropagation").Should().BeTrue();
        closed.Should().BeFalse();
    }

    [Fact]
    public void MetaLine_ShowsTimestampInUtcWithSuffix()
    {
        var entry = new AuditLogEntryDto(
            1, "user-1", "user1@example.com", "127.0.0.1", AuditAction.Update,
            "SubscriptionTier", "abc-123", null, null,
            new DateTimeOffset(2026, 10, 16, 1, 30, 0, TimeSpan.FromHours(2)));

        var cut = Render<AuditLogDetailModal>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.Entry, entry));

        var expected = $"{new DateTime(2026, 10, 15, 23, 30, 0).ToString("g", new CultureInfo("da-DK"))} UTC";
        cut.Find("p.meta").TextContent.Should().Contain(expected);
    }
}