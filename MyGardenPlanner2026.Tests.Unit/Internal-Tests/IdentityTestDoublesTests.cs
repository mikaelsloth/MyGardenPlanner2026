namespace MyGardenPlanner2026.Tests.Unit.Internal_Tests;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities;
using NSubstitute;
using Xunit;

public sealed class IdentityTestDoublesTests
{
    [Fact]
    public async Task CreateUserManager_CanBeStubbedAfterCreation()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.FindByIdAsync("user-1").Returns(Task.FromResult<ApplicationUser?>(user));

        var result = await userManager.FindByIdAsync("user-1");

        result.Should().BeSameAs(user);
    }

    [Fact]
    public async Task CreateRoleManager_WithoutStubs_RoleExistsReturnsFalse()
    {
        var roleManager = IdentityTestDoubles.CreateRoleManager();

        var exists = await roleManager.RoleExistsAsync("SystemAdmin");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CreateRoleManager_CanBeStubbedAfterCreation()
    {
        var roleManager = IdentityTestDoubles.CreateRoleManager();
        roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(Task.FromResult(true));

        var exists = await roleManager.RoleExistsAsync("Anything");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CreateRoleManagerWithRoles_KnownRole_ReturnsTrue()
    {
        var roleManager = IdentityTestDoubles.CreateRoleManagerWithRoles("SystemAdmin", "DataAdmin");

        (await roleManager.RoleExistsAsync("SystemAdmin")).Should().BeTrue();
        (await roleManager.RoleExistsAsync("DataAdmin")).Should().BeTrue();
    }

    [Fact]
    public async Task CreateRoleManagerWithRoles_UnknownRole_ReturnsFalse()
    {
        var roleManager = IdentityTestDoubles.CreateRoleManagerWithRoles("SystemAdmin");

        var exists = await roleManager.RoleExistsAsync("GhostRole");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CreateRoleManagerWithRoles_NoRoles_AllRolesUnknown()
    {
        var roleManager = IdentityTestDoubles.CreateRoleManagerWithRoles();

        var exists = await roleManager.RoleExistsAsync("SystemAdmin");

        exists.Should().BeFalse();
    }
}