namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>Ét medlem i en seedet have. Brug <see cref="Owner"/> og <see cref="Member"/>.</summary>
public sealed record GardenMemberSeed(
    string Email, bool IsOwner, GardenAccessLevel Layer, AccessCategory Category, bool WithEntitlement)
{
    /// <summary>Ejer: Administrator med entitlement (som en betalt have).</summary>
    public static GardenMemberSeed Owner(string email, GardenAccessLevel layer = GardenAccessLevel.HaveArkitekt) =>
        new(email, IsOwner: true, layer, AccessCategory.Administrator, WithEntitlement: true);

    public static GardenMemberSeed Member(
        string email,
        AccessCategory category = AccessCategory.Editor,
        GardenAccessLevel layer = GardenAccessLevel.HaveArkitekt,
        bool withEntitlement = false) =>
        new(email, IsOwner: false, layer, category, withEntitlement);
}

public sealed record GardenSeed(string Name, IReadOnlyList<GardenMemberSeed> Members);

public sealed record SeededGarden(Guid Id, string Name);

public sealed record GardenSnapshot(
    Guid Id, string Name, bool IsDeleted, DateTimeOffset? DeletedAtUtc, string? DeletedByUserId, string? DeletedByEmail);

public sealed record MembershipSnapshot(
    string UserId, bool IsOwner, GardenAccessLevel Layer, AccessCategory Category, bool IsDeleted);

public sealed record EntitlementSnapshot(
    string UserId, GardenAccessLevel Layer, AccessCategory Category, BillingCycle BillingCycle, bool IsTrial, bool IsDeleted);

/// <summary>
/// Seeder og læser af haver i engangsdatabasen (R10.6). Haver oprettes direkte i dbo via app-forbindelsen,
/// uden om appens interceptors og UI. Brugerne skal findes (se <c>PlaywrightAppFixture.SeedExtraUserAsync</c>).
/// Læsere bruger IgnoreQueryFilters, så soft-slettede rækker kan verificeres. Brug en ny have og nye brugere
/// pr. test: databasen deles på tværs af tests.
/// </summary>
public static class GardenTestData
{
    public static async Task<SeededGarden> SeedAsync(string connectionString, GardenSeed seed)
    {
        Validate(seed);

        await using var provider = SmokeTestDataSeeder.BuildServiceProvider(connectionString);
        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = scope.ServiceProvider.GetRequiredService<PlannerDbContext>();

        var garden = new Garden { Name = seed.Name };

        foreach (var member in seed.Members)
        {
            var user = await userManager.FindByEmailAsync(member.Email)
                ?? throw new InvalidOperationException($"Bruger '{member.Email}' findes ikke.");

            garden.Members.Add(new GardenMembership
            {
                UserId = user.Id,
                IsOwner = member.IsOwner,
                Layer = member.Layer,
                Category = member.Category,
            });

            if (member.WithEntitlement)
            {
                context.UserEntitlements.Add(new UserEntitlement
                {
                    UserId = user.Id,
                    GardenId = garden.Id,
                    Layer = member.Layer,
                    Category = member.Category,
                    BillingCycle = BillingCycle.Annual,
                    ValidToUtc = DateTimeOffset.UtcNow.AddYears(1),
                    IsTrial = false,
                });
            }
        }

        context.Gardens.Add(garden);
        await context.SaveChangesAsync();

        return new SeededGarden(garden.Id, garden.Name);
    }

    /// <summary>Null, hvis haven ikke findes (heller ikke soft-slettet).</summary>
    public static Task<GardenSnapshot?> FindGardenAsync(string connectionString, Guid gardenId) =>
        ReadAsync(connectionString, context => context.Gardens
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(g => g.Id == gardenId)
            .Select(g => new GardenSnapshot(g.Id, g.Name, g.IsDeleted, g.DeletedAtUtc, g.DeletedByUserId, g.DeletedByEmail))
            .SingleOrDefaultAsync());

    public static Task<List<MembershipSnapshot>> GetMembershipsAsync(string connectionString, Guid gardenId) =>
        ReadAsync(connectionString, context => context.GardenMemberships
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(m => m.GardenId == gardenId)
            .OrderBy(m => m.UserId)
            .Select(m => new MembershipSnapshot(m.UserId, m.IsOwner, m.Layer, m.Category, m.IsDeleted))
            .ToListAsync());

    public static Task<List<EntitlementSnapshot>> GetEntitlementsAsync(string connectionString, Guid gardenId) =>
        ReadAsync(connectionString, context => context.UserEntitlements
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(e => e.GardenId == gardenId)
            .OrderBy(e => e.UserId)
            .Select(e => new EntitlementSnapshot(e.UserId, e.Layer, e.Category, e.BillingCycle, e.IsTrial, e.IsDeleted))
            .ToListAsync());

    private static async Task<T> ReadAsync<T>(string connectionString, Func<PlannerDbContext, Task<T>> read)
    {
        await using var provider = SmokeTestDataSeeder.BuildServiceProvider(connectionString);
        using var scope = provider.CreateScope();

        return await read(scope.ServiceProvider.GetRequiredService<PlannerDbContext>());
    }

    private static void Validate(GardenSeed seed)
    {
        ArgumentNullException.ThrowIfNull(seed);

        if (string.IsNullOrWhiteSpace(seed.Name))
        {
            throw new ArgumentException("Havens navn må ikke være tomt.", nameof(seed));
        }

        if (seed.Members.Count == 0)
        {
            throw new ArgumentException("En have skal have mindst ét medlem.", nameof(seed));
        }

        if (seed.Members.Count(m => m.IsOwner) != 1)
        {
            throw new ArgumentException("Præcis ét medlem skal være ejer.", nameof(seed));
        }

        if (seed.Members.Select(m => m.Email).Distinct(StringComparer.OrdinalIgnoreCase).Count() != seed.Members.Count)
        {
            throw new ArgumentException("Et medlem må kun optræde én gang i en have.", nameof(seed));
        }
    }
}