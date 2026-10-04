namespace MyGardenPlanner2026.Tests.UI.Components.Routing;

using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components;
using Xunit;

public class RoutesAuthorizationBranchTests
{
    [Fact]
    public void IsAuthenticated_AuthenticatedUser_ReturnsTrue()
    {
        var state = new AuthenticationState(TestPrincipals.Create());

        Routes.IsAuthenticated(state).Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_UnauthenticatedUser_ReturnsFalse()
    {
        var state = new AuthenticationState(TestPrincipals.Anonymous());

        Routes.IsAuthenticated(state).Should().BeFalse();
    }
}