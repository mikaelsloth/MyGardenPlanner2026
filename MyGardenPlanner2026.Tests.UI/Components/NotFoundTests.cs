namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Pages;
using Xunit;

public class NotFoundTests : BunitContext
{
    [Fact]
    public void NotFound_RendersDanishHeadingAndReturnLink()
    {
        var cut = Render<NotFound>();

        cut.Find("h1").TextContent.Should().Be("Siden blev ikke fundet");
        cut.Markup.Should().Contain("findes ikke");
        cut.Find("a.btn-primary[href='/']").TextContent.Should().Contain("forsiden");
    }

    [Fact]
    public void NotFound_UsesNeutralEmptyStateVariant_NotDangerColored()
    {
        var cut = Render<NotFound>();

        cut.Find(".empty-state").ClassList.Should().Contain("empty-search");
        cut.FindAll(".empty-error").Should().BeEmpty();
    }
}