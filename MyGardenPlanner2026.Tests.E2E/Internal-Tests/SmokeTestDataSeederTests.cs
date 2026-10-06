namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Verificerer SmokeTestDataSeeder isoleret mod sin egen engangs-database — uden
/// PlaywrightAppCollection/browser, for hurtig feedback hvis seed-logikken går i stykker.
/// Kører samme lokal/CI-skift som PlaywrightAppFixture via E2ESqlEnvironment.
/// </summary>
public sealed class SmokeTestDataSeederTests : IAsyncLifetime
{
    private readonly string _databaseName = default!;

    private E2ETestDatabase _database = default!;

    public async ValueTask InitializeAsync() => _database = await E2ETestDatabase.CreateAsync();

    public async ValueTask DisposeAsync()
    {
        if (_database is not null)
        {
            await _database.DisposeAsync();
        }
    }

    [Fact]
    public async Task SeedAsync_OpretterAlleSyvBrugereMedKorrekteRollerOgToFactorFlag()
    {
        var connectionString = _database.AppConnectionString;
        var users = await SmokeTestDataSeeder.SeedAsync(connectionString);

        users.Should().HaveCount(7);

        AssertUser(users, "Admin", "admin@test.dk", RoleNames.SystemAdmin, twoFactorEnabled: true);
        AssertUser(users, "DataAdmin", "dataadmin@test.dk", RoleNames.DataAdmin, twoFactorEnabled: true);
        AssertUser(users, "PolicyAdmin", "policyadmin@test.dk", RoleNames.PolicyAdmin, twoFactorEnabled: true);
        AssertUser(users, "Auditor", "auditor@test.dk", RoleNames.AuditViewer, twoFactorEnabled: true);
        AssertUser(users, "NoMfa", "noMfa@test.dk", RoleNames.SystemAdmin, twoFactorEnabled: false);
        AssertUser(users, "Requester", "requester@test.dk", role: null, twoFactorEnabled: true);
        AssertUser(users, "Plain", "plain@test.dk", role: null, twoFactorEnabled: false);
    }

    [Fact]
    public async Task SeedAsync_GenereretTotpKode_ErGyldigUmiddelbartEfterSeeding()
    {
        var connectionString = _database.AppConnectionString;
        var users = await SmokeTestDataSeeder.SeedAsync(connectionString);
        var admin = users["Admin"];

        var code = TotpHelper.GenerateCode(admin.AuthenticatorKey!);
        var isValid = await SmokeTestDataSeeder.VerifyAuthenticatorCodeAsync(connectionString, admin.Email, code);

        isValid.Should().BeTrue(
            "koden er genereret ud fra nøglen umiddelbart efter seeding, uden browser eller separat proces involveret");
    }

    private static void AssertUser(
        IReadOnlyDictionary<string, SmokeTestUser> users, string key, string expectedEmail,
        string? role, bool twoFactorEnabled)
    {
        users.Should().ContainKey(key);
        var user = users[key];

        user.Email.Should().Be(expectedEmail);
        user.Role.Should().Be(role);
        user.TwoFactorEnabled.Should().Be(twoFactorEnabled);

        if (twoFactorEnabled)
        {
            user.AuthenticatorKey.Should().NotBeNullOrWhiteSpace();
        }
        else
        {
            user.AuthenticatorKey.Should().BeNull();
        }
    }
}