namespace MyGardenPlanner2026.Tests.UI.Components;

using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components;
using System.Security.Claims;
using Xunit;

public class RoutesAuthorizationBranchTests
{
    [Fact]
    public void IsAuthenticated_AuthenticatedUser_ReturnsTrue()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")], authenticationType: "Test");
        var state = new AuthenticationState(new ClaimsPrincipal(identity));

        Routes.IsAuthenticated(state).Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_UnauthenticatedUser_ReturnsFalse()
    {
        var state = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        Routes.IsAuthenticated(state).Should().BeFalse();
    }
}