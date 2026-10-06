namespace MyGardenPlanner2026.Tests.UI;

using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using NSubstitute;

/// <summary>
/// Fælles bUnit-kontekst for onboarding-/invitationssider, der både bruger prisberegneren og
/// onboarding-/adgangsservices. Tilføjer fakes for IOnboardingService og IGardenAccessQueryService.
/// </summary>
public abstract class OnboardingPricingTestContext : PricingTestContext
{
    protected readonly IOnboardingService onboardingService = Substitute.For<IOnboardingService>();
    protected readonly IGardenAccessQueryService queryService = Substitute.For<IGardenAccessQueryService>();

    protected OnboardingPricingTestContext()
    {
        Services.AddSingleton(onboardingService);
        Services.AddSingleton(queryService);
    }
}