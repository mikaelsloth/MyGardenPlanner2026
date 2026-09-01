namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Subscriptions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using NSubstitute;
using Xunit;

public class PricingCalculatorTests : BunitContext
{
    private static readonly Guid BedforslagAddOnId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ArtefaktpakkeAAddOnId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly SubscriptionAddOnDto BedforslagAddOn = new(
        Id: BedforslagAddOnId,
        Type: AddOnType.BedforslagNiveau2,
        Name: "Bedforslag (Niveau 2)",
        UnitDescription: "Pakke med 2 bedforslag",
        AnnualPrice: 180m,
        MonthlyPrice: 15m,
        PerpetualPrice: 450m);

    private static readonly SubscriptionAddOnDto ArtefaktpakkeAAddOn = new(
        Id: ArtefaktpakkeAAddOnId,
        Type: AddOnType.ArtefaktpakkeA,
        Name: "Artefaktpakke A",
        UnitDescription: "+25 Planter / Materialer / Opgavelister",
        AnnualPrice: 48m,
        MonthlyPrice: 4m,
        PerpetualPrice: 120m);

    private IPricingCalculatorService RegisterFakes(PricingCalculationResultDto? resultToReturn = null)
    {
        var addOnService = Substitute.For<ISubscriptionAddOnService>();
        addOnService.GetAllAddOnsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SubscriptionAddOnDto>>(
                [BedforslagAddOn, ArtefaktpakkeAAddOn]));

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

        Services.AddSingleton(addOnService);
        Services.AddSingleton(calculatorService);

        return calculatorService;
    }

    [Fact]
    public void PricingCalculator_RendersAddOnInputs_WithDistinctIds()
    {
        RegisterFakes();

        var cut = Render<PricingCalculator>();

        cut.Markup.Should().Contain("Bedforslag (Niveau 2)");
        cut.Markup.Should().Contain("Artefaktpakke A");
        cut.Find($"#addon-{BedforslagAddOnId}").Should().NotBeNull();
        cut.Find($"#addon-{ArtefaktpakkeAAddOnId}").Should().NotBeNull();
    }

    [Fact]
    public void PricingCalculator_EnteringQuantityForOneAddOn_DoesNotAffectOtherAddOn()
    {
        RegisterFakes();
        var cut = Render<PricingCalculator>();

        cut.Find($"#addon-{BedforslagAddOnId}").Change("3");

        cut.Find($"#addon-{BedforslagAddOnId}").GetAttribute("value").Should().Be("3");
        cut.Find($"#addon-{ArtefaktpakkeAAddOnId}").GetAttribute("value").Should().Be("0");
    }

    [Fact]
    public void PricingCalculator_ClickingBeregn_DisplaysResultTotal()
    {
        RegisterFakes();

        var cut = Render<PricingCalculator>();
        cut.Find("button.btn-primary").Click();

        cut.Markup.Should().Contain("336,00 kr.");
    }

    [Fact]
    public void PricingCalculator_ClickingBeregnWithAddOnQuantity_SendsCorrectAddOnIdToService()
    {
        var calculatorService = RegisterFakes();
        var cut = Render<PricingCalculator>();

        cut.Find($"#addon-{BedforslagAddOnId}").Change("2");
        cut.Find("button.btn-primary").Click();

        _ = calculatorService.Received().CalculateAsync(
            Arg.Is<PricingCalculationRequestDto>(r =>
                r.AddOnQuantities.ContainsKey(BedforslagAddOnId) && r.AddOnQuantities[BedforslagAddOnId] == 2 &&
                r.AddOnQuantities.ContainsKey(ArtefaktpakkeAAddOnId) && r.AddOnQuantities[ArtefaktpakkeAAddOnId] == 0),
            Arg.Any<CancellationToken>());
    }
}