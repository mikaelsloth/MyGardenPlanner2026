namespace MyGardenPlanner2026.Tests.UI.Components.Subscriptions;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Domain.Subscriptions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public class PricingFeatureMatrixTests : BunitContext
{
    private static List<SubscriptionTierDto> CreateTiers() =>
    [
        SubscriptionTestData.Tier(GardenAccessLevel.BedDesigner, price: 120m, name: "Bed Designer · Editor") with
        {
            IncludedFeatures = ["2 bedforslag pr. have"],
            FeatureLimits = new Dictionary<string, string> { ["Bedforslag"] = "2", ["Bede pr. forslag"] = "25" }
        },
        SubscriptionTestData.Tier(GardenAccessLevel.Planlaegger, price: 96m, name: "Planlægger · Editor") with
        {
            IncludedFeatures = ["50 planlagte bede pr. have"],
            FeatureLimits = new Dictionary<string, string> { ["Planlagte bede"] = "50" }
        }
    ];

    [Fact]
    public void PricingFeatureMatrix_RendersOneColumnPerTier()
    {
        var cut = Render<PricingFeatureMatrix>(p => p.Add(x => x.Tiers, CreateTiers()));

        cut.FindAll("table thead th").Should().HaveCount(3);
        cut.Markup.Should().Contain("Bed Designer · Editor");
        cut.Markup.Should().Contain("Planlægger · Editor");
    }

    [Fact]
    public void PricingFeatureMatrix_RendersUnionOfFeatureKeysAsRows()
    {
        var cut = Render<PricingFeatureMatrix>(p => p.Add(x => x.Tiers, CreateTiers()));

        var rowHeaders = cut.FindAll("table tbody th").Select(h => h.TextContent).ToList();
        rowHeaders.Should().BeEquivalentTo("Bede pr. forslag", "Bedforslag", "Planlagte bede");
    }

    [Fact]
    public void PricingFeatureMatrix_ShowsDashWhenTierHasNoLimitForFeature()
    {
        var cut = Render<PricingFeatureMatrix>(p => p.Add(x => x.Tiers, CreateTiers()));

        cut.Markup.Should().Contain("–");
    }
}