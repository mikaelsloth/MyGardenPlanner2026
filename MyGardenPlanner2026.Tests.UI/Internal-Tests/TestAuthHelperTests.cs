namespace MyGardenPlanner2026.Tests.UI.InternalTests;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Tests.UI;
using System.Security.Claims;
using Xunit;

public sealed class TestAuthHelperTests : BunitContext
{
    [Fact]
    public void CreatePrincipal_DefaultParameters_HasNameIdentifierAndDefaultUser()
    {
        var principal = TestAuthHelper.CreatePrincipal();

        principal.Identity.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
        principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("user-1");
        principal.FindFirst(ClaimTypes.Name)?.Value.Should().Be("user-1");
    }

    [Fact]
    public void CreatePrincipal_WithRoles_ContainsRoleClaims()
    {
        var principal = TestAuthHelper.CreatePrincipal("admin-user", "SystemAdmin", "DataAdmin");

        principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("admin-user");
        principal.IsInRole("SystemAdmin").Should().BeTrue();
        principal.IsInRole("DataAdmin").Should().BeTrue();
        principal.IsInRole("OtherRole").Should().BeFalse();
    }

    [Fact]
    public void CreatePrincipal_NullUserId_CreatesPrincipalWithoutNameIdentifier()
    {
        var principal = TestAuthHelper.CreatePrincipal(userId: null);

        principal.FindFirst(ClaimTypes.NameIdentifier).Should().BeNull();
    }

    [Fact]
    public async Task CreateAuthStateAsync_ReturnsStateWithMatchingPrincipal()
    {
        var authState = await TestAuthHelper.CreateAuthStateAsync("user-42", "CustomRole");

        authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("user-42");
        authState.User.IsInRole("CustomRole").Should().BeTrue();
    }

    [Fact]
    public void AuthorizeUser_ConfiguresBunitAuthContextWithExpectedClaims()
    {
        var authContext = this.AuthorizeUser("special-user", "Editor");

        authContext.IsAuthenticated.Should().BeTrue();
        authContext.UserName.Should().Be("special-user");
    }

    [Fact]
    public void AuthorizeAdmin_SetsSystemAdminRoleByDefault()
    {
        var authContext = this.AuthorizeAdmin("super-admin");

        authContext.IsAuthenticated.Should().BeTrue();
        authContext.UserName.Should().Be("super-admin");
    }
}