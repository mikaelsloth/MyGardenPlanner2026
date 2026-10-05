namespace MyGardenPlanner2026.Tests.UI.Components.Account.Pages;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Account.Pages;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using Xunit;

public class LoginWithRecoveryCodeTests : BunitContext
{
    private (UserManager<ApplicationUser> UserManager, SignInManager<ApplicationUser> SignInManager, IReAuthenticationService ReAuthenticationService) RegisterFakes()
    {
        var (userManager, signInManager) = this.RegisterIdentityFakes();
        IdentityTestDoubles.SetupTwoFactorLoginUser(userManager, signInManager);
        var reAuthenticationService = this.RegisterReAuthFakes<LoginWithRecoveryCode>();

        return (userManager, signInManager, reAuthenticationService);
    }

    [Fact]
    public void OnValidSubmitAsync_ValidRecoveryCode_RedirectsToReturnUrl()
    {
        var (_, signInManager, _) = RegisterFakes();
        signInManager.TwoFactorRecoveryCodeSignInAsync("ABCD1234").Returns(Task.FromResult(SignInResult.Success));
        var navMan = this.UseIdentityRedirectManager();

        navMan.NavigateTo("/Account/LoginWithRecoveryCode?ReturnUrl=%2Fpricing");
        var cut = Render<LoginWithRecoveryCode>(parameters => parameters.AddCascadingValue(new DefaultHttpContext())); ;
        cut.Find("#Input\\.RecoveryCode").Change("ABCD1234");
        cut.Find("form").Submit();

        navMan.Uri.Should().EndWith("/pricing");
    }

    [Fact]
    public void OnValidSubmitAsync_InvalidRecoveryCode_ShowsDanishErrorMessage()
    {
        var (_, signInManager, _) = RegisterFakes();
        signInManager.TwoFactorRecoveryCodeSignInAsync(Arg.Any<string>()).Returns(Task.FromResult(SignInResult.Failed));
        this.UseIdentityRedirectManager();

        var cut = Render<LoginWithRecoveryCode>(parameters => parameters.AddCascadingValue(new DefaultHttpContext())); ;
        cut.Find("#Input\\.RecoveryCode").Change("FORKERT");
        cut.Find("form").Submit();

        cut.Markup.Should().Contain("Error: Ugyldig gendannelseskode indtastet.");
    }

    [Fact]
    public async Task OnValidSubmitAsync_InvalidRecoveryCode_RecordsReAuthFailure()
    {
        var (_, signInManager, _) = RegisterFakes();
        signInManager.TwoFactorRecoveryCodeSignInAsync(Arg.Any<string>()).Returns(Task.FromResult(SignInResult.Failed));
        this.UseIdentityRedirectManager();
        var tracker = Services.GetRequiredService<IReAuthFailureTracker>();

        var cut = Render<LoginWithRecoveryCode>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));
        await cut.Find("#Input\\.RecoveryCode").ChangeAsync("FORKERT");
        await cut.Find("form").SubmitAsync();

        await tracker.Received(1).RecordFailureAsync("user-1", Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void OnValidSubmitAsync_ValidRecoveryCode_MarksReAuthenticated()
    {
        var (_, signInManager, reAuthenticationService) = RegisterFakes();
        signInManager.TwoFactorRecoveryCodeSignInAsync("ABCD1234").Returns(Task.FromResult(SignInResult.Success));
        this.UseIdentityRedirectManager();

        var cut = Render<LoginWithRecoveryCode>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));
        cut.Find("#Input\\.RecoveryCode").Change("ABCD1234");
        cut.Find("form").Submit();

        reAuthenticationService.Received(1).MarkReAuthenticated();
    }
}