namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Components.Account.Pages;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using Xunit;

public class LoginWithRecoveryCodeTests : BunitContext
{
    private (UserManager<ApplicationUser> UserManager, SignInManager<ApplicationUser> SignInManager) RegisterFakes()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserIdAsync(user).Returns(Task.FromResult("user-1"));

        var signInManager = IdentityTestDoubles.CreateSignInManager(userManager);
        signInManager.GetTwoFactorAuthenticationUserAsync().Returns(Task.FromResult<ApplicationUser?>(user));

        Services.AddSingleton(userManager);
        Services.AddSingleton(signInManager);
        Services.AddSingleton(Substitute.For<ILogger<LoginWithRecoveryCode>>());

        return (userManager, signInManager);
    }

    [Fact]
    public void OnValidSubmitAsync_ValidRecoveryCode_RedirectsToReturnUrl()
    {
        var (_, signInManager) = RegisterFakes();
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
        var (_, signInManager) = RegisterFakes();
        signInManager.TwoFactorRecoveryCodeSignInAsync(Arg.Any<string>()).Returns(Task.FromResult(SignInResult.Failed));
        this.UseIdentityRedirectManager();

        var cut = Render<LoginWithRecoveryCode>(parameters => parameters.AddCascadingValue(new DefaultHttpContext())); ;
        cut.Find("#Input\\.RecoveryCode").Change("FORKERT");
        cut.Find("form").Submit();

        cut.Markup.Should().Contain("Error: Ugyldig gendannelseskode indtastet.");
    }
}