namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Components.Domain.Subscriptions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using Xunit;

public class PricingCardTests : BunitContext
{
    private static SubscriptionTierDto CreateDto(bool isFeatured = false) => new(
        Id: 1,
        Level: GardenAccessLevel.BedDesigner,
        AccessCategory: AccessCategory.Editor,
        Name: "Bed Designer · Editor",
        Description: "Opret bedforslag i en eksisterende have.",
        Price: 120m,
        BillingCycle: BillingCycle.Annual,
        IsFeatured: isFeatured,
        IncludedFeatures: ["2 bedforslag pr. have"],
        FeatureLimits: new Dictionary<string, string> { ["Bedforslag"] = "2" });

    [Fact]
    public void PricingCard_RendersNameDescriptionAndDanishFormattedPrice()
    {
        var cut = Render<PricingCard>(p => p.Add(x => x.Tier, CreateDto()));

        cut.Markup.Should().Contain("Bed Designer · Editor");
        cut.Markup.Should().Contain("Opret bedforslag i en eksisterende have.");
        cut.Markup.Should().Contain("120,00 kr.");
        cut.Markup.Should().Contain("/år");
    }

    [Fact]
    public void PricingCard_WhenFeatured_ShowsBadgeAndPrimaryButton()
    {
        var cut = Render<PricingCard>(p => p.Add(x => x.Tier, CreateDto(isFeatured: true)));

        cut.Markup.Should().Contain("Mest populær");
        cut.Find("button").ClassList.Should().Contain("btn-primary");
    }

    [Fact]
    public void PricingCard_WhenNotFeatured_UsesSecondaryButtonAndNoBadge()
    {
        var cut = Render<PricingCard>(p => p.Add(x => x.Tier, CreateDto(isFeatured: false)));

        cut.Markup.Should().NotContain("Mest populær");
        cut.Find("button").ClassList.Should().Contain("btn-secondary");
    }

    [Fact]
    public void PricingCard_ClickingButton_InvokesOnSelectPlanWithTierId()
    {
        int? selectedId = null;
        var cut = Render<PricingCard>(p => p
            .Add(x => x.Tier, CreateDto())
            .Add(x => x.OnSelectPlan, EventCallback.Factory.Create<int>(this, id => selectedId = id)));

        cut.Find("button").Click();

        selectedId.Should().Be(1);
    }
}