namespace MyGardenPlanner2026.Tests.Unit;

using Microsoft.AspNetCore.Identity;
using MyGardenPlanner2026.Core.Entities;
using NSubstitute;

/// <summary>
/// NSubstitute-substitutter for ASP.NET Core Identity-managere. UserManager og RoleManager
/// har konstruktører med mange afhængigheder, som tests ikke bruger; dem sendes som null.
/// </summary>
internal static class IdentityTestDoubles
{
    /// <summary>UserManager uden nogen stubs; alle kald returnerer NSubstitute-standardværdier.</summary>
    public static UserManager<ApplicationUser> CreateUserManager() =>
        Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

    /// <summary>RoleManager uden nogen stubs; RoleExistsAsync returnerer false for alle roller.</summary>
    public static RoleManager<IdentityRole> CreateRoleManager() =>
        Substitute.For<RoleManager<IdentityRole>>(
            Substitute.For<IRoleStore<IdentityRole>>(), null!, null!, null!, null!);

    /// <summary>RoleManager hvor RoleExistsAsync kun returnerer true for de angivne roller.</summary>
    public static RoleManager<IdentityRole> CreateRoleManagerWithRoles(params string[] knownRoles)
    {
        var roleManager = CreateRoleManager();
        roleManager.RoleExistsAsync(Arg.Any<string>())
            .Returns(callInfo => Task.FromResult(knownRoles.Contains(callInfo.Arg<string>())));
        return roleManager;
    }
}