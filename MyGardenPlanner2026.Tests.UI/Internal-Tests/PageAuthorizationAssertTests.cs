namespace MyGardenPlanner2026.Tests.UI.Internal_Tests;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Tests.UI.Configuration.Authorization.PageAccess;
using Xunit;

public sealed class PageAuthorizationAssertTests
{
    [Authorize(Policy = "PolicyA")]
    public sealed class SecuredPage
    {
    }

    public sealed class OpenPage
    {
    }

    [Fact]
    public void RequiresPolicy_PageHasExpectedPolicy_Passes()
    {
        PageAuthorizationAssert.RequiresPolicy<SecuredPage>("PolicyA");
    }

    [Fact]
    public void RequiresPolicy_PageHasOtherPolicy_Fails()
    {
        var act = () => PageAuthorizationAssert.RequiresPolicy<SecuredPage>("PolicyB");

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void RequiresPolicy_PageHasNoAuthorizeAttribute_Fails()
    {
        var act = () => PageAuthorizationAssert.RequiresPolicy<OpenPage>("PolicyA");

        act.Should().Throw<Exception>();
    }
}