namespace MyGardenPlanner2026.Tests.UI.Services;

using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Services;
using System.Security.Claims;
using Xunit;

public sealed class CircuitUserTests
{
    private sealed class FakeAuthenticationStateProvider(Func<Task<AuthenticationState>> state)
        : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => state();
    }

    private static ClaimsPrincipal Authenticated(string id, string email) =>
        new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, id), new Claim(ClaimTypes.Email, email)], "test"));

    private static AuthenticationState StateOf(ClaimsPrincipal user) => new(user);

    // --- CircuitUserContext.From ---

    [Fact]
    public void From_AutentificeretBruger_GiverIdOgEmailUdenIp()
    {
        var info = CircuitUserContext.From(Authenticated("u1", "u1@test.dk"));

        info.Should().Be(new CurrentUserInfo("u1", "u1@test.dk", null));
    }

    [Fact]
    public void From_AnonymBrugerEllerNull_GiverNull()
    {
        CircuitUserContext.From(new ClaimsPrincipal(new ClaimsIdentity())).Should().BeNull();
        CircuitUserContext.From(null).Should().BeNull();
    }

    [Fact]
    public void From_UdenEmailClaim_BrugerIdentityName()
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "u2"), new Claim(ClaimTypes.Name, "navn@test.dk")],
            "test",
            ClaimTypes.Name,
            ClaimTypes.Role);

        CircuitUserContext.From(new ClaimsPrincipal(identity))
            .Should().Be(new CurrentUserInfo("u2", "navn@test.dk", null));
    }

    // --- CircuitUserCircuitHandler ---

    [Fact]
    public async Task Handler_UnderAktiviteten_ErBrugerenSat_OgNulstillesBagefter()
    {
        var provider = new FakeAuthenticationStateProvider(
            () => Task.FromResult(StateOf(Authenticated("u1", "u1@test.dk"))));
        var handler = new CircuitUserCircuitHandler(provider);
        CurrentUserInfo? seenDuring = null;

        await handler.CreateInboundActivityHandler(_ =>
        {
            seenDuring = CircuitUserContext.Current;
            return Task.CompletedTask;
        })(null!);

        seenDuring.Should().Be(new CurrentUserInfo("u1", "u1@test.dk", null));
        CircuitUserContext.Current.Should().BeNull();
    }

    [Fact]
    public async Task Handler_KasterAktiviteten_NulstillesAlligevel()
    {
        var provider = new FakeAuthenticationStateProvider(
            () => Task.FromResult(StateOf(Authenticated("u1", "u1@test.dk"))));
        var handler = new CircuitUserCircuitHandler(provider);

        var act = () => handler.CreateInboundActivityHandler(
            _ => throw new InvalidOperationException("fejl"))(null!);

        await act.Should().ThrowAsync<InvalidOperationException>();
        CircuitUserContext.Current.Should().BeNull();
    }

    [Fact]
    public async Task Handler_AuthenticationStateIkkeSat_KalderNextUdenBruger()
    {
        var provider = new FakeAuthenticationStateProvider(
            () => throw new InvalidOperationException("ikke sat"));
        var handler = new CircuitUserCircuitHandler(provider);
        var called = false;
        CurrentUserInfo? seenDuring = new("x", "x", null);

        await handler.CreateInboundActivityHandler(_ =>
        {
            called = true;
            seenDuring = CircuitUserContext.Current;
            return Task.CompletedTask;
        })(null!);

        called.Should().BeTrue();
        seenDuring.Should().BeNull();
    }

    // --- HttpContextCurrentUserAccessor ---

    [Fact]
    public void Accessor_UdenHttpContext_BrugerCircuitBrugeren()
    {
        var accessor = new HttpContextCurrentUserAccessor(new HttpContextAccessor { HttpContext = null });

        try
        {
            CircuitUserContext.Current = new CurrentUserInfo("c1", "c1@test.dk", null);

            accessor.GetCurrent().Should().Be(new CurrentUserInfo("c1", "c1@test.dk", null));
        }
        finally
        {
            CircuitUserContext.Current = null;
        }
    }

    [Fact]
    public void Accessor_UdenHttpContextOgUdenCircuitBruger_GiverTomBruger()
    {
        var accessor = new HttpContextCurrentUserAccessor(new HttpContextAccessor { HttpContext = null });

        accessor.GetCurrent().Should().Be(new CurrentUserInfo(null, null, null));
    }

    [Fact]
    public void Accessor_HttpContextMedBruger_VinderOverCircuitBrugeren()
    {
        var httpContext = new DefaultHttpContext { User = Authenticated("h1", "h1@test.dk") };
        var accessor = new HttpContextCurrentUserAccessor(new HttpContextAccessor { HttpContext = httpContext });

        try
        {
            CircuitUserContext.Current = new CurrentUserInfo("c1", "c1@test.dk", null);

            var info = accessor.GetCurrent();

            info.UserId.Should().Be("h1");
            info.UserEmail.Should().Be("h1@test.dk");
        }
        finally
        {
            CircuitUserContext.Current = null;
        }
    }

    [Fact]
    public void Accessor_HttpContextUdenBruger_FaldTilbageTilCircuitBrugerenMedIpFraHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.1.2.3");
        var accessor = new HttpContextCurrentUserAccessor(new HttpContextAccessor { HttpContext = httpContext });

        try
        {
            CircuitUserContext.Current = new CurrentUserInfo("c1", "c1@test.dk", null);

            accessor.GetCurrent().Should().Be(new CurrentUserInfo("c1", "c1@test.dk", "10.1.2.3"));
        }
        finally
        {
            CircuitUserContext.Current = null;
        }
    }
}