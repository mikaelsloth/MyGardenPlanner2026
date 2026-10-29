namespace MyGardenPlanner2026.Configuration.Extensions;

using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Infrastructure.Services.Account;

public static class OffboardingServicesExtensions
{
    public static IServiceCollection AddUserDeletionServices(this IServiceCollection services)
    {
        services.AddScoped<IAccountDeletionService, AccountDeletionService>();
        return services;
    }
}
