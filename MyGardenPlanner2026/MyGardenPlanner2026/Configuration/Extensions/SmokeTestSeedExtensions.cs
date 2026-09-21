namespace MyGardenPlanner2026.Configuration.Extensions;

using MyGardenPlanner2026.Infrastructure.Data.Seed;

public static class SmokeTestSeedExtensions
{
    public static IServiceCollection AddSmokeTestUserSeeding(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmokeTestUsersOptions>(configuration.GetSection(SmokeTestUsersOptions.SectionName));
        services.AddScoped<SmokeTestUserSeeder>();

        return services;
    }

    public static async Task SeedSmokeTestUsersAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<SmokeTestUserSeeder>();
        await seeder.SeedAsync();
    }
}