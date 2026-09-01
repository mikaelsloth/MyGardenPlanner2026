namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Components.Account.Pages;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using Xunit;

public class ExternalLoginTests : BunitContext
{
    private (Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> UserManager, Microsoft.AspNetCore.Identity.SignInManager<ApplicationUser> SignInManager) RegisterFakes()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        var signInManager = IdentityTestDoubles.CreateSignInManager(userManager);

        Services.AddSingleton(userManager);
        Services.AddSingleton(signInManager);
        Services.AddSingleton(Substitute.For<Microsoft.AspNetCore.Identity.IUserStore<ApplicationUser>>());
        Services.AddSingleton(Substitute.For<Microsoft.AspNetCore.Identity.IEmailSender<ApplicationUser>>());
        Services.AddSingleton(Substitute.For<ILogger<ExternalLogin>>());

        return (userManager, signInManager);
    }

    [Fact]
    public void ExternalLogin_RemoteErrorPresent_RedirectsToLoginWithoutCallingSignInManager()
    {
        var (_, signInManager) = RegisterFakes();
        var navMan = this.UseIdentityRedirectManager();

        navMan.NavigateTo("/Account/ExternalLogin?RemoteError=access_denied");
        Render<ExternalLogin>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));

        navMan.Uri.Should().Contain("Account/Login");
        _ = signInManager.DidNotReceive().GetExternalLoginInfoAsync();
    }

    [Fact]
    public void ExternalLogin_NoExternalLoginInfo_RedirectsToLoginWithError()
    {
        var (_, signInManager) = RegisterFakes();
        signInManager.GetExternalLoginInfoAsync().Returns(Task.FromResult<Microsoft.AspNetCore.Identity.ExternalLoginInfo?>(null));
        var navMan = this.UseIdentityRedirectManager();

        Render<ExternalLogin>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));

        navMan.Uri.Should().Contain("Account/Login");
    }
}