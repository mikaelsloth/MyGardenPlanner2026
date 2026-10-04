namespace MyGardenPlanner2026.Tests.UI.Internal_Tests;

using FluentAssertions;
using System.Security.Claims;
using Xunit;

public sealed class TestPrincipalsTests
{
    [Fact]
    public void Create_Default_IsAuthenticatedWithDefaultUserIdAsNameIdentifier()
    {
        var principal = TestPrincipals.Create();

        principal.Identity!.IsAuthenticated.Should().BeTrue();
        principal.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be(TestPrincipals.DefaultUserId);
    }

    [Fact]
    public void Create_WithRoles_AddsRoleClaims()
    {
        var principal = TestPrincipals.Create("user-7", "SystemAdmin", "DataAdmin");

        principal.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("user-7");
        principal.IsInRole("SystemAdmin").Should().BeTrue();
        principal.IsInRole("DataAdmin").Should().BeTrue();
        principal.IsInRole("PolicyAdmin").Should().BeFalse();
    }

    [Fact]
    public void Create_NullUserId_IsAuthenticatedButHasNoNameIdentifierClaim()
    {
        var principal = TestPrincipals.Create(userId: null);

        principal.Identity!.IsAuthenticated.Should().BeTrue();
        principal.FindFirst(ClaimTypes.NameIdentifier).Should().BeNull();
    }

    [Fact]
    public void Anonymous_IsNotAuthenticated()
    {
        TestPrincipals.Anonymous().Identity!.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAuthStateAsync_ReturnsStateForRequestedUser()
    {
        var state = await TestPrincipals.CreateAuthStateAsync("approver-1", "SystemAdmin");

        state.User.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("approver-1");
        state.User.IsInRole("SystemAdmin").Should().BeTrue();
    }
}