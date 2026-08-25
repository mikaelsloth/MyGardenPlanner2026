namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Domain.Subscriptions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public class SubscriptionAddOnCardTests : BunitContext
{
    [Fact]
    public void SubscriptionAddOnCard_RendersNameUnitAndAllThreePrices()
    {
        var dto = new SubscriptionAddOnDto(
            Guid.NewGuid(), AddOnType.BedforslagNiveau2, "Bedforslag (Niveau 2)", "Pakke med 2 bedforslag",
            AnnualPrice: 180m, MonthlyPrice: 15m, PerpetualPrice: 450m);

        var cut = Render<SubscriptionAddOnCard>(p => p.Add(x => x.AddOn, dto));

        cut.Markup.Should().Contain("Bedforslag (Niveau 2)");
        cut.Markup.Should().Contain("Pakke med 2 bedforslag");
        cut.Markup.Should().Contain("180,00 kr.");
        cut.Markup.Should().Contain("15,00 kr.");
        cut.Markup.Should().Contain("450,00 kr.");
    }
}