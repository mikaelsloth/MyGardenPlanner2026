namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.Extensions.Configuration;
using MyGardenPlanner2026.Infrastructure.Data.Seed;

public static class IdentityBootstrapSeedExtensions
{
    public static IServiceCollection AddIdentityBootstrapSeeding(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<InitialAdminOptions>(configuration.GetSection(InitialAdminOptions.SectionName));
        services.AddScoped<IdentityBootstrapSeeder>();

        return services;
    }

    public static async Task SeedIdentityBootstrapAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IdentityBootstrapSeeder>();
        await seeder.SeedAsync();
    }
}