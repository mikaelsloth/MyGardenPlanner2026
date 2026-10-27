namespace MyGardenPlanner2026.Tests.UI.Services;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Services;
using System.Net;
using Xunit;

public class HttpContextCurrentUserAccessorTests
{
    private const string CircuitIp = "203.0.113.7";
    private const string HttpIp = "198.51.100.1";

    private static DefaultHttpContext HttpContextFor(System.Security.Claims.ClaimsPrincipal user)
    {
        var httpContext = new DefaultHttpContext { User = user };
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(HttpIp);
        return httpContext;
    }

    [Fact]
    public void GetCurrent_NoHttpContext_UsesCircuitUserIncludingClientIp()
    {
        var accessor = new HttpContextCurrentUserAccessor(new HttpContextAccessor());
        CircuitUserContext.Current = new CurrentUserInfo("circuit-user", "c@test.dk", CircuitIp);

        try
        {
            accessor.GetCurrent().Should().Be(new CurrentUserInfo("circuit-user", "c@test.dk", CircuitIp));
        }
        finally
        {
            CircuitUserContext.Current = null;
        }
    }

    [Fact]
    public void GetCurrent_NoHttpContextAndNoCircuitUser_ReturnsEmpty()
    {
        var accessor = new HttpContextCurrentUserAccessor(new HttpContextAccessor());

        accessor.GetCurrent().Should().Be(new CurrentUserInfo(null, null, null));
    }

    [Fact]
    public void GetCurrent_AuthenticatedHttpContext_UsesHttpContextUserAndIp()
    {
        var accessor = new HttpContextCurrentUserAccessor(
            new HttpContextAccessor { HttpContext = HttpContextFor(TestPrincipals.Create("http-user")) });

        accessor.GetCurrent().Should().Be(new CurrentUserInfo("http-user", null, HttpIp));
    }

    [Fact]
    public void GetCurrent_AnonymousHttpContextWithCircuitUser_PrefersHttpContextIp()
    {
        var accessor = new HttpContextCurrentUserAccessor(
            new HttpContextAccessor { HttpContext = HttpContextFor(TestPrincipals.Anonymous()) });
        CircuitUserContext.Current = new CurrentUserInfo("circuit-user", null, CircuitIp);

        try
        {
            accessor.GetCurrent().Should().Be(new CurrentUserInfo("circuit-user", null, HttpIp));
        }
        finally
        {
            CircuitUserContext.Current = null;
        }
    }
}