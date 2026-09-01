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

public class PricingMatrixTableTests : BunitContext
{
    private static SubscriptionTierDto CreateDto(GardenAccessLevel level, AccessCategory category, decimal price) => new(
        Id: Guid.NewGuid(),
        Level: level,
        AccessCategory: category,
        Name: $"{level} · {category}",
        Description: "Beskrivelse",
        Price: price,
        BillingCycle: BillingCycle.Annual,
        IsFeatured: category == AccessCategory.Editor,
        IncludedFeatures: [],
        FeatureLimits: new Dictionary<string, string>());

    private ISubscriptionPricingService RegisterFakeService()
    {
        var service = Substitute.For<ISubscriptionPricingService>();
        service.GetAllTiersAsync(Arg.Any<BillingCycle>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SubscriptionTierDto>>(
            [
                CreateDto(GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, 336m),
                CreateDto(GardenAccessLevel.HaveArkitekt, AccessCategory.Editor, 168m)
            ]));

        Services.AddSingleton(service);
        return service;
    }

    [Fact]
    public void PricingMatrixTable_RendersLevelTitleAndPrices()
    {
        RegisterFakeService();

        var cut = Render<PricingMatrixTable>();

        cut.Markup.Should().Contain("Have Arkitekt");
        cut.Markup.Should().Contain("336");
    }

    [Fact]
    public void PricingMatrixTable_ClickingMonthlyTab_ReloadsTiersWithMonthlyCycle()
    {
        var service = RegisterFakeService();
        var cut = Render<PricingMatrixTable>();

        cut.FindAll(".context-tab")[1].Click();

        _ = service.Received().GetAllTiersAsync(BillingCycle.Monthly, Arg.Any<CancellationToken>());
    }
}