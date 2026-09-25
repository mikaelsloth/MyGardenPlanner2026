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
/// </summary>
public sealed class SmokeTestDataSeederTests : IAsyncLifetime
{
    private const string SqlExpressServer = @".\SQLEXPRESS";

    private string _databaseName = default!;
    private string _connectionString = default!;

    public async ValueTask InitializeAsync()
    {
        _databaseName = $"MyGardenPlanner2026_E2E_SeederTest_{Guid.NewGuid():N}";
        _connectionString =
            $@"Server={SqlExpressServer};Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<PlannerDbContext>()
            .UseSqlServer(_connectionString)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        await using var context = new PlannerDbContext(options);
        await context.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        var masterConnectionString =
            $@"Server={SqlExpressServer};Database=master;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<PlannerDbContext>()
            .UseSqlServer(masterConnectionString)
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
        var users = await SmokeTestDataSeeder.SeedAsync(_connectionString);

        users.Should().HaveCount(7);

        AssertUser(users, "Admin", "admin@test.dk", RoleNames.SystemAdmin, twoFactorEnabled: true);
        AssertUser(users, "DataAdmin", "dataadmin@test.dk", RoleNames.DataAdmin, twoFactorEnabled: true);
        AssertUser(users, "PolicyAdmin", "policyadmin@test.dk", RoleNames.PolicyAdmin, twoFactorEnabled: true);
        AssertUser(users, "Auditor", "auditor@test.dk", RoleNames.AuditViewer, twoFactorEnabled: true);
        AssertUser(users, "NoMfa", "noMfa@test.dk", RoleNames.SystemAdmin, twoFactorEnabled: false);
        AssertUser(users, "Requester", "requester@test.dk", role: null, twoFactorEnabled: true);
        AssertUser(users, "Plain", "plain@test.dk", role: null, twoFactorEnabled: false);
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
    public async Task SeedAsync_GenereretTotpKode_ErGyldigUmiddelbartEfterSeeding()
    {
        var users = await SmokeTestDataSeeder.SeedAsync(_connectionString);
        var admin = users["Admin"];

        var code = TotpHelper.GenerateCode(admin.AuthenticatorKey!);
        var isValid = await SmokeTestDataSeeder.VerifyAuthenticatorCodeAsync(_connectionString, admin.Email, code);

        isValid.Should().BeTrue(
            "koden er genereret ud fra nøglen umiddelbart efter seeding, uden browser eller separat proces involveret");
    }
}