namespace MyGardenPlanner2026.Tests.UI.Components.Account.Shared;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using MyGardenPlanner2026.Components.Account.Shared;
using Xunit;

public sealed class StepUpReAuthModalFocusTests : BunitContext
{
    private readonly FakeFocusTrapService focusTrap;

    public StepUpReAuthModalFocusTests()
    {
        this.RegisterIdentityFakes();
        this.RegisterReAuthFakes<StepUpReAuthModal>();
        focusTrap = this.AddFakeFocusTrap();
    }

    [Fact]
    public void WhenOpened_ActivatesFocusTrap()
    {
        Render<StepUpReAuthModal>(p => p.Add(x => x.IsOpen, true));

        focusTrap.ActivateCount.Should().Be(1);
    }

    [Fact]
    public void WhenClosed_DoesNotActivateFocusTrap()
    {
        Render<StepUpReAuthModal>(p => p.Add(x => x.IsOpen, false));

        focusTrap.ActivateCount.Should().Be(0);
    }

    [Fact]
    public void WhenClosedAfterOpen_ReleasesFocusTrap()
    {
        var cut = Render<StepUpReAuthModal>(p => p.Add(x => x.IsOpen, true));

        cut.Render(p => p.Add(x => x.IsOpen, false));

        cut.WaitForAssertion(() => focusTrap.DisposedCount.Should().Be(1));
    }

    [Fact]
    public async Task DisposedWhileOpen_ReleasesFocusTrap()
    {
        var cut = Render<StepUpReAuthModal>(p => p.Add(x => x.IsOpen, true));

        await cut.Instance.DisposeAsync();

        focusTrap.DisposedCount.Should().Be(1);
    }

    [Fact]
    public void PressingEscape_InvokesOnCancel()
    {
        var cancelled = false;
        var cut = Render<StepUpReAuthModal>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.OnCancel, () => cancelled = true));

        cut.Find(".confirm-dialog").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        cancelled.Should().BeTrue();
    }

    [Fact]
    public void PressingEscape_ClearsEnteredPassword()
    {
        var cut = Render<StepUpReAuthModal>(p => p.Add(x => x.IsOpen, true));
        cut.Find("#step-up-password").Change("hemmeligt");

        cut.Find(".confirm-dialog").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        cut.Find("#step-up-password").GetAttribute("value").Should().BeNullOrEmpty();
    }

    [Fact]
    public void PressingOtherKey_DoesNotInvokeOnCancel()
    {
        var cancelled = false;
        var cut = Render<StepUpReAuthModal>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.OnCancel, () => cancelled = true));

        cut.Find(".confirm-dialog").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        cancelled.Should().BeFalse();
    }

    [Fact]
    public void Backdrop_HasNoClickHandler_SoClickingOutsideDoesNotCancel()
    {
        var cancelled = false;
        var cut = Render<StepUpReAuthModal>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.OnCancel, () => cancelled = true));

        Action click = () => cut.Find(".confirm-dialog-backdrop").Click();

        click.Should().Throw<MissingEventHandlerException>(
            "modalen beskytter en følsom handling og må ikke lukkes ved et uforvarende klik");
        cancelled.Should().BeFalse();
    }

    [Fact]
    public void PasswordField_IsInitialFocus_AndDialogIsProgrammaticallyFocusable()
    {
        var cut = Render<StepUpReAuthModal>(p => p.Add(x => x.IsOpen, true));

        cut.Find("[data-autofocus]").Id.Should().Be("step-up-password");
        cut.Find(".confirm-dialog").GetAttribute("tabindex").Should().Be("-1");
    }
}