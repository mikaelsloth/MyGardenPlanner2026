namespace MyGardenPlanner2026.Tests.Unit.Services.Account;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public sealed class GardenRestoreCoverageTests
{
    [Theory]
    [InlineData(GardenAccessLevel.BedDesigner, AccessCategory.Editor, GardenAccessLevel.BedDesigner, AccessCategory.Editor, true)]
    [InlineData(GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, GardenAccessLevel.Planlaegger, AccessCategory.Viewer, true)]
    [InlineData(GardenAccessLevel.HaveArkitekt, AccessCategory.Editor, GardenAccessLevel.BedDesigner, AccessCategory.Editor, true)]
    [InlineData(GardenAccessLevel.BedDesigner, AccessCategory.Administrator, GardenAccessLevel.BedDesigner, AccessCategory.Editor, true)]
    [InlineData(GardenAccessLevel.Planlaegger, AccessCategory.Editor, GardenAccessLevel.BedDesigner, AccessCategory.Editor, false)]
    [InlineData(GardenAccessLevel.BedDesigner, AccessCategory.Viewer, GardenAccessLevel.BedDesigner, AccessCategory.Editor, false)]
    [InlineData(GardenAccessLevel.HaveArkitekt, AccessCategory.Editor, GardenAccessLevel.BedDesigner, AccessCategory.Administrator, false)]
    [InlineData(GardenAccessLevel.Planlaegger, AccessCategory.Administrator, GardenAccessLevel.HaveArkitekt, AccessCategory.Viewer, false)]
    public void Covers_RequiresLayerAtLeastAsHighAndCategoryAtLeastAsHigh(
        GardenAccessLevel purchasedLayer, AccessCategory purchasedCategory,
        GardenAccessLevel requiredLayer, AccessCategory requiredCategory, bool expected) => GardenRestoreCoverage.Covers(purchasedLayer, purchasedCategory, requiredLayer, requiredCategory)
            .Should().Be(expected);
}