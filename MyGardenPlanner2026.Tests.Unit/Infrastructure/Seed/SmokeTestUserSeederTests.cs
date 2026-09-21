namespace MyGardenPlanner2026.Tests.Unit.Infrastructure.Seed;

using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using NSubstitute;
using Xunit;

public class SmokeTestUserSeederTests
{
    private const string ValidKey = "JBSWY3DPEHPK3PXPJBSWY3DPEHPK3PXP";

    private static UserManager<ApplicationUser> CreateUserManager(Dictionary<string, string> authenticatorKeys)
    {
        var userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

        userManager.FindByEmailAsync(Arg.Any<string>()).Returns(Task.FromResult<ApplicationUser?>(null));
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.IsInRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(false);
        userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.GetTwoFactorEnabledAsync(Arg.Any<ApplicationUser>()).Returns(false);
        userManager.SetTwoFactorEnabledAsync(Arg.Any<ApplicationUser>(), Arg.Any<bool>()).Returns(IdentityResult.Success);

        userManager
            .SetAuthenticationTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>())
            .Returns(call =>
            {
                authenticatorKeys[call.ArgAt<ApplicationUser>(0).Email!] = call.ArgAt<string>(3);
                return IdentityResult.Success;
            });

        userManager
            .GetAuthenticatorKeyAsync(Arg.Any<ApplicationUser>())
            .Returns(call => authenticatorKeys.GetValueOrDefault(call.ArgAt<ApplicationUser>(0).Email!));

        return userManager;
    }

    private static SmokeTestUserSeeder CreateSeeder(
        UserManager<ApplicationUser> userManager, string? password = "Smoke-Test-2026", string? key = ValidKey) =>
        new(userManager,
            Options.Create(new SmokeTestUsersOptions { Password = password, AuthenticatorKey = key }),
            NullLogger<SmokeTestUserSeeder>.Instance);

    [Fact]
    public async Task SeedAsync_NotConfigured_SkipsWithoutCreatingUsers()
    {
        var userManager = CreateUserManager([]);

        await CreateSeeder(userManager, password: null).SeedAsync(TestContext.Current.CancellationToken);

        _ = userManager.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Theory]
    [InlineData("kort")]
    [InlineData("jbswy3dpehpk3pxp")]
    [InlineData("JBSWY3DPEHPK3PX1")]
    public async Task SeedAsync_InvalidAuthenticatorKey_ThrowsInvalidOperationException(string key)
    {
        var userManager = CreateUserManager([]);

        var act = () => CreateSeeder(userManager, key: key).SeedAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SeedAsync_EmptyDatabase_CreatesSevenConfirmedUsers()
    {
        var userManager = CreateUserManager([]);

        await CreateSeeder(userManager).SeedAsync(TestContext.Current.CancellationToken);

        _ = userManager.Received(7).CreateAsync(
            Arg.Is<ApplicationUser>(u => u.EmailConfirmed && u.UserName == u.Email && u.Email!.EndsWith("@test.dk")),
            Arg.Is<string>(p => p == "Smoke-Test-2026"));
    }

    [Theory]
    [InlineData("admin@test.dk", RoleNames.SystemAdmin)]
    [InlineData("dataadmin@test.dk", RoleNames.DataAdmin)]
    [InlineData("policyadmin@test.dk", RoleNames.PolicyAdmin)]
    [InlineData("auditor@test.dk", RoleNames.AuditViewer)]
    [InlineData("noMfa@test.dk", RoleNames.SystemAdmin)]
    public async Task SeedAsync_UsersWithRole_AreAddedToExactlyThatRole(string email, string role)
    {
        var userManager = CreateUserManager([]);

        await CreateSeeder(userManager).SeedAsync(TestContext.Current.CancellationToken);

        _ = userManager.Received(1).AddToRoleAsync(
            Arg.Is<ApplicationUser>(u => u.Email == email), Arg.Is<string>(r => r == role));
        _ = userManager.Received(1).AddToRoleAsync(
            Arg.Is<ApplicationUser>(u => u.Email == email), Arg.Any<string>());
    }

    [Theory]
    [InlineData("requester@test.dk")]
    [InlineData("plain@test.dk")]
    public async Task SeedAsync_UsersWithoutRole_AreNeverAddedToARole(string email)
    {
        var userManager = CreateUserManager([]);

        await CreateSeeder(userManager).SeedAsync(TestContext.Current.CancellationToken);

        _ = userManager.DidNotReceive().AddToRoleAsync(
            Arg.Is<ApplicationUser>(u => u.Email == email), Arg.Any<string>());
    }

    [Theory]
    [InlineData("admin@test.dk")]
    [InlineData("dataadmin@test.dk")]
    [InlineData("policyadmin@test.dk")]
    [InlineData("auditor@test.dk")]
    [InlineData("requester@test.dk")]
    public async Task SeedAsync_TwoFactorUsers_GetKnownKeyAndTwoFactorEnabled(string email)
    {
        var keys = new Dictionary<string, string>();
        var userManager = CreateUserManager(keys);

        await CreateSeeder(userManager).SeedAsync(TestContext.Current.CancellationToken);

        keys.Should().ContainKey(email).WhoseValue.Should().Be(ValidKey);
        _ = userManager.Received(1).SetTwoFactorEnabledAsync(
            Arg.Is<ApplicationUser>(u => u.Email == email), Arg.Is<bool>(enabled => enabled));
    }

    [Theory]
    [InlineData("noMfa@test.dk")]
    [InlineData("plain@test.dk")]
    public async Task SeedAsync_UsersWithoutTwoFactor_GetNeitherKeyNorTwoFactor(string email)
    {
        var keys = new Dictionary<string, string>();
        var userManager = CreateUserManager(keys);

        await CreateSeeder(userManager).SeedAsync(TestContext.Current.CancellationToken);

        keys.Should().NotContainKey(email);
        _ = userManager.DidNotReceive().SetTwoFactorEnabledAsync(
            Arg.Is<ApplicationUser>(u => u.Email == email), Arg.Any<bool>());
    }

    [Fact]
    public async Task SeedAsync_ExistingUsers_AreNotRecreated()
    {
        var userManager = CreateUserManager([]);
        userManager.FindByEmailAsync(Arg.Any<string>())
            .Returns(call => Task.FromResult<ApplicationUser?>(new ApplicationUser { Email = call.ArgAt<string>(0) }));

        await CreateSeeder(userManager).SeedAsync(TestContext.Current.CancellationToken);

        _ = userManager.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task SeedAsync_CreateFails_ThrowsInvalidOperationException()
    {
        var userManager = CreateUserManager([]);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Password for svagt." }));

        var act = () => CreateSeeder(userManager).SeedAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Password for svagt.*");
    }

    [Fact]
    public async Task SeedAsync_KeyCannotBeReadBack_ThrowsInvalidOperationException()
    {
        var userManager = CreateUserManager([]);
        userManager.GetAuthenticatorKeyAsync(Arg.Any<ApplicationUser>()).Returns((string?)null);

        var act = () => CreateSeeder(userManager).SeedAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*token-navne*");
    }
}