namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Account.Pages;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class AccessDeniedTests : BunitContext
{
    [Fact]
    public void AccessDenied_UserHasTwoFactorEnabled_ShowsGenericAccessDeniedMessage_WithoutRedirecting()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.GetTwoFactorEnabledAsync(user).Returns(Task.FromResult(true));
        Services.AddSingleton(userManager);
        var navMan = this.UseIdentityRedirectManager();

        var cut = Render<AccessDenied>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));

        cut.Find("h1").TextContent.Should().Be("Adgang nægtet");
        cut.Markup.Should().Contain("ikke adgang til denne ressource");
        navMan.Uri.Should().NotContain("EnableAuthenticator");
    }

    [Fact]
    public void AccessDenied_UserDoesNotHaveTwoFactorEnabled_RedirectsToEnableAuthenticator()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.GetTwoFactorEnabledAsync(user).Returns(Task.FromResult(false));
        Services.AddSingleton(userManager);
        var navMan = this.UseIdentityRedirectManager();

        Render<AccessDenied>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));

        navMan.Uri.Should().Contain("Account/Manage/EnableAuthenticator");
    }

    [Fact]
    public void AccessDenied_NoUserFound_ShowsGenericAccessDeniedMessage_WithoutRedirecting()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(null));
        Services.AddSingleton(userManager);
        var navMan = this.UseIdentityRedirectManager();

        var cut = Render<AccessDenied>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));

        cut.Markup.Should().Contain("ikke adgang til denne ressource");
        navMan.Uri.Should().NotContain("EnableAuthenticator");
    }
}