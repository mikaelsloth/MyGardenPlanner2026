namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Seeder til de 7 faste smoke-test-brugere. Kører FØR appen startes som separat proces
/// (se PlaywrightAppFixture), direkte mod engangs-databasen via sin egen minimale
/// Identity-opsætning — ingen afhængighed af den kørende app. Rollerne oprettes idempotent
/// her, så IdentityBootstrapSeeder i selve appen ikke opretter en InitialAdmin-bruger ved
/// siden af (den seeder kun hvis SystemAdmin-rollen er tom, hvilket den ikke er herefter).
/// </summary>
public static class SmokeTestDataSeeder
{
    public const string SharedPassword = "Smoketest123!";

    public static async Task<IReadOnlyDictionary<string, SmokeTestUser>> SeedAsync(string connectionString)
    {
        await using var provider = BuildServiceProvider(connectionString);
        using var scope = provider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[]
                 { RoleNames.SystemAdmin, RoleNames.DataAdmin, RoleNames.PolicyAdmin, RoleNames.AuditViewer })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        return new Dictionary<string, SmokeTestUser>
        {
            ["Admin"] = await CreateUserAsync(userManager, "admin@test.dk", RoleNames.SystemAdmin, twoFactorEnabled: true),
            ["DataAdmin"] = await CreateUserAsync(userManager, "dataadmin@test.dk", RoleNames.DataAdmin, twoFactorEnabled: true),
            ["PolicyAdmin"] = await CreateUserAsync(userManager, "policyadmin@test.dk", RoleNames.PolicyAdmin, twoFactorEnabled: true),
            ["Auditor"] = await CreateUserAsync(userManager, "auditor@test.dk", RoleNames.AuditViewer, twoFactorEnabled: true),
            ["NoMfa"] = await CreateUserAsync(userManager, "noMfa@test.dk", RoleNames.SystemAdmin, twoFactorEnabled: false),
            ["Requester"] = await CreateUserAsync(userManager, "requester@test.dk", role: null, twoFactorEnabled: true),
            ["Plain"] = await CreateUserAsync(userManager, "plain@test.dk", role: null, twoFactorEnabled: false),
        };
    }

    private static async Task<SmokeTestUser> CreateUserAsync(
        UserManager<ApplicationUser> userManager, string email, string? role, bool twoFactorEnabled)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        EnsureSucceeded(
            await userManager.CreateAsync(user, SharedPassword),
            $"Kunne ikke oprette smoke-test-bruger '{email}'");

        if (role is not null)
        {
            EnsureSucceeded(
                await userManager.AddToRoleAsync(user, role),
                $"Kunne ikke tildele rollen '{role}' til '{email}'");
        }

        string? authenticatorKey = null;
        if (twoFactorEnabled)
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);

            EnsureSucceeded(
                await userManager.SetTwoFactorEnabledAsync(user, true),
                $"Kunne ikke aktivere 2FA for '{email}'");
        }

        return new SmokeTestUser(email, SharedPassword, role, twoFactorEnabled, authenticatorKey);
    }

    internal static void EnsureSucceeded(IdentityResult result, string context)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"{context}: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    /// <summary>
    /// Diagnostik-hjælper: verificerer en TOTP-kode direkte mod databasen, uden browser
    /// eller separat app-proces involveret. Bruges til at isolere om en "Ugyldig
    /// godkendelseskode"-fejl skyldes encoding-mismatch (Otp.NET vs. Identitys eget
    /// Base32) eller timing/latency i selve E2E-flowet.
    /// </summary>
    public static async Task<bool> VerifyAuthenticatorCodeAsync(string connectionString, string email, string code)
    {
        await using var provider = BuildServiceProvider(connectionString);
        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email)
            ?? throw new InvalidOperationException($"Bruger '{email}' findes ikke.");

        return await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code);
    }

    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        var services = new ServiceCollection();

        services.AddDbContext<PlannerDbContext>(options =>
            E2ETestDatabase.ConfigureContextOptions(options, connectionString));

        services.AddDataProtection();

        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<PlannerDbContext>()
            .AddDefaultTokenProviders();

        return services.BuildServiceProvider();
    }
}