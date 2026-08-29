namespace MyGardenPlanner2026.Tests.Unit.Infrastructure.Seed;

using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using NSubstitute;
using Xunit;

public class IdentityBootstrapSeederTests
{
    private static UserManager<ApplicationUser> CreateUserManager() =>
        Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

    private static RoleManager<IdentityRole> CreateRoleManager()
    {
        var roleManager = Substitute.For<RoleManager<IdentityRole>>(
            Substitute.For<IRoleStore<IdentityRole>>(), null, null, null, null);

        // Standardadfærd for alle roller, medmindre en test eksplicit overstyrer for en
        // specifik rolle: rollen "findes allerede" (ingen CreateAsync-kald), og hvis en
        // test alligevel lader en rolle "ikke findes", lykkes oprettelsen som fallback.
        // Forhindrer NullReferenceException i EnsureRoleExistsAsync for roller (DataAdmin/
        // PolicyAdmin/AuditViewer), som den enkelte test ikke selv stubber.
        roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(Task.FromResult(true));
        roleManager.CreateAsync(Arg.Any<IdentityRole>()).Returns(Task.FromResult(IdentityResult.Success));

        return roleManager;
    }

    private static IdentityBootstrapSeeder CreateSeeder(
        UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, InitialAdminOptions options) =>
        new(userManager, roleManager, Options.Create(options), Substitute.For<ILogger<IdentityBootstrapSeeder>>());

    [Fact]
    public async Task SeedAsync_RoleDoesNotExist_CreatesSystemAdminRole()
    {
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        roleManager.RoleExistsAsync(RoleNames.SystemAdmin).Returns(Task.FromResult(false));
        roleManager.CreateAsync(Arg.Any<IdentityRole>()).Returns(Task.FromResult(IdentityResult.Success));
        userManager.GetUsersInRoleAsync(RoleNames.SystemAdmin).Returns(Task.FromResult<IList<ApplicationUser>>([]));

        var seeder = CreateSeeder(userManager, roleManager, new InitialAdminOptions());

        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        await roleManager.Received().CreateAsync(Arg.Is<IdentityRole>(r => r.Name == RoleNames.SystemAdmin));
    }

    [Fact]
    public async Task SeedAsync_RoleAlreadyHasMembers_SkipsUserCreation()
    {
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        roleManager.RoleExistsAsync(RoleNames.SystemAdmin).Returns(Task.FromResult(true));
        userManager.GetUsersInRoleAsync(RoleNames.SystemAdmin)
            .Returns(Task.FromResult<IList<ApplicationUser>>([new ApplicationUser { Email = "existing@mygardenplanner.dk" }]));

        var seeder = CreateSeeder(userManager, roleManager, new InitialAdminOptions
        {
            Email = "admin@mygardenplanner.dk",
            Password = "P@ssw0rd123!"
        });

        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        await userManager.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task SeedAsync_NoCredentialsConfigured_SkipsWithoutThrowing()
    {
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        roleManager.RoleExistsAsync(RoleNames.SystemAdmin).Returns(Task.FromResult(true));
        userManager.GetUsersInRoleAsync(RoleNames.SystemAdmin).Returns(Task.FromResult<IList<ApplicationUser>>([]));

        var seeder = CreateSeeder(userManager, roleManager, new InitialAdminOptions());

        var act = async () => await seeder.SeedAsync();

        await act.Should().NotThrowAsync();
        await userManager.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task SeedAsync_UserWithEmailAlreadyExists_AssignsRoleWithoutCreatingUser()
    {
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        var existingUser = new ApplicationUser { Email = "admin@mygardenplanner.dk" };

        roleManager.RoleExistsAsync(RoleNames.SystemAdmin).Returns(Task.FromResult(true));
        userManager.GetUsersInRoleAsync(RoleNames.SystemAdmin).Returns(Task.FromResult<IList<ApplicationUser>>([]));
        userManager.FindByEmailAsync("admin@mygardenplanner.dk").Returns(Task.FromResult<ApplicationUser?>(existingUser));
        userManager.AddToRoleAsync(existingUser, RoleNames.SystemAdmin).Returns(Task.FromResult(IdentityResult.Success));

        var seeder = CreateSeeder(userManager, roleManager, new InitialAdminOptions
        {
            Email = "admin@mygardenplanner.dk",
            Password = "P@ssw0rd123!"
        });

        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        await userManager.Received().AddToRoleAsync(existingUser, RoleNames.SystemAdmin);
        await userManager.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task SeedAsync_ValidConfiguredCredentials_CreatesUserWithConfirmedEmailAndAssignsRole()
    {
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();

        roleManager.RoleExistsAsync(RoleNames.SystemAdmin).Returns(Task.FromResult(true));
        userManager.GetUsersInRoleAsync(RoleNames.SystemAdmin).Returns(Task.FromResult<IList<ApplicationUser>>([]));
        userManager.FindByEmailAsync("admin@mygardenplanner.dk").Returns(Task.FromResult<ApplicationUser?>(null));
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), "P@ssw0rd123!").Returns(Task.FromResult(IdentityResult.Success));
        userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), RoleNames.SystemAdmin).Returns(Task.FromResult(IdentityResult.Success));

