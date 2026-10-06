namespace MyGardenPlanner2026.Tests.UI;

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using NSubstitute;

/// <summary>
/// Fælles bUnit-kontekst for sider og komponenter, der bruger prisberegneren (PricingCalculator).
/// Registrerer fakes for ISubscriptionAddOnService (tom tilkøbsliste) og IPricingCalculatorService.
/// </summary>
public abstract class PricingTestContext : BunitContext
{
    protected readonly ISubscriptionAddOnService addOnService = Substitute.For<ISubscriptionAddOnService>();
    protected readonly IPricingCalculatorService calculatorService = Substitute.For<IPricingCalculatorService>();

    protected PricingTestContext()
    {
        addOnService.GetAllAddOnsAsync(Arg.Any<CancellationToken>()).Returns([]);
        Services.AddSingleton(addOnService);
        Services.AddSingleton(calculatorService);
    }
}