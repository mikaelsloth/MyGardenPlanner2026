namespace MyGardenPlanner2026.Tests.Unit.Services.Layer1;

using MyGardenPlanner2026.Infrastructure.Data.Seed;
using Xunit;

/// <summary>
/// Basisklasse for tests af Layer1-services, der kræver seedede kataloger
/// (abonnementsniveauer, rabattrapper og tilkøb).
/// </summary>
public abstract class CatalogTestDbContext : TestDbContext
{
    protected async Task SeedSubscriptionTiersAsync()
    {
        var seeder = new SubscriptionTierSeeder(CreateAdminDbContextFactory(), new DefaultSubscriptionTierCatalog());
        await seeder.SeedAsync(TestContext.Current.CancellationToken);
    }

    protected async Task SeedGardenVolumeDiscountsAsync()
    {
        var seeder = new GardenVolumeDiscountSeeder(CreateAdminDbContextFactory(), new DefaultGardenVolumeDiscountCatalog());
        await seeder.SeedAsync(TestContext.Current.CancellationToken);
    }

    protected async Task SeedSubscriptionAddOnsAsync()
    {
        var seeder = new SubscriptionAddOnSeeder(CreateAdminDbContextFactory(), new DefaultSubscriptionAddOnCatalog());
        await seeder.SeedAsync(TestContext.Current.CancellationToken);
    }

    protected async Task SeedAllCatalogsAsync()
    {
        await SeedSubscriptionTiersAsync();
        await SeedGardenVolumeDiscountsAsync();
        await SeedSubscriptionAddOnsAsync();
    }
}