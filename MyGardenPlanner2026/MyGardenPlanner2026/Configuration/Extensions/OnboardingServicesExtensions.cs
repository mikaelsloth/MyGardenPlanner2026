namespace MyGardenPlanner2026.Configuration.Extensions;

using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Infrastructure.Services.Onboarding;
using MyGardenPlanner2026.Services;

/// <summary>
/// Registrerer onboarding-relaterede services. IOnboardingService registreres bevidst
/// IKKE her endnu — implementeringen (der læser/skriver Garden/GardenMembership/
/// UserEntitlement/GardenInvitation via EF Core) kommer i en senere PR.
/// </summary>
public static class OnboardingServicesExtensions
{
    public static IServiceCollection AddOnboardingServices(this IServiceCollection services)
    {
        services.AddSingleton<IInvitationTokenService, InvitationTokenService>();
        services.AddScoped<OnboardingStateContainer>();

        return services;
    }
}