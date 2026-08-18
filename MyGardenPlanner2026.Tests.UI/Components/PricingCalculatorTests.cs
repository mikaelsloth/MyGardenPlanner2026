namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Subscriptions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using NSubstitute;
using Xunit;

public class PricingCalculatorTests : BunitContext
{
    private void RegisterFakes(PricingCalculationResultDto? resultToReturn = null)
    {
        var addOnCatalog = Substitute.For<ISubscriptionAddOnCatalog>();
        addOnCatalog.GetDefaultAddOns().Returns(
        [
            new SubscriptionAddOn
            {
                Id = 1,
                Type = AddOnType.BedforslagNiveau2,
                Name = "Bedforslag (Niveau 2)",
                UnitDescription = "Pakke med 2 bedforslag",
                AnnualPrice = 180m,
                MonthlyPrice = 15m,
                PerpetualPrice = 450m,
                DisplayOrder = 1
            }
        ]);

        var calculatorService = Substitute.For<IPricingCalculatorService>();
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(resultToReturn ?? new PricingCalculationResultDto(
                BasePricePerGarden: 336m,
                WeightedGardenCount: 1m,
                DiscountMultiplier: 1.00m,
                GardenSubtotal: 336m,
                AddOnLineItems: [],
                AddOnsTotal: 0m,
                Total: 336m)));

        Services.AddSingleton(addOnCatalog);
        Services.AddSingleton(calculatorService);
    }

    [Fact]
    public void PricingCalculator_BillingCycleSelect_IncludesAllThreeCycles()
    {
        RegisterFakes();

        var cut = Render<PricingCalculator>();
        var options = cut.FindAll("#calc-cycle option");

        options.Should().HaveCount(3);
        options.Select(o => o.TextContent).Should().Contain(["Årligt", "Månedligt", "Engangsbeløb"]);
    }

    [Fact]
    public void PricingCalculator_RendersAddOnInputs()
    {
        RegisterFakes();

        var cut = Render<PricingCalculator>();

        cut.Markup.Should().Contain("Bedforslag (Niveau 2)");
    }

    [Fact]
    public void PricingCalculator_ClickingBeregn_DisplaysResultTotal()
    {
        RegisterFakes();

        var cut = Render<PricingCalculator>();
        cut.Find("button.btn-primary").Click();

        cut.Markup.Should().Contain("336,00 kr.");
    }
}