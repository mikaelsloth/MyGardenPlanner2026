namespace MyGardenPlanner2026.Infrastructure.Data.Seed;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Idempotent seeder til de 7 smoke-test-logins (kun Debug/Development, se Program.cs).
/// Sikrer pr. bruger: eksistens (EmailConfirmed), direkte Identity-rolle og den ønskede
/// 2FA-tilstand med en kendt TOTP-nøgle. Eksisterende brugere får ALDRIG nulstillet
/// adgangskode. Skal køre EFTER IdentityBootstrapSeeder, som opretter rollerne.
/// </summary>
public sealed partial class SmokeTestUserSeeder(
    UserManager<ApplicationUser> userManager,
    IOptions<SmokeTestUsersOptions> options,
    ILogger<SmokeTestUserSeeder> logger)
{
    // Identity's interne token-navne (UserManager.GetAuthenticatorKeyAsync læser herfra).
    // Selvtjekket i EnsureTwoFactorAsync fanger det, hvis de skulle ændre sig.
    private const string AuthenticatorTokenLoginProvider = "[AspNetUserStore]";
    private const string AuthenticatorTokenName = "AuthenticatorKey";

    private sealed record SmokeTestUser(string Email, string? RoleName, bool TwoFactorEnabled);

    private static readonly SmokeTestUser[] Users =
    [
        new("admin@test.dk", RoleNames.SystemAdmin, true),
        new("dataadmin@test.dk", RoleNames.DataAdmin, true),
        new("policyadmin@test.dk", RoleNames.PolicyAdmin, true),
        new("auditor@test.dk", RoleNames.AuditViewer, true),
        new("noMfa@test.dk", RoleNames.SystemAdmin, false),
        new("requester@test.dk", null, true),
        new("plain@test.dk", null, false)
    ];

    [LoggerMessage(EventId = 1110, Level = LogLevel.Warning, Message = "Smoke-test-brugere springes over: SmokeTestUsers:Password/AuthenticatorKey er ikke konfigureret. Sæt dem via 'dotnet user-secrets'.")]
    static partial void NotConfigured(ILogger logger);

    [LoggerMessage(EventId = 1111, Level = LogLevel.Information, Message = "Smoke-test-bruger '{Email}' oprettet.")]
    static partial void UserCreated(ILogger logger, string Email);

    [LoggerMessage(EventId = 1112, Level = LogLevel.Information, Message = "Smoke-test-bruger '{Email}' tilføjet til rollen '{RoleName}'.")]
    static partial void RoleAssigned(ILogger logger, string Email, string RoleName);

    [LoggerMessage(EventId = 1113, Level = LogLevel.Information, Message = "Smoke-test-bruger '{Email}': totrinsbekræftelse sat til {Enabled}.")]
    static partial void TwoFactorStateSet(ILogger logger, string Email, bool Enabled);

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Password) || string.IsNullOrWhiteSpace(settings.AuthenticatorKey))
        {
            NotConfigured(logger);
            return;
        }

        EnsureValidBase32(settings.AuthenticatorKey);

        foreach (var definition in Users)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var user = await EnsureUserAsync(definition, settings.Password);
            await EnsureRoleAsync(user, definition);
            await EnsureTwoFactorAsync(user, definition, settings.AuthenticatorKey);
        }
    }

    private async Task<ApplicationUser> EnsureUserAsync(SmokeTestUser definition, string password)
    {
        var existing = await userManager.FindByEmailAsync(definition.Email);
        if (existing is not null)
        {
            return existing;
        }

        var user = new ApplicationUser
        {
            UserName = definition.Email,
            Email = definition.Email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Kunne ikke oprette smoke-test-bruger '{definition.Email}': {DescribeErrors(result)}");
        }

        UserCreated(logger, definition.Email);
        return user;
    }

    private async Task EnsureRoleAsync(ApplicationUser user, SmokeTestUser definition)
    {
        if (definition.RoleName is null || await userManager.IsInRoleAsync(user, definition.RoleName))
        {
            return;
        }

        var result = await userManager.AddToRoleAsync(user, definition.RoleName);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Kunne ikke tildele rollen '{definition.RoleName}' til '{definition.Email}': {DescribeErrors(result)}");
        }

        RoleAssigned(logger, definition.Email, definition.RoleName);
    }

    private async Task EnsureTwoFactorAsync(ApplicationUser user, SmokeTestUser definition, string authenticatorKey)
    {
        if (definition.TwoFactorEnabled && await userManager.GetAuthenticatorKeyAsync(user) != authenticatorKey)
        {
            var tokenResult = await userManager.SetAuthenticationTokenAsync(
                user, AuthenticatorTokenLoginProvider, AuthenticatorTokenName, authenticatorKey);
            if (!tokenResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Kunne ikke sætte TOTP-nøgle for '{definition.Email}': {DescribeErrors(tokenResult)}");
            }

            if (await userManager.GetAuthenticatorKeyAsync(user) != authenticatorKey)
            {
                throw new InvalidOperationException(
                    "TOTP-nøglen blev gemt, men kan ikke læses tilbage via UserManager — Identity's interne token-navne har muligvis ændret sig.");
            }
        }

        if (await userManager.GetTwoFactorEnabledAsync(user) == definition.TwoFactorEnabled)
        {
            return;
        }

        var result = await userManager.SetTwoFactorEnabledAsync(user, definition.TwoFactorEnabled);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Kunne ikke sætte 2FA for '{definition.Email}': {DescribeErrors(result)}");
        }

        TwoFactorStateSet(logger, definition.Email, definition.TwoFactorEnabled);
    }

    private static void EnsureValidBase32(string key)
    {
        if (key.Length < 16 || !key.All(c => c is (>= 'A' and <= 'Z') or (>= '2' and <= '7')))
        {
            throw new InvalidOperationException(
                "SmokeTestUsers:AuthenticatorKey skal være Base32 (A-Z, 2-7) og mindst 16 tegn.");
        }
    }

    private static string DescribeErrors(IdentityResult result) =>
        string.Join(", ", result.Errors.Select(e => e.Description));
}