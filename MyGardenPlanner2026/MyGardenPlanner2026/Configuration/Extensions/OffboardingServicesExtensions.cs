namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.Extensions.DependencyInjection.Extensions;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Infrastructure.Services.Account;

public static class OffboardingServicesExtensions
{
    public static IServiceCollection AddUserDeletionServices(this IServiceCollection services)
    {
        services.AddOptions<GardenRestoreOptions>()
            .BindConfiguration(GardenRestoreOptions.SectionName)
            .Validate(
                o => o.RetentionDays is >= 1 and <= 365,
                "GardenRestore:RetentionDays skal være mellem 1 og 365.")
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IAccountDeletionService, AccountDeletionService>();
        services.AddScoped<IGardenRestoreService, GardenRestoreService>();
        return services;
    }
}