namespace MyGardenPlanner2026.Tests.Unit.Services.Onboarding;

using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Services.Onboarding;
using Xunit;

/// <summary>
/// Basisklasse for tests af onboarding- og adgangsservices: fast tidspunkt, service-factory
/// og seeding af have, medlemskab og entitlement med fornuftige standardværdier.
/// </summary>
public abstract class OnboardingTestDbContext : TestDbContext
{
    protected static readonly DateTimeOffset FixedNow = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    protected GardenAccessQueryService CreateAccessQueryService(TimeProvider? timeProvider = null) =>
        new(CreateDbContextFactory(), timeProvider ?? new TestTimeProvider(FixedNow));

    protected async Task<Garden> SeedGardenAsync(string name = "Testhave", bool archived = false)
    {
        var garden = new Garden { Name = name, Archived = archived };

        await using var context = CreateDbContext();
        await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return garden;
    }

    protected async Task<GardenMembership> SeedMembershipAsync(
        Guid gardenId, string userId, GardenAccessLevel layer, AccessCategory category, bool isOwner = false)
    {
        var membership = new GardenMembership
        {
            GardenId = gardenId,
            UserId = userId,
            IsOwner = isOwner,
            Layer = layer,
            Category = category
        };

        await using var context = CreateDbContext();
        await context.GardenMemberships.AddAsync(membership, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return membership;
    }

    protected async Task<UserEntitlement> SeedEntitlementAsync(
        Guid gardenId, string userId, GardenAccessLevel layer, AccessCategory category,
        BillingCycle billingCycle, bool isTrial = false, DateTimeOffset? validToUtc = null)
    {
        var entitlement = new UserEntitlement
        {
            UserId = userId,
            GardenId = gardenId,
            Layer = layer,
            Category = category,
            BillingCycle = billingCycle,
            IsTrial = isTrial,
            ValidToUtc = validToUtc
        };

        await using var context = CreateDbContext();
        await context.UserEntitlements.AddAsync(entitlement, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return entitlement;
    }
}