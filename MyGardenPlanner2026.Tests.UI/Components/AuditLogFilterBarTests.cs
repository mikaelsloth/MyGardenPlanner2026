namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public class AuditLogFilterBarTests : BunitContext
{
    private static AuditLogFilterDto EmptyFilter() => new(null, null, null, null, null, null, null);

    [Fact]
    public void AuditLogFilterBar_RendersOneOptionPerEntityNamePlusAlle()
    {
        var cut = Render<AuditLogFilterBar>(p => p
            .Add(x => x.InitialFilter, EmptyFilter())
            .Add(x => x.EntityNameOptions, ["SubscriptionTier", "SubscriptionAddOn"]));

        cut.FindAll("#audit-filter-entity option").Should().HaveCount(3);
        cut.Markup.Should().Contain("SubscriptionTier");
        cut.Markup.Should().Contain("SubscriptionAddOn");
    }

    [Fact]
    public void AuditLogFilterBar_InitialFilter_PrefillsEntityIdField()
    {
        var filter = EmptyFilter() with { EntityId = "abc-123" };

        var cut = Render<AuditLogFilterBar>(p => p
            .Add(x => x.InitialFilter, filter)
            .Add(x => x.EntityNameOptions, []));

        cut.Find("#audit-filter-entity-id").GetAttribute("value").Should().Be("abc-123");
    }

    [Fact]
    public void ClickingSoeg_WithAllFieldsEmpty_InvokesOnSearchWithAllNullFilter()
    {
        AuditLogFilterDto? emitted = null;

        var cut = Render<AuditLogFilterBar>(p => p
            .Add(x => x.InitialFilter, EmptyFilter())
            .Add(x => x.EntityNameOptions, [])
            .Add(x => x.OnSearch, filter => emitted = filter));

        cut.Find(".filter-bar-actions button").Click();

        emitted.Should().NotBeNull();
        emitted!.EntityName.Should().BeNull();
        emitted.EntityId.Should().BeNull();
        emitted.UserId.Should().BeNull();
        emitted.UserEmail.Should().BeNull();
        emitted.Action.Should().BeNull();
        emitted.PageNumber.Should().Be(1);
    }

    [Fact]
    public void ClickingSoeg_AfterFillingEntityIdAndUserEmail_InvokesOnSearchWithThoseValues()
    {
        AuditLogFilterDto? emitted = null;

        var cut = Render<AuditLogFilterBar>(p => p
            .Add(x => x.InitialFilter, EmptyFilter())
            .Add(x => x.EntityNameOptions, [])
            .Add(x => x.OnSearch, filter => emitted = filter));

        cut.Find("#audit-filter-entity-id").Change("abc-123");
        cut.Find("#audit-filter-user-email").Change("alice@example.com");
        cut.Find(".filter-bar-actions button").Click();

        emitted!.EntityId.Should().Be("abc-123");
        emitted.UserEmail.Should().Be("alice@example.com");
    }

    [Fact]
    public void ClickingSoeg_AfterSelectingActionAndPageSize_InvokesOnSearchWithThoseValues()
    {
        AuditLogFilterDto? emitted = null;

        var cut = Render<AuditLogFilterBar>(p => p
            .Add(x => x.InitialFilter, EmptyFilter())
            .Add(x => x.EntityNameOptions, [])
            .Add(x => x.OnSearch, filter => emitted = filter));

        cut.Find("#audit-filter-action").Change(nameof(AuditAction.Delete));
        cut.Find("#audit-filter-pagesize").Change("50");
        cut.Find(".filter-bar-actions button").Click();

        emitted!.Action.Should().Be(AuditAction.Delete);
        emitted.PageSize.Should().Be(50);
    }

    [Fact]
    public void ClickingSoeg_AlwaysResetsPageNumberToOne()
    {
        AuditLogFilterDto? emitted = null;

        var cut = Render<AuditLogFilterBar>(p => p
            .Add(x => x.InitialFilter, EmptyFilter() with { PageNumber = 4 })
            .Add(x => x.EntityNameOptions, [])
            .Add(x => x.OnSearch, filter => emitted = filter));

        cut.Find(".filter-bar-actions button").Click();

        emitted!.PageNumber.Should().Be(1);
    }
}