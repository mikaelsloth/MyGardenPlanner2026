namespace MyGardenPlanner2026.Tests.UI.Components.Account.Shared;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class StepUpReAuthModalTests : BunitContext
{
    private (UserManager<ApplicationUser> UserManager, IReAuthenticationService ReAuthenticationService, ApplicationUser User) RegisterFakes(bool twoFactorEnabled = false)
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.GetTwoFactorEnabledAsync(user).Returns(Task.FromResult(twoFactorEnabled));

        Services.AddSingleton(userManager);
        var reAuthenticationService = this.RegisterReAuthFakes<StepUpReAuthModal>();

        return (userManager, reAuthenticationService, user);
    }

    [Fact]
    public void IsOpenFalse_RendersNothing()
    {
        RegisterFakes();

        var cut = Render<StepUpReAuthModal>(p => p
            .Add(x => x.IsOpen, false)
            .AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.ShouldNotShowStepUpModal();
    }

    [Fact]
    public void IsOpenTrue_WithoutTwoFactor_DoesNotRenderTotpField()
    {
        RegisterFakes(twoFactorEnabled: false);

        var cut = Render<StepUpReAuthModal>(p => p
            .Add(x => x.IsOpen, true)
            .AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.ShouldShowStepUpModal();
        cut.FindAll("#step-up-totp").Should().BeEmpty();
    }

    [Fact]
    public void IsOpenTrue_WithTwoFactorEnabled_RendersTotpField()
    {
        RegisterFakes(twoFactorEnabled: true);

        var cut = Render<StepUpReAuthModal>(p => p
            .Add(x => x.IsOpen, true)
            .AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("#step-up-totp").Should().NotBeNull();
    }

    [Fact]
    public void Submit_WrongPassword_ShowsErrorAndDoesNotMarkReAuthenticated()
    {
        var (userManager, reAuthenticationService, user) = RegisterFakes();
        userManager.CheckPasswordAsync(user, "forkert").Returns(Task.FromResult(false));

        var cut = Render<StepUpReAuthModal>(p => p
            .Add(x => x.IsOpen, true)
            .AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("#step-up-password").Change("forkert");
        cut.Find("form").Submit();

        cut.Markup.Should().Contain("Forkert adgangskode.");
        reAuthenticationService.DidNotReceive().MarkReAuthenticated();
    }

    [Fact]
    public void Submit_CorrectPasswordNoTwoFactor_MarksReAuthenticatedAndInvokesCallback()
    {
        var (userManager, reAuthenticationService, user) = RegisterFakes(twoFactorEnabled: false);
        userManager.CheckPasswordAsync(user, "Rigtig123!").Returns(Task.FromResult(true));
        var invoked = false;

        var cut = Render<StepUpReAuthModal>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.OnReAuthenticated, EventCallback.Factory.Create(this, () => invoked = true))
            .AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("#step-up-password").Change("Rigtig123!");
        cut.Find("form").Submit();

        reAuthenticationService.Received(1).MarkReAuthenticated();
        invoked.Should().BeTrue();
    }

    [Fact]
    public void Submit_CorrectPasswordMissingTotp_ShowsErrorAndDoesNotMarkReAuthenticated()
    {
        var (userManager, reAuthenticationService, user) = RegisterFakes(twoFactorEnabled: true);
        userManager.CheckPasswordAsync(user, "Rigtig123!").Returns(Task.FromResult(true));

        var cut = Render<StepUpReAuthModal>(p => p
            .Add(x => x.IsOpen, true)
            .AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("#step-up-password").Change("Rigtig123!");
        cut.Find("form").Submit();

        cut.Markup.Should().Contain("Indtast venligst din godkendelseskode.");
        reAuthenticationService.DidNotReceive().MarkReAuthenticated();
    }

    [Fact]
    public void Submit_CorrectPasswordAndTotp_MarksReAuthenticatedAndInvokesCallback()
    {
        var (userManager, reAuthenticationService, user) = RegisterFakes(twoFactorEnabled: true);
        userManager.CheckPasswordAsync(user, "Rigtig123!").Returns(Task.FromResult(true));
        userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, "123456")
            .Returns(Task.FromResult(true));
        var invoked = false;

        var cut = Render<StepUpReAuthModal>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.OnReAuthenticated, EventCallback.Factory.Create(this, () => invoked = true))
            .AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("#step-up-password").Change("Rigtig123!");
        cut.Find("#step-up-totp").Change("123456");
        cut.Find("form").Submit();

        reAuthenticationService.Received(1).MarkReAuthenticated();
        invoked.Should().BeTrue();
    }

    [Fact]
    public void Submit_CorrectPasswordWrongTotp_ShowsErrorAndDoesNotMarkReAuthenticated()
    {
        var (userManager, reAuthenticationService, user) = RegisterFakes(twoFactorEnabled: true);
        userManager.CheckPasswordAsync(user, "Rigtig123!").Returns(Task.FromResult(true));
        userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, "000000")
            .Returns(Task.FromResult(false));

        var cut = Render<StepUpReAuthModal>(p => p
            .Add(x => x.IsOpen, true)
            .AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("#step-up-password").Change("Rigtig123!");
        cut.Find("#step-up-totp").Change("000000");
        cut.Find("form").Submit();

        cut.Markup.Should().Contain("Ugyldig godkendelseskode.");
        reAuthenticationService.DidNotReceive().MarkReAuthenticated();
    }

    [Fact]
    public void ClickingAnnuller_InvokesOnCancel_AndDoesNotMarkReAuthenticated()
    {
        var (_, reAuthenticationService, _) = RegisterFakes();
        var cancelled = false;

        var cut = Render<StepUpReAuthModal>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.OnCancel, EventCallback.Factory.Create(this, () => cancelled = true))
            .AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("button.btn-secondary").Click();

        cancelled.Should().BeTrue();
        reAuthenticationService.DidNotReceive().MarkReAuthenticated();
    }

    [Fact]
    public async Task Submit_WrongPassword_RecordsReAuthFailure()
    {
        var (userManager, _, user) = RegisterFakes();
        userManager.CheckPasswordAsync(user, "forkert").Returns(Task.FromResult(false));
        var tracker = Services.GetRequiredService<IReAuthFailureTracker>();

        var cut = Render<StepUpReAuthModal>(p => p
            .Add(x => x.IsOpen, true)
            .AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        await cut.Find("#step-up-password").ChangeAsync("forkert");
        await cut.Find("form").SubmitAsync();

        await tracker.Received(1).RecordFailureAsync("user-1", Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Submit_CorrectPasswordNoTwoFactor_ClearsReAuthFailures()
    {
        var (userManager, _, user) = RegisterFakes(twoFactorEnabled: false);
        userManager.CheckPasswordAsync(user, "Rigtig123!").Returns(Task.FromResult(true));
        var tracker = Services.GetRequiredService<IReAuthFailureTracker>();

        var cut = Render<StepUpReAuthModal>(p => p
            .Add(x => x.IsOpen, true)
            .AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        await cut.Find("#step-up-password").ChangeAsync("Rigtig123!");
        await cut.Find("form").SubmitAsync();

        await tracker.Received(1).ClearFailuresAsync("user-1", Arg.Any<CancellationToken>());
    }
}