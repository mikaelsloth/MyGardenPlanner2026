namespace MyGardenPlanner2026.Tests.UI.Components.Account.Shared;

using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using System.Security.Claims;
using Xunit;

public sealed class CurrentUserEmailResolverTests
{
    private static Task<AuthenticationState> StateAsync(bool authenticated, params Claim[] claims)
    {
        var identity = authenticated ? new ClaimsIdentity(claims, "test") : new ClaimsIdentity(claims);
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    [Fact]
    public async Task ResolveAsync_NullTask_ReturnsNull() => (await CurrentUserEmailResolver.ResolveAsync(null)).Should().BeNull();

    [Fact]
    public async Task ResolveAsync_AuthenticatedWithEmailClaim_ReturnsEmail()
    {
        var result = await CurrentUserEmailResolver.ResolveAsync(
            StateAsync(true, new Claim(ClaimTypes.Email, "bruger@example.dk")));

        result.Should().Be("bruger@example.dk");
    }

    [Fact]
    public async Task ResolveAsync_AuthenticatedWithoutEmailClaim_ReturnsNull()
    {
        var result = await CurrentUserEmailResolver.ResolveAsync(
            StateAsync(true, new Claim(ClaimTypes.NameIdentifier, "user-1")));

        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveAsync_NotAuthenticated_ReturnsNullEvenWithEmailClaim()
    {
        var result = await CurrentUserEmailResolver.ResolveAsync(
            StateAsync(false, new Claim(ClaimTypes.Email, "bruger@example.dk")));

        result.Should().BeNull();
    }
}