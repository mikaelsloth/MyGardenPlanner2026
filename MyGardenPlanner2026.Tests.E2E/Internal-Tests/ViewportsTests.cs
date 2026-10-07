namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

public sealed class ViewportsTests
{
    [Theory]
    [InlineData(375, 812)]
    [InlineData(768, 1024)]
    [InlineData(939, 900)]
    [InlineData(940, 900)]
    [InlineData(1280, 800)]
    public void For_KendtBredde_GiverForventetHoejde(int width, int height)
    {
        var viewport = Viewports.For(width);

        viewport.Width.Should().Be(width);
        viewport.Height.Should().Be(height);
    }

    [Fact]
    public void For_UkendtBredde_Kaster()
    {
        var act = () => Viewports.For(500);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void All_IndeholderDeFemFasteBredder()
    {
        Viewports.All.Should().Equal(375, 768, 939, 940, 1280);
    }
}