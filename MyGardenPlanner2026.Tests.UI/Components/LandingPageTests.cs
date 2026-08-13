namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Marketing;
using MyGardenPlanner2026.Components.Domain.Subscriptions;
using MyGardenPlanner2026.Components.Pages;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using NSubstitute;
using Xunit;

public class LandingPageTests : BunitContext
{
    private static SubscriptionTierDto CreateDto(GardenAccessLevel level) => new(
        Id: (int)level,
        Level: level,
        AccessCategory: AccessCategory.Editor,
        Name: $"{level} · Editor",
        Description: "Beskrivelse",
        Price: 100m,
        BillingCycle: BillingCycle.Annual,
        IsFeatured: true,
        IncludedFeatures: ["Feature"],
        FeatureLimits: new Dictionary<string, string>());

    private void RegisterFakePricingService()
    {
        var service = Substitute.For<ISubscriptionPricingService>();
        service.GetFeaturedTiersAsync(BillingCycle.Annual, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SubscriptionTierDto>>(
            [
                CreateDto(GardenAccessLevel.HaveArkitekt),
                CreateDto(GardenAccessLevel.BedDesigner),
                CreateDto(GardenAccessLevel.Planlaegger)
            ]));

        Services.AddSingleton(service);
    }

    [Fact]
    public void LandingPage_RendersHeroBannerAndThreeFeatureCards()
    {
        RegisterFakePricingService();

        var cut = Render<LandingPage>();

        cut.FindComponent<HeroBanner>().Should().NotBeNull();
        cut.FindComponents<FeatureCard>().Should().HaveCount(3);
    }

    [Fact]
    public void LandingPage_RendersThreePricingCards_OncePricingServiceResolves()
    {
        RegisterFakePricingService();

        var cut = Render<LandingPage>();

        cut.FindComponents<PricingCard>().Should().HaveCount(3);
    }

    [Fact]
    public void LandingPage_ClickingPricingCardButton_NavigatesToPricingPage()
    {
        RegisterFakePricingService();
        var cut = Render<LandingPage>();
        var navMan = Services.GetRequiredService<BunitNavigationManager>();

        cut.FindComponents<PricingCard>()[0].Find("button").Click();

        navMan.Uri.Should().EndWith("/pricing");
    }
}