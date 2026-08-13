namespace MyGardenPlanner2026.Configuration.Extensions;

using MyGardenPlanner2026.Infrastructure.Data.Seed;

public static class DatabaseSeedExtensions
{
    public static async Task AddDatabaseSeeds(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<SubscriptionTierSeeder>();
        await seeder.SeedAsync();
    }
}
