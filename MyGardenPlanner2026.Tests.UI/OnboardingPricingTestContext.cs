namespace MyGardenPlanner2026.Tests.UI;

using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using NSubstitute;

/// <summary>
/// Fælles bUnit-kontekst for onboarding-/invitationssider, der både bruger prisberegneren og
/// onboarding-/adgangsservices. Tilføjer fakes for IOnboardingService, IGardenAccessQueryService
/// og IGardenRestoreService (standard: ingen gendannelige haver).
/// </summary>
public abstract class OnboardingPricingTestContext : PricingTestContext
{
    protected readonly IOnboardingService onboardingService = Substitute.For<IOnboardingService>();
    protected readonly IGardenAccessQueryService queryService = Substitute.For<IGardenAccessQueryService>();
    protected readonly IGardenRestoreService restoreService = Substitute.For<IGardenRestoreService>();

    protected OnboardingPricingTestContext()
    {
        restoreService.GetRestorableGardensAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RestorableGardenDto>>([]));

        Services.AddSingleton(onboardingService);
        Services.AddSingleton(queryService);
        Services.AddSingleton(restoreService);
    }
}