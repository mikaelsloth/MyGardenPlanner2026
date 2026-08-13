namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Domain.Marketing;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public class FeatureCardTests : BunitContext
{
    [Fact]
    public void FeatureCard_RendersBadgeTitleAndDescription()
    {
        var cut = Render<FeatureCard>(p => p
            .Add(x => x.AccessLevel, GardenAccessLevel.BedDesigner)
            .Add(x => x.Title, "Beddesign & Materialer")
            .Add(x => x.Description, "Skitsér bedforslag."));

        cut.Markup.Should().Contain("Bed Designer");
        cut.Markup.Should().Contain("Beddesign &amp; Materialer");
        cut.Markup.Should().Contain("Skitsér bedforslag.");
    }
}