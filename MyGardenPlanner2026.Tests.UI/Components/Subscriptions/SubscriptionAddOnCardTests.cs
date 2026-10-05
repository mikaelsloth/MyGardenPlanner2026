namespace MyGardenPlanner2026.Tests.UI.Components.Subscriptions;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Domain.Subscriptions;
using Xunit;

public class SubscriptionAddOnCardTests : BunitContext
{
    [Fact]
    public void SubscriptionAddOnCard_RendersNameUnitAndAllThreePrices()
    {
        var dto = SubscriptionTestData.BedforslagAddOn();

        var cut = Render<SubscriptionAddOnCard>(p => p.Add(x => x.AddOn, dto));

        cut.Markup.Should().Contain("Bedforslag (Niveau 2)");
        cut.Markup.Should().Contain("Pakke med 2 bedforslag");
        cut.Markup.Should().Contain("180,00 kr.");
        cut.Markup.Should().Contain("15,00 kr.");
        cut.Markup.Should().Contain("450,00 kr.");
    }
}