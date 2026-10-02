namespace MyGardenPlanner2026.Tests.Unit.Internal_Tests;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public sealed class TestEntitiesTests
{
    [Fact]
    public void AddOn_WithoutArguments_UsesDefaults()
    {
        var addOn = TestEntities.AddOn();

        addOn.Type.Should().Be(AddOnType.ArtefaktpakkeA);
        addOn.Name.Should().Be("Test add-on");
        addOn.UnitDescription.Should().Be("Enhed");
        addOn.AnnualPrice.Should().Be(48m);
        addOn.MonthlyPrice.Should().Be(4m);
        addOn.PerpetualPrice.Should().Be(120m);
    }

    [Fact]
    public void AddOn_WithArguments_OverridesDefaults()
    {
        var addOn = TestEntities.AddOn(AddOnType.ArtefaktpakkeB, "B", 24m, 2m, 60m);

        addOn.Type.Should().Be(AddOnType.ArtefaktpakkeB);
        addOn.Name.Should().Be("B");
        addOn.AnnualPrice.Should().Be(24m);
        addOn.MonthlyPrice.Should().Be(2m);
        addOn.PerpetualPrice.Should().Be(60m);
    }

    [Fact]
    public void AddOn_TwoCalls_ProduceDifferentIds()
    {
        TestEntities.AddOn().Id.Should().NotBe(TestEntities.AddOn().Id);
    }

    [Fact]
    public void Tier_WithoutArguments_UsesDefaults()
    {
        var tier = TestEntities.Tier();

        tier.Level.Should().Be(GardenAccessLevel.HaveArkitekt);
        tier.AccessCategory.Should().Be(AccessCategory.Viewer);
        tier.Name.Should().Be("Test tier");
        tier.Description.Should().Be("Test");
        tier.AnnualPrice.Should().Be(100m);
        tier.MonthlyPrice.Should().Be(10m);
        tier.PerpetualPrice.Should().Be(250m);
    }

    [Fact]
    public void Tier_WithArguments_OverridesDefaults()
    {
        var tier = TestEntities.Tier(GardenAccessLevel.BedDesigner, AccessCategory.Editor, "X", 42m, 3.5m, 105m);

        tier.Level.Should().Be(GardenAccessLevel.BedDesigner);
        tier.AccessCategory.Should().Be(AccessCategory.Editor);
        tier.Name.Should().Be("X");
        tier.AnnualPrice.Should().Be(42m);
        tier.MonthlyPrice.Should().Be(3.5m);
        tier.PerpetualPrice.Should().Be(105m);
    }

    [Fact]
    public void Tier_TwoCalls_ProduceDifferentIds()
    {
        TestEntities.Tier().Id.Should().NotBe(TestEntities.Tier().Id);
    }
}