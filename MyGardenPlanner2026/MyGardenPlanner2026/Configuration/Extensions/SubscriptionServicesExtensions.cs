namespace MyGardenPlanner2026.Configuration.Extensions;

using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using MyGardenPlanner2026.Infrastructure.Services;

public static class SubscriptionServicesExtensions
{
    public static IServiceCollection AddSubscriptionCatalogServices(this IServiceCollection services)
    {
        services.AddSingleton<ISubscriptionTierCatalog, DefaultSubscriptionTierCatalog>();
        services.AddScoped<SubscriptionTierSeeder>();
        services.AddScoped<ISubscriptionPricingService, SubscriptionPricingService>();

        return services;
    }
}