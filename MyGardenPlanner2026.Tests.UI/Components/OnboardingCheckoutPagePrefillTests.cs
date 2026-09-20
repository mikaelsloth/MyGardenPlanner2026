namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Pages;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using NSubstitute;
using Xunit;

/// <summary>
/// Dækker query-string-prefill (level/category/cycle) tilføjet i PR 4E. Egen fil for
/// ikke at kollidere med den eksisterende OnboardingCheckoutPageTests.cs.
/// </summary>
public sealed class OnboardingCheckoutPagePrefillTests : BunitContext
{
    private readonly IOnboardingService onboardingService = Substitute.For<IOnboardingService>();
    private readonly IGardenAccessQueryService queryService = Substitute.For<IGardenAccessQueryService>();
    private readonly IPricingCalculatorService calculatorService = Substitute.For<IPricingCalculatorService>();
    private readonly ISubscriptionAddOnService addOnService = Substitute.For<ISubscriptionAddOnService>();

    public OnboardingCheckoutPagePrefillTests()
    {
        addOnService.GetAllAddOnsAsync(Arg.Any<CancellationToken>()).Returns([]);
        Services.AddSingleton(onboardingService);
        Services.AddSingleton(queryService);
        Services.AddSingleton(calculatorService);
        Services.AddSingleton(addOnService);

        AddAuthorization().SetNotAuthorized();
    }

    private static PricingCalculationResultDto CreateResult() => new(100m, 1m, 1.0m, 100m, [], 0m, 100m);

    [Fact]
    public async Task LevelAndCategoryQueryParams_PrefillPricingCalculator_WithoutTouchingDropdowns()
    {
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/onboarding/checkout?level=BedDesigner&category=Editor");

        var cut = Render<OnboardingCheckoutPage>();
        await cut.Find("#garden-name").ChangeAsync("Min have");
        await cut.Find(".btn-accent").ClickAsync();

        await calculatorService.Received(1).CalculateAsync(
            Arg.Is<PricingCalculationRequestDto>(r =>
                r.Level == GardenAccessLevel.BedDesigner && r.AccessCategory == AccessCategory.Editor),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvalidLevelQueryParam_FallsBackToDefaultSelection_WithoutThrowing()
    {
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/onboarding/checkout?level=NotARealLevel");

        var cut = Render<OnboardingCheckoutPage>();
        await cut.Find("#garden-name").ChangeAsync("Min have");
        await cut.Find(".btn-accent").ClickAsync();

        await calculatorService.Received(1).CalculateAsync(
            Arg.Is<PricingCalculationRequestDto>(r => r.Level == GardenAccessLevel.HaveArkitekt),
            Arg.Any<CancellationToken>());
    }
}