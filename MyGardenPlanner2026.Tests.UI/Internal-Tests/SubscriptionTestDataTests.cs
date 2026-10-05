namespace MyGardenPlanner2026.Tests.UI.Internal_Tests;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using Xunit;

public sealed class SubscriptionTestDataTests
{
    [Fact]
    public void PricingResult_Defaults_AreOneGardenWithoutDiscountOrAddOns()
    {
        var result = SubscriptionTestData.PricingResult();

        result.BasePricePerGarden.Should().Be(100m);
        result.WeightedGardenCount.Should().Be(1m);
        result.DiscountMultiplier.Should().Be(1.0m);
        result.GardenSubtotal.Should().Be(100m);
        result.AddOnLineItems.Should().BeEmpty();
        result.AddOnsTotal.Should().Be(0m);
        result.Total.Should().Be(100m);
    }

    [Fact]
    public void PricingResult_Overrides_AreApplied()
    {
        var result = SubscriptionTestData.PricingResult(total: 336m, addOnsTotal: 50m, basePricePerGarden: 336m);

        result.Total.Should().Be(336m);
        result.AddOnsTotal.Should().Be(50m);
        result.BasePricePerGarden.Should().Be(336m);
        result.GardenSubtotal.Should().Be(336m);
    }

    [Fact]
    public void BedforslagAddOn_HasStandardValues_AndGivenId()
    {
        var id = Guid.NewGuid();

        var addOn = SubscriptionTestData.BedforslagAddOn(id);

        addOn.Id.Should().Be(id);
        addOn.Type.Should().Be(AddOnType.BedforslagNiveau2);
        addOn.Name.Should().Be("Bedforslag (Niveau 2)");
        addOn.UnitDescription.Should().Be("Pakke med 2 bedforslag");
        addOn.AnnualPrice.Should().Be(180m);
        addOn.MonthlyPrice.Should().Be(15m);
        addOn.PerpetualPrice.Should().Be(450m);
    }

    [Fact]
    public void ArtefaktpakkeAAddOn_HasStandardValues()
    {
        var addOn = SubscriptionTestData.ArtefaktpakkeAAddOn();

        addOn.Type.Should().Be(AddOnType.ArtefaktpakkeA);
        addOn.Name.Should().Be("Artefaktpakke A");
        addOn.UnitDescription.Should().Be("+25 Planter / Materialer / Opgavelister");
        addOn.AnnualPrice.Should().Be(48m);
        addOn.MonthlyPrice.Should().Be(4m);
        addOn.PerpetualPrice.Should().Be(120m);
    }

    [Fact]
    public void AddOns_WithoutId_GetDistinctIds()
    {
        SubscriptionTestData.BedforslagAddOn().Id.Should().NotBe(SubscriptionTestData.BedforslagAddOn().Id);
    }

    [Fact]
    public void Tier_Defaults_AreAnnualBedDesignerEditorWithoutFeatures()
    {
        var tier = SubscriptionTestData.Tier();

        tier.Level.Should().Be(GardenAccessLevel.BedDesigner);
        tier.AccessCategory.Should().Be(AccessCategory.Editor);
        tier.Name.Should().Be($"{GardenAccessLevel.BedDesigner} · {AccessCategory.Editor}");
        tier.Description.Should().Be("Beskrivelse");
        tier.Price.Should().Be(100m);
        tier.BillingCycle.Should().Be(BillingCycle.Annual);
        tier.IsFeatured.Should().BeFalse();
        tier.IncludedFeatures.Should().BeEmpty();
        tier.FeatureLimits.Should().BeEmpty();
    }

    [Fact]
    public void Tier_Overrides_AreApplied()
    {
        var id = Guid.NewGuid();

        var tier = SubscriptionTestData.Tier(
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer, 96m, isFeatured: true,
            name: "Planlægger · Læser", description: "Tekst", billingCycle: BillingCycle.Monthly, id: id);

        tier.Id.Should().Be(id);
        tier.Level.Should().Be(GardenAccessLevel.Planlaegger);
        tier.AccessCategory.Should().Be(AccessCategory.Viewer);
        tier.Price.Should().Be(96m);
        tier.IsFeatured.Should().BeTrue();
        tier.Name.Should().Be("Planlægger · Læser");
        tier.Description.Should().Be("Tekst");
        tier.BillingCycle.Should().Be(BillingCycle.Monthly);
    }

    [Fact]
    public void Tier_WithExpression_CanSetFeatureLists()
    {
        var tier = SubscriptionTestData.Tier() with
        {
            IncludedFeatures = ["Feature"],
            FeatureLimits = new Dictionary<string, string> { ["Bedforslag"] = "2" }
        };

        tier.IncludedFeatures.Should().ContainSingle().Which.Should().Be("Feature");
        tier.FeatureLimits.Should().ContainKey("Bedforslag").WhoseValue.Should().Be("2");
    }
}