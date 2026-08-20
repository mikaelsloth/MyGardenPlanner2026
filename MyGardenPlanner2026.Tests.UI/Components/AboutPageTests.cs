namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Pages;
using Xunit;

public class AboutPageTests : BunitContext
{
    [Fact]
    public void AboutPage_RendersFourFeatureCards()
    {
        var cut = Render<AboutPage>();

        cut.FindAll(".card-entity").Should().HaveCount(4);
        cut.Markup.Should().Contain("Projektstyring");
        cut.Markup.Should().Contain("Smart Budget");
        cut.Markup.Should().Contain("Botanisk Opslag");
        cut.Markup.Should().Contain("Billedarkiv");
    }

    [Fact]
    public void AboutPage_RendersTechnologyCreditsAndContactSections()
    {
        var cut = Render<AboutPage>();

        cut.Markup.Should().Contain("Teknologi");
        cut.Markup.Should().Contain("Krediteringer");
        cut.Markup.Should().Contain("Kontakt");
    }
}