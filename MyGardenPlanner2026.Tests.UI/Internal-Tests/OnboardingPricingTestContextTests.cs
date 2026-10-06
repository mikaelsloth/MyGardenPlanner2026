namespace MyGardenPlanner2026.Tests.UI.Internal_Tests;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using Xunit;

public sealed class OnboardingPricingTestContextTests : OnboardingPricingTestContext
{
    [Fact]
    public void RegistersOnboardingQueryAndPricingFakesInServices()
    {
        Services.GetRequiredService<IOnboardingService>().Should().BeSameAs(onboardingService);
        Services.GetRequiredService<IGardenAccessQueryService>().Should().BeSameAs(queryService);
        Services.GetRequiredService<IPricingCalculatorService>().Should().BeSameAs(calculatorService);
    }
}