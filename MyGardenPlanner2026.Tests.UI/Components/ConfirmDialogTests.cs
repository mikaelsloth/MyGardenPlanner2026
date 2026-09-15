namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Interaction;
using Xunit;

public sealed class ConfirmDialogTests : BunitContext
{
    [Fact]
    public void IsOpenFalse_RendersNothing()
    {
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, false)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked"));

        cut.Markup.Should().BeEmpty();
    }

    [Fact]
    public void IsOpenTrue_RendersTitleAndMessage()
    {
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.Title, "Er du sikker?")
            .Add(d => d.Message, "Handlingen kan ikke fortrydes."));

        cut.Find(".confirm-dialog-title").TextContent.Should().Be("Er du sikker?");
        cut.Markup.Should().Contain("Handlingen kan ikke fortrydes.");
    }

    [Fact]
    public void ClickingConfirm_InvokesOnConfirm()
    {
        var confirmed = false;
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked")
            .Add(d => d.OnConfirm, () => confirmed = true));

        cut.Find(".confirm-dialog-actions .btn-danger").Click();

        confirmed.Should().BeTrue();
    }

    [Fact]
    public void ClickingCancel_InvokesOnCancel()
    {
        var cancelled = false;
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked")
            .Add(d => d.OnCancel, () => cancelled = true));

        cut.Find(".confirm-dialog-actions .btn-secondary").Click();

        cancelled.Should().BeTrue();
    }

    [Fact]
    public void ClickingBackdrop_WithCloseOnBackdropClickTrue_InvokesOnCancel()
    {
        var cancelled = false;
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked")
            .Add(d => d.OnCancel, () => cancelled = true));

        cut.Find(".confirm-dialog-backdrop").Click();

        cancelled.Should().BeTrue();
    }

    [Fact]
    public void IsSubmitting_DisablesBothButtons_AndShowsSpinner()
    {
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked")
            .Add(d => d.IsSubmitting, true));

        cut.FindAll(".confirm-dialog-actions button")
            .Should().OnlyContain(b => b.HasAttribute("disabled"));
        cut.Find(".btn-spinner").Should().NotBeNull();
    }
}