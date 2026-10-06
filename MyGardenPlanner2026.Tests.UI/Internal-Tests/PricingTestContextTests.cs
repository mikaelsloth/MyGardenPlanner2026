namespace MyGardenPlanner2026.Tests.UI.Internal_Tests;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using Xunit;

public sealed class PricingTestContextTests : PricingTestContext
{
    [Fact]
    public void RegistersCalculatorAndAddOnFakesInServices()
    {
        Services.GetRequiredService<IPricingCalculatorService>().Should().BeSameAs(calculatorService);
        Services.GetRequiredService<ISubscriptionAddOnService>().Should().BeSameAs(addOnService);
    }

    [Fact]
    public async Task AddOnService_DefaultsToEmptyList()
    {
        var addOns = await addOnService.GetAllAddOnsAsync(TestContext.Current.CancellationToken);

        addOns.Should().BeEmpty();
    }
}