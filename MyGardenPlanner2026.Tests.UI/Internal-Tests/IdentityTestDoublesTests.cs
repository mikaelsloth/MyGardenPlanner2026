namespace MyGardenPlanner2026.Tests.UI.InternalTests;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Account;
using MyGardenPlanner2026.Tests.UI;
using Xunit;

public sealed class IdentityTestDoublesTests : BunitContext
{
    [Fact]
    public void CreateUserManager_ReturnsNonNullSubstitute()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.Should().NotBeNull();
    }

    [Fact]
    public void CreateSignInManager_ReturnsNonNullSubstitute()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        var signInManager = IdentityTestDoubles.CreateSignInManager(userManager);
        signInManager.Should().NotBeNull();
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    public void CreateHttpContextWithAuthService_ConfiguresAuthServiceAndMethod(string method)
    {
        var httpContext = IdentityTestDoubles.CreateHttpContextWithAuthService(method);
        httpContext.Request.Method.Should().Be(method);
        httpContext.RequestServices.GetService<IAuthenticationService>().Should().NotBeNull();
    }

    [Fact]
    public void UseIdentityRedirectManager_RegistersManagerAndReturnsNavigationManager()
    {
        var navMan = this.UseIdentityRedirectManager();
        navMan.Should().NotBeNull();
        Services.GetService<IdentityRedirectManager>().Should().NotBeNull();
    }
}