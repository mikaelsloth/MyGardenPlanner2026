namespace MyGardenPlanner2026.Tests.UI.Components.Pages.Onboarding;

using Bunit;
using MyGardenPlanner2026.Components.Pages;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using NSubstitute;
using Xunit;

/// <summary>
/// Dækker query-string-prefill (level/category/cycle) tilføjet i PR 4E. Egen fil for
/// ikke at kollidere med den eksisterende OnboardingCheckoutPageTests.cs.
/// </summary>
public sealed class OnboardingCheckoutPagePrefillTests : OnboardingPricingTestContext
{
    public OnboardingCheckoutPagePrefillTests()
    {
        AddAuthorization().SetNotAuthorized();
    }

    [Fact]
    public async Task LevelAndCategoryQueryParams_PrefillPricingCalculator_WithoutTouchingDropdowns()
    {
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(SubscriptionTestData.PricingResult());

        var cut = this.RenderAt<OnboardingCheckoutPage>("/onboarding/checkout?level=BedDesigner&category=Editor");
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
            .Returns(SubscriptionTestData.PricingResult());

        var cut = this.RenderAt<OnboardingCheckoutPage>("/onboarding/checkout?level=NotARealLevel");
        await cut.Find("#garden-name").ChangeAsync("Min have");
        await cut.Find(".btn-accent").ClickAsync();

        await calculatorService.Received(1).CalculateAsync(
            Arg.Is<PricingCalculationRequestDto>(r => r.Level == GardenAccessLevel.HaveArkitekt),
            Arg.Any<CancellationToken>());
    }
}