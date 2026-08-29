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
    public void AccessDenied_UserHasTwoFactorEnabled_ShowsGenericAccessDeniedMessage()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.GetTwoFactorEnabledAsync(user).Returns(Task.FromResult(true));
        Services.AddSingleton(userManager);

        var cut = Render<AccessDenied>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));

        cut.Find("h1").TextContent.Should().Be("Adgang nægtet");
        cut.Markup.Should().Contain("ikke adgang til denne ressource");
        cut.Markup.Should().NotContain("Opsæt totrinsbekræftelse");
    }

    [Fact]
    public void AccessDenied_UserDoesNotHaveTwoFactorEnabled_ShowsTwoFactorSetupLink()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.GetTwoFactorEnabledAsync(user).Returns(Task.FromResult(false));
        Services.AddSingleton(userManager);

        var cut = Render<AccessDenied>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));

        cut.Markup.Should().Contain("totrinsbekræftelse");
        cut.Find("a[href='Account/Manage/EnableAuthenticator']").Should().NotBeNull();
    }

    [Fact]
    public void AccessDenied_NoUserFound_ShowsGenericAccessDeniedMessage()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(null));
        Services.AddSingleton(userManager);

        var cut = Render<AccessDenied>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));

        cut.Markup.Should().Contain("ikke adgang til denne ressource");
    }
}