        var seeder = CreateSeeder(userManager, roleManager, new InitialAdminOptions
        {
            Email = "admin@mygardenplanner.dk",
            Password = "P@ssw0rd123!"
        });

        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        await userManager.Received().CreateAsync(
            Arg.Is<ApplicationUser>(u => u.Email == "admin@mygardenplanner.dk" && u.EmailConfirmed),
            "P@ssw0rd123!");
        await userManager.Received().AddToRoleAsync(
            Arg.Is<ApplicationUser>(u => u.Email == "admin@mygardenplanner.dk"), RoleNames.SystemAdmin);
    }

    [Fact]
    public async Task SeedAsync_CreateAsyncFails_ThrowsInvalidOperationException()
    {
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();

        roleManager.RoleExistsAsync(RoleNames.SystemAdmin).Returns(Task.FromResult(true));
        userManager.GetUsersInRoleAsync(RoleNames.SystemAdmin).Returns(Task.FromResult<IList<ApplicationUser>>([]));
        userManager.FindByEmailAsync("admin@mygardenplanner.dk").Returns(Task.FromResult<ApplicationUser?>(null));
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Password too weak." })));

        var seeder = CreateSeeder(userManager, roleManager, new InitialAdminOptions
        {
            Email = "admin@mygardenplanner.dk",
            Password = "weak"
        });

        var act = async () => await seeder.SeedAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Password too weak*");
    }

    [Theory]
    [InlineData(RoleNames.DataAdmin)]
    [InlineData(RoleNames.PolicyAdmin)]
    [InlineData(RoleNames.AuditViewer)]
    public async Task SeedAsync_AdditionalRoleDoesNotExist_CreatesRole(string roleName)
    {
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(Task.FromResult(false));
        roleManager.CreateAsync(Arg.Any<IdentityRole>()).Returns(Task.FromResult(IdentityResult.Success));
        userManager.GetUsersInRoleAsync(RoleNames.SystemAdmin).Returns(Task.FromResult<IList<ApplicationUser>>([]));

        var seeder = CreateSeeder(userManager, roleManager, new InitialAdminOptions());

        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        await roleManager.Received().CreateAsync(Arg.Is<IdentityRole>(r => r.Name == roleName));
    }

    [Fact]
    public async Task SeedAsync_ChecksExistenceForAllFourRoles()
    {
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(Task.FromResult(true));
        userManager.GetUsersInRoleAsync(RoleNames.SystemAdmin).Returns(Task.FromResult<IList<ApplicationUser>>([]));

        var seeder = CreateSeeder(userManager, roleManager, new InitialAdminOptions());

        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        await roleManager.Received(1).RoleExistsAsync(RoleNames.SystemAdmin);
        await roleManager.Received(1).RoleExistsAsync(RoleNames.DataAdmin);
        await roleManager.Received(1).RoleExistsAsync(RoleNames.PolicyAdmin);
        await roleManager.Received(1).RoleExistsAsync(RoleNames.AuditViewer);
    }
}