namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Layout;
using Xunit;

public class PageHeaderTests : BunitContext
{
    [Fact]
    public void PageHeader_RendersTitleIntroAndLastUpdated()
    {
        var cut = Render<PageHeader>(p => p
            .Add(x => x.Title, "Handelsbetingelser")
            .Add(x => x.Intro, "En kort introduktion.")
            .Add(x => x.LastUpdated, "20. august 2026"));

        cut.Find("h1").TextContent.Should().Be("Handelsbetingelser");
        cut.Markup.Should().Contain("En kort introduktion.");
        cut.Markup.Should().Contain("20. august 2026");
    }

    [Fact]
    public void PageHeader_WhenIntroAndLastUpdatedAreNull_OnlyRendersTitle()
    {
        var cut = Render<PageHeader>(p => p.Add(x => x.Title, "Om platformen"));

        cut.FindAll(".page-header-intro").Should().BeEmpty();
        cut.FindAll(".page-header-meta").Should().BeEmpty();
    }
}