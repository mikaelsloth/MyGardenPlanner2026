namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Verificerer SmokeTestDataSeeder isoleret mod sin egen engangs-database — uden
/// PlaywrightAppCollection/browser, for hurtig feedback hvis seed-logikken går i stykker.
/// Kører samme lokal/CI-skift som PlaywrightAppFixture via E2ESqlEnvironment.
/// </summary>
public sealed class SmokeTestDataSeederTests : IAsyncLifetime
{
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

        AssertUser(users, SmokeTestPersonas.Admin, "admin@test.dk", RoleNames.SystemAdmin, twoFactorEnabled: true);
        AssertUser(users, SmokeTestPersonas.DataAdmin, "dataadmin@test.dk", RoleNames.DataAdmin, twoFactorEnabled: true);
        AssertUser(users, SmokeTestPersonas.PolicyAdmin, "policyadmin@test.dk", RoleNames.PolicyAdmin, twoFactorEnabled: true);
        AssertUser(users, SmokeTestPersonas.Auditor, "auditor@test.dk", RoleNames.AuditViewer, twoFactorEnabled: true);
        AssertUser(users, SmokeTestPersonas.NoMfa, "noMfa@test.dk", RoleNames.SystemAdmin, twoFactorEnabled: false);
        AssertUser(users, SmokeTestPersonas.Requester, "requester@test.dk", role: null, twoFactorEnabled: true);
        AssertUser(users, SmokeTestPersonas.Plain, "plain@test.dk", role: null, twoFactorEnabled: false);
    }

    [Fact]
    public async Task SeedAsync_GenereretTotpKode_ErGyldigUmiddelbartEfterSeeding()
    {
        var connectionString = _database.AppConnectionString;
        var users = await SmokeTestDataSeeder.SeedAsync(connectionString);
        var admin = users[SmokeTestPersonas.Admin];

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

    [Fact]
    public async Task LockOutUserAsync_SaetterLockoutEndIFremtiden()
    {
        var connectionString = _database.AppConnectionString;
        await SmokeTestDataSeeder.SeedAsync(connectionString);

        await SmokeTestDataSeeder.LockOutUserAsync(connectionString, "plain@test.dk");

        await using var context = new PlannerDbContext(E2ETestDatabase.CreateContextOptions(connectionString));
        var user = await context.Set<ApplicationUser>()
            .SingleAsync(u => u.Email == "plain@test.dk", TestContext.Current.CancellationToken);

        user.LockoutEnabled.Should().BeTrue();
        user.LockoutEnd.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task GetUserIdAsync_GiverIdetPaaDenSeededBruger()
    {
        var connectionString = _database.AppConnectionString;
        await SmokeTestDataSeeder.SeedAsync(connectionString);

        var id = await SmokeTestDataSeeder.GetUserIdAsync(connectionString, "plain@test.dk");

        await using var context = new PlannerDbContext(E2ETestDatabase.CreateContextOptions(connectionString));
        var expected = await context.Set<ApplicationUser>()
            .Where(u => u.Email == "plain@test.dk")
            .Select(u => u.Id)
            .SingleAsync(TestContext.Current.CancellationToken);

        id.Should().Be(expected);
    }
}