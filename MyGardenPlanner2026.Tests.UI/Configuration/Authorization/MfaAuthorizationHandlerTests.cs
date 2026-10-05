namespace MyGardenPlanner2026.Tests.UI.Configuration.Authorization;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Configuration.Authorization;
using MyGardenPlanner2026.Core.Entities;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class MfaAuthorizationHandlerTests
{
    [Fact]
    public async Task HandleRequirementAsync_UserHasTwoFactorEnabled_Succeeds()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.GetTwoFactorEnabledAsync(user).Returns(Task.FromResult(true));

        var handler = new MfaAuthorizationHandler(userManager);
        var context = new AuthorizationHandlerContext([new MfaRequirement()], TestPrincipals.Create("user-1"), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserDoesNotHaveTwoFactorEnabled_DoesNotSucceed()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.GetTwoFactorEnabledAsync(user).Returns(Task.FromResult(false));

        var handler = new MfaAuthorizationHandler(userManager);
        var context = new AuthorizationHandlerContext([new MfaRequirement()], TestPrincipals.Create("user-1"), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_NoUserFound_DoesNotSucceed_AndSkipsTwoFactorLookup()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(null));

        var handler = new MfaAuthorizationHandler(userManager);
        var context = new AuthorizationHandlerContext([new MfaRequirement()], TestPrincipals.Create("user-1"), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        await userManager.DidNotReceive().GetTwoFactorEnabledAsync(Arg.Any<ApplicationUser>());
    }
}