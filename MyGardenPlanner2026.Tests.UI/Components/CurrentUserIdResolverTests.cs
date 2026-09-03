namespace MyGardenPlanner2026.Tests.UI.Components;

using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using System.Security.Claims;
using Xunit;

public class CurrentUserIdResolverTests
{
    [Fact]
    public async Task ResolveAsync_NullAuthenticationStateTask_ReturnsNull()
    {
        var result = await CurrentUserIdResolver.ResolveAsync(null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveAsync_AuthenticatedUser_ReturnsNameIdentifierClaim()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-42")], authenticationType: "Test");
        var authStateTask = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));

        var result = await CurrentUserIdResolver.ResolveAsync(authStateTask);

        result.Should().Be("user-42");
    }

    [Fact]
    public async Task ResolveAsync_UnauthenticatedUser_ReturnsNull()
    {
        var authStateTask = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        var result = await CurrentUserIdResolver.ResolveAsync(authStateTask);

        result.Should().BeNull();
    }
}