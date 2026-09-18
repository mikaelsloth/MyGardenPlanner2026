namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Foundation;
using Xunit;

public sealed class PermissionHintTests : BunitContext
{
    [Fact]
    public void RendersChildContentInsidePermissionHintParagraph()
    {
        var cut = Render<PermissionHint>(p => p.AddChildContent("Tilkøb abonnement for at oprette ubegrænsede bede"));

        var paragraph = cut.Find("p.permission-hint");
        paragraph.TextContent.Should().Be("Tilkøb abonnement for at oprette ubegrænsede bede");
    }
}