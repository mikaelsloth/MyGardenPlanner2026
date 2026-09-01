namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Components.Account.Pages;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using Xunit;

public class LoginWith2faTests : BunitContext
{
    private (UserManager<ApplicationUser> UserManager, SignInManager<ApplicationUser> SignInManager, ApplicationUser User, IReAuthenticationService ReAuthenticationService) RegisterFakes()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserIdAsync(user).Returns(Task.FromResult("user-1"));

        var signInManager = IdentityTestDoubles.CreateSignInManager(userManager);
        signInManager.GetTwoFactorAuthenticationUserAsync().Returns(Task.FromResult<ApplicationUser?>(user));

        var reAuthenticationService = Substitute.For<IReAuthenticationService>();

        Services.AddSingleton(userManager);
        Services.AddSingleton(signInManager);
        Services.AddSingleton(reAuthenticationService);
        Services.AddSingleton(Substitute.For<ILogger<LoginWith2fa>>());
        Services.AddSingleton(Substitute.For<IReAuthFailureTracker>());

        var currentUserAccessor = Substitute.For<ICurrentUserAccessor>();
        currentUserAccessor.GetCurrent().Returns(new CurrentUserInfo(null, null, "127.0.0.1"));
        Services.AddSingleton(currentUserAccessor);

        return (userManager, signInManager, user, reAuthenticationService);
    }

    [Fact]
    public void OnValidSubmitAsync_ValidCode_RedirectsToReturnUrl()
    {
        var (_, signInManager, _, _) = RegisterFakes();
        signInManager.TwoFactorAuthenticatorSignInAsync("123456", false, false)
            .Returns(Task.FromResult(SignInResult.Success));
        var navMan = this.UseIdentityRedirectManager();

        navMan.NavigateTo("/Account/LoginWith2fa?ReturnUrl=%2Fpricing&RememberMe=false");
        var cut = Render<LoginWith2fa>(parameters => parameters.AddCascadingValue(new DefaultHttpContext())); ;
        cut.Find("#Input\\.TwoFactorCode").Change("123456");
        cut.Find("form").Submit();

        navMan.Uri.Should().EndWith("/pricing");
    }

    [Fact]
    public void OnValidSubmitAsync_InvalidCode_ShowsDanishErrorMessage()
    {
        var (_, signInManager, _, _) = RegisterFakes();
        signInManager.TwoFactorAuthenticatorSignInAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>())
            .Returns(Task.FromResult(SignInResult.Failed));
        this.UseIdentityRedirectManager();

        var cut = Render<LoginWith2fa>(parameters => parameters.AddCascadingValue(new DefaultHttpContext())); ;
        cut.Find("#Input\\.TwoFactorCode").Change("000000");
        cut.Find("form").Submit();

        cut.Markup.Should().Contain("Error: Ugyldig godkendelseskode.");
    }

    [Fact]
    public void OnValidSubmitAsync_ValidCode_MarksReAuthenticated()
    {
        var (_, signInManager, _, reAuthenticationService) = RegisterFakes();
        signInManager.TwoFactorAuthenticatorSignInAsync("123456", false, false)
            .Returns(Task.FromResult(SignInResult.Success));
        this.UseIdentityRedirectManager();

        var cut = Render<LoginWith2fa>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));
        cut.Find("#Input\\.TwoFactorCode").Change("123456");
        cut.Find("form").Submit();

        reAuthenticationService.Received(1).MarkReAuthenticated();
    }

    [Fact]
    public async Task OnValidSubmitAsync_ValidCode_ClearsReAuthFailures()
    {
        var (_, signInManager, _, _) = RegisterFakes();
        signInManager.TwoFactorAuthenticatorSignInAsync("123456", false, false)
            .Returns(Task.FromResult(SignInResult.Success));
        this.UseIdentityRedirectManager();
        var tracker = Services.GetRequiredService<IReAuthFailureTracker>();

        var cut = Render<LoginWith2fa>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));
        await cut.Find("#Input\\.TwoFactorCode").ChangeAsync("123456");
        await cut.Find("form").SubmitAsync();

        await tracker.Received(1).ClearFailuresAsync("user-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OnValidSubmitAsync_InvalidCode_RecordsReAuthFailure()
    {
        var (_, signInManager, _, _) = RegisterFakes();
        signInManager.TwoFactorAuthenticatorSignInAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>())
            .Returns(Task.FromResult(SignInResult.Failed));
        this.UseIdentityRedirectManager();
        var tracker = Services.GetRequiredService<IReAuthFailureTracker>();

        var cut = Render<LoginWith2fa>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));
        await cut.Find("#Input\\.TwoFactorCode").ChangeAsync("000000");
        await cut.Find("form").SubmitAsync();

        await tracker.Received(1).RecordFailureAsync("user-1", Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }
}