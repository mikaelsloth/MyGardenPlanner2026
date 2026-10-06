namespace MyGardenPlanner2026.Tests.UI.Components.Pages.Public;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Pages;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using NSubstitute;
using Xunit;

/// <summary>Ny fil — PricingPage havde ingen tests fra før.</summary>
public sealed class PricingPageTests : PricingTestContext
{
    private readonly ISubscriptionPricingService pricingService = Substitute.For<ISubscriptionPricingService>();
    private readonly IGardenVolumeDiscountCatalog volumeDiscountCatalog = Substitute.For<IGardenVolumeDiscountCatalog>();

    public PricingPageTests()
    {
        pricingService.GetAllTiersAsync(Arg.Any<BillingCycle>(), Arg.Any<CancellationToken>()).Returns([]);
        volumeDiscountCatalog.GetDefaultTiers().Returns([]);
        Services.AddSingleton(pricingService);
        Services.AddSingleton(volumeDiscountCatalog);
    }

    [Fact]
    public void ClickingContinueOnCalculator_NavigatesToOnboardingCheckout_WithLevelCategoryAndCycle()
    {
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(SubscriptionTestData.PricingResult());

        var cut = Render<PricingPage>();
        cut.Find(".btn-accent").Click();

        var uri = this.CurrentUri();
        uri.Should().Contain("/onboarding/checkout");
        uri.Should().Contain($"level={GardenAccessLevel.HaveArkitekt}");
        uri.Should().Contain($"category={AccessCategory.Editor}");
        uri.Should().Contain($"cycle={BillingCycle.Annual}");
    }
}