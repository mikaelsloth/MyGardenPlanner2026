namespace MyGardenPlanner2026.Infrastructure.Data.Seed;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Bootstrapper SystemAdmin-rollen og (hvis konfigureret) den første SystemAdmin-bruger.
/// Kører i BÅDE Development og Production (se Program.cs) — modsat de øvrige seedere,
/// der kun kører i Development. Idempotent: opretter aldrig en bruger, hvis SystemAdmin-
/// rollen allerede har mindst ét medlem.
///
/// Credentials læses fra konfiguration ("InitialAdmin:Email"/"InitialAdmin:Password") —
/// ALDRIG hardkodet. Brug 'dotnet user-secrets' lokalt og miljøvariabler/secret store i
/// produktion. Mangler konfiguration, springes brugerbootstrap over (rollen oprettes
/// stadig, så JIT-anmodninger til SystemAdmin er mulige), og der logges en advarsel.
/// </summary>
public sealed class IdentityBootstrapSeeder(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<InitialAdminOptions> options,
    ILogger<IdentityBootstrapSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureRoleExistsAsync(RoleNames.SystemAdmin);

        var existingMembers = await userManager.GetUsersInRoleAsync(RoleNames.SystemAdmin);
        if (existingMembers.Count > 0)
        {
            // Allerede bootstrappet — undgår at genoprette/nulstille admin ved genstart.
            return;
        }

        var adminOptions = options.Value;
        if (string.IsNullOrWhiteSpace(adminOptions.Email) || string.IsNullOrWhiteSpace(adminOptions.Password))
        {
            logger.LogWarning(
                "Ingen SystemAdmin findes, og InitialAdmin:Email/InitialAdmin:Password er ikke konfigureret. " +
                "Bootstrap af bruger springes over. Konfigurér User Secrets/miljøvariabler og genstart.");
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(adminOptions.Email);
        if (existingUser is not null)
        {
            await AddToSystemAdminRoleAsync(existingUser);
            logger.LogInformation("Eksisterende bruger '{Email}' tilføjet til SystemAdmin-rollen.", adminOptions.Email);
            return;
        }

        var user = new ApplicationUser
        {
            UserName = adminOptions.Email,
            Email = adminOptions.Email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, adminOptions.Password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Kunne ikke oprette initial SystemAdmin-bruger: {DescribeErrors(createResult)}");
        }

        await AddToSystemAdminRoleAsync(user);
        logger.LogInformation("Initial SystemAdmin-bruger '{Email}' oprettet og tildelt rollen.", adminOptions.Email);
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Kunne ikke oprette rollen '{roleName}': {DescribeErrors(result)}");
        }
    }

    private async Task AddToSystemAdminRoleAsync(ApplicationUser user)
    {
        var result = await userManager.AddToRoleAsync(user, RoleNames.SystemAdmin);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Kunne ikke tildele SystemAdmin-rollen til '{user.Email}': {DescribeErrors(result)}");
        }
    }

    private static string DescribeErrors(IdentityResult result) =>
        string.Join(", ", result.Errors.Select(e => e.Description));
}