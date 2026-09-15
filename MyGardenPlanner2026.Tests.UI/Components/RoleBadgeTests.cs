namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Domain.Gardens;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public sealed class RoleBadgeTests : BunitContext
{
    [Fact]
    public void IsOwnerTrue_RendersEjerLabel_RegardlessOfCategory()
    {
        var cut = Render<RoleBadge>(p => p
            .Add(b => b.IsOwner, true)
            .Add(b => b.Category, AccessCategory.Viewer));

        cut.Markup.Should().Contain("Ejer");
    }

    [Theory]
    [InlineData(AccessCategory.Administrator, "Administrator")]
    [InlineData(AccessCategory.Editor, "Redaktør")]
    [InlineData(AccessCategory.ViewerPlus, "Læser+")]
    [InlineData(AccessCategory.Viewer, "Læser")]
    public void IsOwnerFalse_RendersDanishCategoryLabel(AccessCategory category, string expectedLabel)
    {
        var cut = Render<RoleBadge>(p => p
            .Add(b => b.IsOwner, false)
            .Add(b => b.Category, category));

        cut.Markup.Should().Contain(expectedLabel);
    }
}