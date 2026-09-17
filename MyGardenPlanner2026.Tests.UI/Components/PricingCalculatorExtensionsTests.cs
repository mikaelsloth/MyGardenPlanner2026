namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Subscriptions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

/// <summary>
/// Dækker udvidelserne fra Prompt 3 (Fixed*/Allowed*/Initial*/ContinueButtonLabel/OnContinue).
/// Egen fil for ikke at kollidere med den eksisterende PricingCalculatorTests.cs.
/// </summary>
public sealed class PricingCalculatorExtensionsTests : BunitContext
{
    private readonly ISubscriptionAddOnService addOnService = Substitute.For<ISubscriptionAddOnService>();
    private readonly IPricingCalculatorService calculatorService = Substitute.For<IPricingCalculatorService>();

    public PricingCalculatorExtensionsTests()
    {
        addOnService.GetAllAddOnsAsync(Arg.Any<CancellationToken>()).Returns([]);
        Services.AddSingleton(addOnService);
        Services.AddSingleton(calculatorService);
    }

    private static PricingCalculationResultDto CreateResult() =>
        new(100m, 1m, 1.0m, 100m, [], 0m, 100m);

    [Fact]
    public void FixedActiveGardens_Null_StillRendersManualInput()
    {
        var cut = Render<PricingCalculator>();

        cut.FindAll("#calc-active").Should().HaveCount(1);
    }

    [Fact]
    public void FixedActiveGardens_Set_HidesManualInput_AndShowsLockedValue()
    {
        var cut = Render<PricingCalculator>(p => p.Add(c => c.FixedActiveGardens, 3));

        cut.FindAll("#calc-active").Should().BeEmpty();
        cut.Markup.Should().Contain("3");
        cut.Markup.Should().Contain("låst");
    }

    [Fact]
    public void FixedArchivedGardens_Set_HidesManualInput()
    {
        var cut = Render<PricingCalculator>(p => p.Add(c => c.FixedArchivedGardens, 2));

        cut.FindAll("#calc-archived").Should().BeEmpty();
    }

    [Fact]
    public void AllowedLevels_RestrictsDropdownOptions()
    {
        var cut = Render<PricingCalculator>(p => p
            .Add(c => c.AllowedLevels, [GardenAccessLevel.Planlaegger]));

        var options = cut.FindAll("#calc-level option");

        options.Should().HaveCount(1);
        options[0].GetAttribute("value").Should().Be(nameof(GardenAccessLevel.Planlaegger));
    }

    [Fact]
    public async Task InitialLevel_UsedAsDefaultSelection_WhenContinueClickedWithoutChangingDropdown()
    {
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        var cut = Render<PricingCalculator>(p => p
            .Add(c => c.ContinueButtonLabel, "Fortsæt")
            .Add(c => c.InitialLevel, GardenAccessLevel.BedDesigner));

        await cut.Find(".btn-accent").ClickAsync();

        await calculatorService.Received(1).CalculateAsync(
            Arg.Is<PricingCalculationRequestDto>(r => r.Level == GardenAccessLevel.BedDesigner),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ContinueButtonLabel_NotSet_DoesNotRenderContinueButton()
    {
        var cut = Render<PricingCalculator>();

        cut.FindAll(".btn-accent").Should().BeEmpty();
    }

    [Fact]
    public void ContinueButtonLabel_Set_RendersButtonWithLabel()
    {
        var cut = Render<PricingCalculator>(p => p.Add(c => c.ContinueButtonLabel, "Fortsæt til login"));

        cut.Find(".btn-accent").TextContent.Should().Be("Fortsæt til login");
    }

    [Fact]
    public void ClickingContinue_CalculationSucceeds_InvokesOnContinueWithSelection()
    {
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        PricingSelectionDto? invoked = null;
        var cut = Render<PricingCalculator>(p => p
            .Add(c => c.ContinueButtonLabel, "Fortsæt")
            .Add(c => c.OnContinue, s => invoked = s));

        cut.Find(".btn-accent").Click();

        invoked.Should().NotBeNull();
        invoked!.PriceResult.Total.Should().Be(100m);
    }

    [Fact]
    public void ClickingContinue_CalculationFails_DoesNotInvokeOnContinue_AndShowsError()
    {
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Intet abonnement fundet."));

        var invoked = false;
        var cut = Render<PricingCalculator>(p => p
            .Add(c => c.ContinueButtonLabel, "Fortsæt")
            .Add(c => c.OnContinue, _ => invoked = true));

        cut.Find(".btn-accent").Click();

        invoked.Should().BeFalse();
        cut.Find(".status-message.status-danger").Should().NotBeNull();
    }

    [Fact]
    public async Task ClickingContinue_UsesFixedGardenCounts_InCalculationRequest()
    {
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        var cut = Render<PricingCalculator>(p => p
            .Add(c => c.ContinueButtonLabel, "Fortsæt")
            .Add(c => c.FixedActiveGardens, 1)
            .Add(c => c.FixedArchivedGardens, 0));

        await cut.Find(".btn-accent").ClickAsync();

        await calculatorService.Received(1).CalculateAsync(
            Arg.Is<PricingCalculationRequestDto>(r => r.ActiveGardens == 1 && r.ArchivedGardens == 0),
            Arg.Any<CancellationToken>());
    }
}