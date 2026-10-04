namespace MyGardenPlanner2026.Tests.UI.Internal_Tests;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Xunit;

public sealed class BunitAuthorizationExtensionsTests : BunitContext
{
    private async Task<AuthenticationState> GetStateAsync() =>
        await Services.GetRequiredService<AuthenticationStateProvider>().GetAuthenticationStateAsync();

    [Fact]
    public async Task AuthorizeAs_UserIdOnly_SetsAuthenticatedUserWithNameAndNameIdentifier()
    {
        this.AuthorizeAs("user-1");

        var state = await GetStateAsync();

        state.User.Identity!.IsAuthenticated.Should().BeTrue();
        state.User.Identity.Name.Should().Be("user-1");
        state.User.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("user-1");
    }

    [Fact]
    public async Task AuthorizeAs_WithUserName_UsesUserNameAsNameAndUserIdAsNameIdentifier()
    {
        this.AuthorizeAs("user-1", "user1@example.com");

        var state = await GetStateAsync();

        state.User.Identity!.Name.Should().Be("user1@example.com");
        state.User.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("user-1");
    }
}