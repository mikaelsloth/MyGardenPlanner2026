namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Foundation;
using Xunit;

public sealed class BadgeTests : BunitContext
{
    [Fact]
    public void Badge_DefaultVariant_RendersBaseBadgeRoleClassOnly()
    {
        var cut = Render<Badge>(p => p.AddChildContent("Ny"));

        cut.Find("span").ClassList.Should().Contain("badge-role");
        cut.Markup.Should().Contain("Ny");
    }

    [Fact]
    public void Badge_PrimaryVariant_RendersBadgePrimaryClass()
    {
        var cut = Render<Badge>(p => p.Add(b => b.Variant, Badge.BadgeVariant.Primary));

        cut.Find("span").ClassList.Should().Contain("badge-primary");
    }
}