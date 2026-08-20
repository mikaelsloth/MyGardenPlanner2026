namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Domain.Marketing;
using Xunit;

public class HeroBannerTests : BunitContext
{
    [Fact]
    public void HeroBanner_RendersTitleAndSubtitle()
    {
        var cut = Render<HeroBanner>();

        cut.Markup.Should().Contain("Planlæg, design og dyrk din drømmehave");
        cut.Markup.Should().Contain("MyGardenPlanner samler alle dine havedata ét sted.");
    }

    [Fact]
    public void HeroBanner_RendersPrimaryAndSecondaryCtaWithCorrectLinksAndClasses()
    {
        var cut = Render<HeroBanner>();

        var primary = cut.Find("a[href='/account/register']");
        primary.ClassList.Should().Contain("btn-primary");
        primary.TextContent.Should().Contain("Opret gratis konto");

        var secondary = cut.Find("a[href='/pricing']");
        secondary.ClassList.Should().Contain("btn-secondary");
        secondary.TextContent.Should().Contain("Se abonnementer");
    }
}