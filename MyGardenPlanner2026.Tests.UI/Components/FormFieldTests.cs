namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Interaction;
using Xunit;

public sealed class FormFieldTests : BunitContext
{
    [Fact]
    public void FormField_RendersLabelForCorrectInputId()
    {
        var cut = Render<FormField>(p => p
            .Add(f => f.For, "email")
            .Add(f => f.Label, "E-mail"));

        cut.Find("label").GetAttribute("for").Should().Be("email");
        cut.Find("label").TextContent.Should().Contain("E-mail");
    }

    [Fact]
    public void FormField_Required_ShowsRequiredLabel()
    {
        var cut = Render<FormField>(p => p
            .Add(f => f.For, "email")
            .Add(f => f.Label, "E-mail")
            .Add(f => f.Required, true));

        cut.Find(".required-label").Should().NotBeNull();
    }

    [Fact]
    public void FormField_WithErrorMessage_RendersFieldMessageAndErrorClass()
    {
        var cut = Render<FormField>(p => p
            .Add(f => f.For, "email")
            .Add(f => f.Label, "E-mail")
            .Add(f => f.ErrorMessage, "Ugyldig e-mail"));

        cut.Find(".field").ClassList.Should().Contain("field-error");
        cut.Find(".field-message").TextContent.Should().Be("Ugyldig e-mail");
    }
}