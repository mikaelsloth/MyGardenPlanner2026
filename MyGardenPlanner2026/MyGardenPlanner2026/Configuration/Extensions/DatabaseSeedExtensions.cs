namespace MyGardenPlanner2026.Configuration.Extensions;

using MyGardenPlanner2026.Infrastructure.Data.Seed;

public static class DatabaseSeedExtensions
{
    public static async Task AddDatabaseSeeds(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var tierSeeder = scope.ServiceProvider.GetRequiredService<SubscriptionTierSeeder>();
        await tierSeeder.SeedAsync();

        var volumeDiscountSeeder = scope.ServiceProvider.GetRequiredService<GardenVolumeDiscountSeeder>();
        await volumeDiscountSeeder.SeedAsync();

        var addOnSeeder = scope.ServiceProvider.GetRequiredService<SubscriptionAddOnSeeder>();
        await addOnSeeder.SeedAsync();
    }
}
