namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using Xunit;

public class SmokeTest : BunitContext
{
    [Fact]
    public void BUnitTestEnvironment_IsWorking()
    {
        // Arrange & Act
        var cut = Render<Microsoft.AspNetCore.Components.Web.HeadContent>();

        // Assert
        Assert.NotNull(cut);
    }
}