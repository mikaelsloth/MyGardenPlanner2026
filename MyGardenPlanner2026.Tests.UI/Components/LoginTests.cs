namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Components.Account.Pages;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using Xunit;

public class LoginTests : BunitContext
{
    private (UserManager<ApplicationUser> UserManager, SignInManager<ApplicationUser> SignInManager, IReAuthenticationService ReAuthenticationService) RegisterFakes()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        var signInManager = IdentityTestDoubles.CreateSignInManager(userManager);
        var reAuthenticationService = Substitute.For<IReAuthenticationService>();

        Services.AddSingleton(userManager);
        Services.AddSingleton(signInManager);
        Services.AddSingleton(reAuthenticationService);
        Services.AddSingleton(Substitute.For<ILogger<Login>>());

        return (userManager, signInManager, reAuthenticationService);
    }

    [Fact]
    public void LoginUser_ValidCredentials_RedirectsToReturnUrl()
    {
        var (_, signInManager, _) = RegisterFakes();
        signInManager.PasswordSignInAsync("test@example.dk", "Password123!", false, false)
            .Returns(Task.FromResult(SignInResult.Success));
        var navMan = this.UseIdentityRedirectManager();

        var httpContext = IdentityTestDoubles.CreateHttpContextWithAuthService();
        navMan.NavigateTo("/Account/Login?ReturnUrl=%2Fpricing");

        var cut = Render<Login>(parameters => parameters.AddCascadingValue(httpContext));
        cut.Find("#Input\\.Email").Change("test@example.dk");
        cut.Find("#Input\\.Password").Change("Password123!");
        cut.Find("form").Submit();

        navMan.Uri.Should().EndWith("/pricing");
    }

    [Fact]
    public void LoginUser_InvalidCredentials_ShowsErrorStatusMessage()
    {
        var (_, signInManager, _) = RegisterFakes();
        signInManager.PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>())
            .Returns(Task.FromResult(SignInResult.Failed));
        this.UseIdentityRedirectManager();

        var httpContext = IdentityTestDoubles.CreateHttpContextWithAuthService();
        var cut = Render<Login>(parameters => parameters.AddCascadingValue(httpContext));
        cut.Find("#Input\\.Email").Change("test@example.dk");
        cut.Find("#Input\\.Password").Change("forkert-adgangskode");
        cut.Find("form").Submit();

        cut.Markup.Should().Contain("Error: Invalid login attempt.");
    }

    [Fact]
    public void Login_RendersAuthPageShellWithDanishTitle()
    {
        RegisterFakes();
        this.UseIdentityRedirectManager();

        var httpContext = IdentityTestDoubles.CreateHttpContextWithAuthService();
        var cut = Render<Login>(parameters => parameters.AddCascadingValue(httpContext));

        cut.Find("h1").TextContent.Should().Be("Log ind");
    }

    [Fact]
    public void LoginUser_ValidCredentials_MarksReAuthenticated()
    {
        var (_, signInManager, reAuthenticationService) = RegisterFakes();
        signInManager.PasswordSignInAsync("test@example.dk", "Password123!", false, false)
            .Returns(Task.FromResult(SignInResult.Success));
        this.UseIdentityRedirectManager();

        var httpContext = IdentityTestDoubles.CreateHttpContextWithAuthService();
        var cut = Render<Login>(parameters => parameters.AddCascadingValue(httpContext));
        cut.Find("#Input\\.Email").Change("test@example.dk");
        cut.Find("#Input\\.Password").Change("Password123!");
        cut.Find("form").Submit();

        reAuthenticationService.Received(1).MarkReAuthenticated();
    }
}