namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Foundation;
using Xunit;

public sealed class ButtonTests : BunitContext
{
    [Fact]
    public void Button_DefaultVariant_RendersBtnPrimary()
    {
        var cut = Render<Button>(p => p.AddChildContent("Gem"));

        cut.Find("button").ClassList.Should().Contain("btn-primary");
        cut.Markup.Should().Contain("Gem");
    }

    [Fact]
    public void Button_Clicked_InvokesOnClick()
    {
        var clicked = false;
        var cut = Render<Button>(p => p
            .Add(b => b.OnClick, () => clicked = true)
            .AddChildContent("Gem"));

        cut.Find("button").Click();

        clicked.Should().BeTrue();
    }

    [Fact]
    public void Button_Loading_IsDisabledAndShowsSpinner_AndDoesNotInvokeOnClick()
    {
        var clicked = false;
        var cut = Render<Button>(p => p
            .Add(b => b.Loading, true)
            .Add(b => b.OnClick, () => clicked = true));

        cut.Find("button").HasAttribute("disabled").Should().BeTrue();
        cut.Find(".btn-spinner").Should().NotBeNull();

        cut.Find("button").Click();
        clicked.Should().BeFalse();
    }

    [Fact]
    public void Button_DangerVariant_RendersBtnDanger()
    {
        var cut = Render<Button>(p => p.Add(b => b.Variant, Button.ButtonVariant.Danger));

        cut.Find("button").ClassList.Should().Contain("btn-danger");
    }
}