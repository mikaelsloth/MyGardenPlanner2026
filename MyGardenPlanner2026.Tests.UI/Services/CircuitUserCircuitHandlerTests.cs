namespace MyGardenPlanner2026.Tests.UI.Services;

using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Services;
using NSubstitute;
using System.Net;
using Xunit;

public class CircuitUserCircuitHandlerTests
{
    private const string ClientIp = "203.0.113.7";
    private const string ReconnectIp = "198.51.100.9";

    private static (CircuitUserCircuitHandler Handler, HttpContextAccessor Accessor) CreateHandler()
    {
        var authenticationStateProvider = Substitute.For<AuthenticationStateProvider>();
        authenticationStateProvider.GetAuthenticationStateAsync()
            .Returns(Task.FromResult(new AuthenticationState(TestPrincipals.Create())));

        var accessor = new HttpContextAccessor();
        return (new CircuitUserCircuitHandler(authenticationStateProvider, accessor), accessor);
    }

    private static DefaultHttpContext HttpContextFrom(string ipAddress)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(ipAddress);
        return httpContext;
    }

    private static async Task<CurrentUserInfo?> RunActivityAsync(CircuitUserCircuitHandler handler)
    {
        CurrentUserInfo? seen = null;
        var activity = handler.CreateInboundActivityHandler(_ =>
        {
            seen = CircuitUserContext.Current;
            return Task.CompletedTask;
        });

        await activity(null!);

        return seen;
    }

    [Fact]
    public async Task InboundActivity_IpCapturedWhenCircuitOpened_SurvivesHttpContextBeingGone()
    {
        var (handler, accessor) = CreateHandler();
        accessor.HttpContext = HttpContextFrom(ClientIp);
        await handler.OnCircuitOpenedAsync(null!, TestContext.Current.CancellationToken);
        accessor.HttpContext = null;

        var seen = await RunActivityAsync(handler);

        seen.Should().Be(new CurrentUserInfo(TestPrincipals.DefaultUserId, null, ClientIp));
    }

    [Fact]
    public async Task InboundActivity_NoHttpContextEver_IpIsNull()
    {
        var (handler, _) = CreateHandler();
        await handler.OnCircuitOpenedAsync(null!, TestContext.Current.CancellationToken);

        var seen = await RunActivityAsync(handler);

        seen.Should().Be(new CurrentUserInfo(TestPrincipals.DefaultUserId, null, null));
    }

    [Fact]
    public async Task InboundActivity_ConnectionUpWithNewIp_UsesLatestIp()
    {
        var (handler, accessor) = CreateHandler();
        accessor.HttpContext = HttpContextFrom(ClientIp);
        await handler.OnCircuitOpenedAsync(null!, TestContext.Current.CancellationToken);
        accessor.HttpContext = HttpContextFrom(ReconnectIp);
        await handler.OnConnectionUpAsync(null!, TestContext.Current.CancellationToken);
        accessor.HttpContext = null;

        var seen = await RunActivityAsync(handler);

        seen.Should().Be(new CurrentUserInfo(TestPrincipals.DefaultUserId, null, ReconnectIp));
    }

    [Fact]
    public async Task InboundActivity_HttpContextAvailableOnlyDuringActivity_CapturesIp()
    {
        var (handler, accessor) = CreateHandler();
        accessor.HttpContext = HttpContextFrom(ClientIp);

        var seen = await RunActivityAsync(handler);

        seen.Should().Be(new CurrentUserInfo(TestPrincipals.DefaultUserId, null, ClientIp));
    }
}