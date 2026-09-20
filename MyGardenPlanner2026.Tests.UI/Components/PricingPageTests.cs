namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Pages;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using NSubstitute;
using Xunit;

/// <summary>Ny fil — PricingPage havde ingen tests fra før.</summary>
public sealed class PricingPageTests : BunitContext
{
    private readonly ISubscriptionAddOnService addOnService = Substitute.For<ISubscriptionAddOnService>();
    private readonly ISubscriptionPricingService pricingService = Substitute.For<ISubscriptionPricingService>();
    private readonly IGardenVolumeDiscountCatalog volumeDiscountCatalog = Substitute.For<IGardenVolumeDiscountCatalog>();
    private readonly IPricingCalculatorService calculatorService = Substitute.For<IPricingCalculatorService>();

    public PricingPageTests()
    {
        addOnService.GetAllAddOnsAsync(Arg.Any<CancellationToken>()).Returns([]);
        pricingService.GetAllTiersAsync(Arg.Any<BillingCycle>(), Arg.Any<CancellationToken>()).Returns([]);
        volumeDiscountCatalog.GetDefaultTiers().Returns([]);
        Services.AddSingleton(addOnService);
        Services.AddSingleton(pricingService);
        Services.AddSingleton(volumeDiscountCatalog);
        Services.AddSingleton(calculatorService);
    }

    [Fact]
    public void ClickingContinueOnCalculator_NavigatesToOnboardingCheckout_WithLevelCategoryAndCycle()
    {
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(new PricingCalculationResultDto(100m, 1m, 1.0m, 100m, [], 0m, 100m));

        var cut = Render<PricingPage>();
        cut.Find(".btn-accent").Click();

        var uri = Services.GetRequiredService<NavigationManager>().Uri;
        uri.Should().Contain("/onboarding/checkout");
        uri.Should().Contain($"level={GardenAccessLevel.HaveArkitekt}");
        uri.Should().Contain($"category={AccessCategory.Editor}");
        uri.Should().Contain($"cycle={BillingCycle.Annual}");
    }
}