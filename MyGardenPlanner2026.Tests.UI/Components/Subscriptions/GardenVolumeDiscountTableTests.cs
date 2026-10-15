namespace MyGardenPlanner2026.Tests.UI.Components.Subscriptions;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Subscriptions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using NSubstitute;
using Xunit;

public class GardenVolumeDiscountTableTests : BunitContext
{
    private readonly IGardenVolumeDiscountService discountService = Substitute.For<IGardenVolumeDiscountService>();

    public GardenVolumeDiscountTableTests()
    {
        Services.AddSingleton(discountService);
    }

    [Fact]
    public void GardenVolumeDiscountTable_RendersAllSevenTiersAndInfoBox()
    {
        discountService.GetAllTiersAsync(Arg.Any<CancellationToken>())
            .Returns(SubscriptionTestData.DefaultVolumeDiscountTiers());

        var cut = Render<GardenVolumeDiscountTable>();

        cut.FindAll("tbody tr").Should().HaveCount(7);
        cut.FindAll(".status-info").Should().HaveCount(1);
        cut.Markup.Should().Contain("25%");
    }

    [Fact]
    public void GardenVolumeDiscountTable_RendersRangeLabelAndPercent_ForEachTierFromService()
    {
        discountService.GetAllTiersAsync(Arg.Any<CancellationToken>())
            .Returns(SubscriptionTestData.DefaultVolumeDiscountTiers());

        var cut = Render<GardenVolumeDiscountTable>();

        var rows = cut.FindAll("tbody tr");
        rows.Select(r => r.QuerySelector("th")!.TextContent.Trim()).Should().Equal(
            "1 have", "2 - 5 haver", "6 - 10 haver", "11 - 50 haver", "51 - 100 haver", "101 - 200 haver", "201+ haver");
        rows.Select(r => r.QuerySelector("td")!.TextContent.Trim()).Should().Equal(
            "100 % af basispris", "90 % af basispris", "80 % af basispris", "70 % af basispris",
            "60 % af basispris", "50 % af basispris", "40 % af basispris");
    }

    [Fact]
    public void GardenVolumeDiscountTable_ShowsEditedTiersFromService_NotTheDefaultCatalog()
    {
        discountService.GetAllTiersAsync(Arg.Any<CancellationToken>()).Returns(
        [
            SubscriptionTestData.VolumeDiscountTier(1, 2, 1.00m),
            SubscriptionTestData.VolumeDiscountTier(3, 4, 0.85m),
            SubscriptionTestData.VolumeDiscountTier(5, null, 0.75m),
        ]);

        var cut = Render<GardenVolumeDiscountTable>();

        var rows = cut.FindAll("tbody tr");
        rows.Select(r => r.QuerySelector("th")!.TextContent.Trim()).Should().Equal("1 - 2 haver", "3 - 4 haver", "5+ haver");
        rows[1].QuerySelector("td")!.TextContent.Trim().Should().Be("85 % af basispris");
        rows[2].QuerySelector("td")!.TextContent.Trim().Should().Be("75 % af basispris");
    }

    [Fact]
    public void GardenVolumeDiscountTable_WhenServiceReturnsNoTiers_RendersEmptyBodyAndInfoBox()
    {
        discountService.GetAllTiersAsync(Arg.Any<CancellationToken>()).Returns([]);

        var cut = Render<GardenVolumeDiscountTable>();

        cut.FindAll("tbody tr").Should().BeEmpty();
        cut.FindAll(".status-info").Should().HaveCount(1);
    }
}