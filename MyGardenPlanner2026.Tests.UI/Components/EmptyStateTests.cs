namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Foundation;
using Xunit;

public sealed class EmptyStateTests : BunitContext
{
    [Fact]
    public void RendersIconTitleAndDescription()
    {
        var cut = Render<EmptyState>(p => p
            .Add(e => e.IconClass, "bi-exclamation-triangle")
            .Add(e => e.Title, "Der opstod en fejl")
            .Add(e => e.Description, "Prøv igen senere."));

        cut.Find(".empty-icon i").ClassList.Should().Contain("bi-exclamation-triangle");
        cut.Find("h2").TextContent.Should().Be("Der opstod en fejl");
        cut.Find("p").TextContent.Should().Be("Prøv igen senere.");
    }

    [Fact]
    public void ErrorVariant_RendersEmptyErrorClass()
    {
        var cut = Render<EmptyState>(p => p
            .Add(e => e.Variant, EmptyState.EmptyStateVariant.Error)
            .Add(e => e.IconClass, "bi-x")
            .Add(e => e.Title, "Fejl"));

        cut.Find(".empty-state").ClassList.Should().Contain("empty-error");
    }

    [Fact]
    public void FirstUseVariant_RendersNoModifierClass()
    {
        var cut = Render<EmptyState>(p => p
            .Add(e => e.IconClass, "bi-info")
            .Add(e => e.Title, "Ingen data endnu"));

        var classList = cut.Find(".empty-state").ClassList;
        classList.Should().Contain("empty-state");
        classList.Should().NotContain("empty-error");
        classList.Should().NotContain("empty-restricted");
    }

    [Fact]
    public void Inline_RendersEmptyStateInlineClass()
    {
        var cut = Render<EmptyState>(p => p
            .Add(e => e.IconClass, "bi-info")
            .Add(e => e.Title, "Titel")
            .Add(e => e.Inline, true));

        cut.Find(".empty-state").ClassList.Should().Contain("empty-state-inline");
    }

    [Fact]
    public void HeadingLevelOne_RendersH1()
    {
        var cut = Render<EmptyState>(p => p
            .Add(e => e.IconClass, "bi-info")
            .Add(e => e.Title, "Titel")
            .Add(e => e.HeadingLevel, 1));

        cut.FindAll("h1").Should().HaveCount(1);
    }

    [Fact]
    public void Actions_RendersInsideEmptyActionsContainer()
    {
        var cut = Render<EmptyState>(p => p
            .Add(e => e.IconClass, "bi-info")
            .Add(e => e.Title, "Titel")
            .Add(e => e.Actions, "<a href=\"/\" class=\"btn btn-primary\">Gå til forsiden</a>"));

        cut.Find(".empty-actions .btn-primary").Should().NotBeNull();
    }

    [Fact]
    public void NoActions_DoesNotRenderEmptyActionsContainer()
    {
        var cut = Render<EmptyState>(p => p
            .Add(e => e.IconClass, "bi-info")
            .Add(e => e.Title, "Titel"));

        cut.FindAll(".empty-actions").Should().BeEmpty();
    }
}