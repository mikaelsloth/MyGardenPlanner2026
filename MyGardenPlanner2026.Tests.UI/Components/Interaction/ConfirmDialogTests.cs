namespace MyGardenPlanner2026.Tests.UI.Components.Interaction;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using MyGardenPlanner2026.Components.Interaction;
using Xunit;

public sealed class ConfirmDialogTests : BunitContext
{
    private readonly FakeFocusTrapService focusTrap;

    public ConfirmDialogTests()
    {
        focusTrap = this.AddFakeFocusTrap();
    }

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

    [Fact]
    public void WhenOpened_ActivatesFocusTrap()
    {
        Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked"));

        focusTrap.ActivateCount.Should().Be(1);
    }

    [Fact]
    public void WhenClosed_DoesNotActivateFocusTrap()
    {
        Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, false)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked"));

        focusTrap.ActivateCount.Should().Be(0);
    }

    [Fact]
    public void WhenClosedAfterOpen_ReleasesFocusTrap()
    {
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked"));

        cut.Render(p => p.Add(d => d.IsOpen, false));

        cut.WaitForAssertion(() => focusTrap.DisposedCount.Should().Be(1));
    }

    [Fact]
    public async Task DisposedWhileOpen_ReleasesFocusTrap()
    {
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked"));

        await cut.Instance.DisposeAsync();

        focusTrap.DisposedCount.Should().Be(1);
    }

    [Fact]
    public void PressingEscape_InvokesOnCancel()
    {
        var cancelled = false;
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked")
            .Add(d => d.OnCancel, () => cancelled = true));

        cut.Find(".confirm-dialog").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        cancelled.Should().BeTrue();
    }

    [Fact]
    public void PressingEscape_WhileSubmitting_DoesNotInvokeOnCancel()
    {
        var cancelled = false;
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked")
            .Add(d => d.IsSubmitting, true)
            .Add(d => d.OnCancel, () => cancelled = true));

        cut.Find(".confirm-dialog").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        cancelled.Should().BeFalse();
    }

    [Fact]
    public void PressingOtherKey_DoesNotInvokeOnCancel()
    {
        var cancelled = false;
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked")
            .Add(d => d.OnCancel, () => cancelled = true));

        cut.Find(".confirm-dialog").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        cancelled.Should().BeFalse();
    }

    [Fact]
    public void Dangerous_MarksCancelButtonAsInitialFocus()
    {
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.IsDangerous, true)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked"));

        cut.Find(".confirm-dialog-actions .btn-secondary").HasAttribute("data-autofocus").Should().BeTrue();
    }

    [Fact]
    public void NotDangerous_HasNoExplicitInitialFocus()
    {
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.IsDangerous, false)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked"));

        cut.FindAll("[data-autofocus]").Should().BeEmpty();
    }

    [Fact]
    public void Dialog_IsProgrammaticallyFocusable()
    {
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked"));

        cut.Find(".confirm-dialog").GetAttribute("tabindex").Should().Be("-1");
    }
}