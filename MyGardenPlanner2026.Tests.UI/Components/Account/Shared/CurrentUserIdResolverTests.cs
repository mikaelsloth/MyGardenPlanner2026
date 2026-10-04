namespace MyGardenPlanner2026.Tests.UI.Components.Account.Shared;

using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
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
        var authStateTask = TestPrincipals.CreateAuthStateAsync("user-42");

        var result = await CurrentUserIdResolver.ResolveAsync(authStateTask);

        result.Should().Be("user-42");
    }

    [Fact]
    public async Task ResolveAsync_UnauthenticatedUser_ReturnsNull()
    {
        var authStateTask = Task.FromResult(new AuthenticationState(TestPrincipals.Anonymous()));

        var result = await CurrentUserIdResolver.ResolveAsync(authStateTask);

        result.Should().BeNull();
    }
}