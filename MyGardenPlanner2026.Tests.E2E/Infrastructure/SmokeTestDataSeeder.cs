namespace MyGardenPlanner2026.Tests.E2E.Infrastructure;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        using var scope = provider.CreateScope(); var services = new ServiceCollection();

        //services.AddDbContext<PlannerDbContext>(options =>
        //    options.UseSqlServer(connectionString)
        //           .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

        //services.AddDataProtection();

        //services.AddIdentityCore<ApplicationUser>()
        //    .AddRoles<IdentityRole>()
        //    .AddEntityFrameworkStores<PlannerDbContext>()
        //    .AddDefaultTokenProviders();

        //await using var provider = services.BuildServiceProvider();
        //using var scope = provider.CreateScope();

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

        var createResult = await userManager.CreateAsync(user, SharedPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Kunne ikke oprette smoke-test-bruger '{email}': " +
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }

        if (role is not null)
        {
            var addToRoleResult = await userManager.AddToRoleAsync(user, role);
            if (!addToRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Kunne ikke tildele rollen '{role}' til '{email}': " +
                    string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
            }
        }

        string? authenticatorKey = null;
        if (twoFactorEnabled)
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);

            var setTwoFactorResult = await userManager.SetTwoFactorEnabledAsync(user, true);
            if (!setTwoFactorResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Kunne ikke aktivere 2FA for '{email}': " +
                    string.Join(", ", setTwoFactorResult.Errors.Select(e => e.Description)));
            }
        }

        return new SmokeTestUser(email, SharedPassword, role, twoFactorEnabled, authenticatorKey);
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
            options.UseSqlServer(connectionString)
                   .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

        services.AddDataProtection();

        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<PlannerDbContext>()
            .AddDefaultTokenProviders();

        return services.BuildServiceProvider();
    }
}