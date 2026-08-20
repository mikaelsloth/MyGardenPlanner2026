namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Layout;
using Xunit;

public class AuthPageShellTests : BunitContext
{
    [Fact]
    public void AuthPageShell_RendersTitleSubtitleAndChildContent()
    {
        var cut = Render<AuthPageShell>(p => p
            .Add(x => x.Title, "Log ind")
            .Add(x => x.Subtitle, "Velkommen tilbage til din have.")
            .AddChildContent("<p>Formular-indhold</p>"));

        cut.Find("h1").TextContent.Should().Be("Log ind");
        cut.Markup.Should().Contain("Velkommen tilbage til din have.");
        cut.Markup.Should().Contain("Formular-indhold");
    }

    [Fact]
    public void AuthPageShell_WhenSubtitleIsNull_DoesNotRenderSubtitleParagraph()
    {
        var cut = Render<AuthPageShell>(p => p
            .Add(x => x.Title, "Opret bruger")
            .AddChildContent("<p>Formular</p>"));

        cut.FindAll(".auth-form-subtitle").Should().BeEmpty();
    }

    [Fact]
    public void AuthPageShell_RendersBrandPanelWithThreeFeatures()
    {
        var cut = Render<AuthPageShell>(p => p
            .Add(x => x.Title, "Log ind")
            .AddChildContent("<p>Formular</p>"));

        cut.FindAll(".auth-brand-feature").Should().HaveCount(3);
    }
}