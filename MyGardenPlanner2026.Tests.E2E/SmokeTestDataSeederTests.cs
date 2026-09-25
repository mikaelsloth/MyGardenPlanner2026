namespace MyGardenPlanner2026.Tests.E2E;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Tests.E2E.Infrastructure;
using Xunit;

/// <summary>
/// Verificerer SmokeTestDataSeeder isoleret mod sin egen engangs-database — uden
/// PlaywrightAppCollection/browser, for hurtig feedback hvis seed-logikken går i stykker.
/// Kører samme lokal/CI-skift som PlaywrightAppFixture via E2ESqlEnvironment.
/// </summary>
public sealed class SmokeTestDataSeederTests : IAsyncLifetime
{
    private string _databaseName = default!;

    public async ValueTask InitializeAsync()
    {
        _databaseName = E2ESqlEnvironment.ResolveDatabaseName();

        if (E2ESqlEnvironment.IsCi)
        {
            await CiSqlProvisioner.ProvisionAsync(_databaseName);
        }

        var options = new DbContextOptionsBuilder<PlannerDbContext>()
            .UseSqlServer(E2ESqlEnvironment.MigrationConnectionString(_databaseName))
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        await using var context = new PlannerDbContext(options);
        await context.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (E2ESqlEnvironment.IsCi)
        {
            return;
        }

        var options = new DbContextOptionsBuilder<PlannerDbContext>()
            .UseSqlServer(E2ESqlEnvironment.MasterConnectionString())
            .Options;

        await using var context = new PlannerDbContext(options);
#pragma warning disable EF1003 // Risk of vulnerability to SQL injection.
        await context.Database.ExecuteSqlRawAsync(
            $"ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            $"DROP DATABASE [{_databaseName}];");
#pragma warning restore EF1003 // Risk of vulnerability to SQL injection.
    }

    [Fact]
    public async Task SeedAsync_OpretterAlleSyvBrugereMedKorrekteRollerOgToFactorFlag()
    {
        var connectionString = E2ESqlEnvironment.AppConnectionString(_databaseName);
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
        var connectionString = E2ESqlEnvironment.AppConnectionString(_databaseName);
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