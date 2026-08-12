namespace MyGardenPlanner2026.Tests.Unit.Entities.Layer1;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using Xunit;

public class SubscriptionTierMapperTests
{
    private static SubscriptionTier CreateTier() => new()
    {
        Id = 1,
        Level = GardenAccessLevel.BedDesigner,
        AccessCategory = AccessCategory.Editor,
        Name = "Bed Designer · Editor",
        Description = "Opret bedforslag i en eksisterende have.",
        AnnualPrice = 120m,
        MonthlyPrice = 10m,
        PerpetualPrice = 300m,
        IsFeatured = true,
        IncludedFeatures = ["2 bedforslag pr. have", "Op til 25 bede pr. forslag"],
        FeatureLimits = new Dictionary<string, string> { ["Bedforslag"] = "2", ["Bede pr. forslag"] = "25" }
    };

    [Theory]
    [InlineData(BillingCycle.Annual, 120)]
    [InlineData(BillingCycle.Monthly, 10)]
    [InlineData(BillingCycle.Perpetual, 300)]
    public void ToDto_ReturnsPriceMatchingRequestedBillingCycle(BillingCycle cycle, decimal expectedPrice)
    {
        var dto = CreateTier().ToDto(cycle);

        dto.Price.Should().Be(expectedPrice);
        dto.BillingCycle.Should().Be(cycle);
    }

    [Fact]
    public void ToDto_CopiesAllDisplayFields()
    {
        var tier = CreateTier();

        var dto = tier.ToDto(BillingCycle.Annual);

        dto.Id.Should().Be(tier.Id);
        dto.Level.Should().Be(tier.Level);
        dto.AccessCategory.Should().Be(tier.AccessCategory);
        dto.Name.Should().Be(tier.Name);
        dto.IsFeatured.Should().Be(tier.IsFeatured);
        dto.IncludedFeatures.Should().BeEquivalentTo(tier.IncludedFeatures);
        dto.FeatureLimits.Should().BeEquivalentTo(tier.FeatureLimits);
    }

    [Fact]
    public void ToDto_NullTier_ThrowsArgumentNullException()
    {
        SubscriptionTier? tier = null;

        var act = () => tier!.ToDto(BillingCycle.Annual);

        act.Should().Throw<ArgumentNullException>();
    }
}