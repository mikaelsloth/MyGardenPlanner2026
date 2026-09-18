namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Foundation;
using Xunit;

public sealed class CardTests : BunitContext
{
    [Fact]
    public void DefaultVariant_RendersBaseCardClassOnly()
    {
        var cut = Render<Card>();

        var classList = cut.Find(".card").ClassList;
        classList.Should().Contain("card");
        classList.Should().NotContain("card-entity");
    }

    [Fact]
    public void EntityVariant_RendersCardEntityClass()
    {
        var cut = Render<Card>(p => p.Add(c => c.Variant, Card.CardVariant.Entity));

        cut.Find(".card").ClassList.Should().Contain("card-entity");
    }

    [Fact]
    public void ArchivedVariant_RendersCardEntityAndCardArchivedClasses()
    {
        var cut = Render<Card>(p => p.Add(c => c.Variant, Card.CardVariant.Archived));

        var classList = cut.Find(".card").ClassList;
        classList.Should().Contain("card-entity");
        classList.Should().Contain("card-archived");
    }

    [Fact]
    public void Title_RendersInsideCardHeader()
    {
        var cut = Render<Card>(p => p.Add(c => c.Title, "Mit kort"));

        cut.Find(".card-header").TextContent.Should().Be("Mit kort");
    }

    [Fact]
    public void NoTitleAndNoHeaderExtra_DoesNotRenderCardHeader()
    {
        var cut = Render<Card>();

        cut.FindAll(".card-header").Should().BeEmpty();
    }

    [Fact]
    public void Description_RendersCardDescription()
    {
        var cut = Render<Card>(p => p.Add(c => c.Description, "En beskrivelse"));

        cut.Find(".card-description").TextContent.Should().Be("En beskrivelse");
    }

    [Fact]
    public void Actions_RendersInsideCardActions()
    {
        var cut = Render<Card>(p => p.Add(c => c.Actions, "<button class=\"btn btn-primary\">Gem</button>"));

        cut.Find(".card-actions .btn-primary").Should().NotBeNull();
    }

    [Fact]
    public void NoActions_DoesNotRenderCardActions()
    {
        var cut = Render<Card>();

        cut.FindAll(".card-actions").Should().BeEmpty();
    }

    [Fact]
    public void ChildContent_RendersInsideCard()
    {
        var cut = Render<Card>(p => p.AddChildContent("<p class=\"custom-content\">Indhold</p>"));

        cut.Find(".custom-content").Should().NotBeNull();
    }
}