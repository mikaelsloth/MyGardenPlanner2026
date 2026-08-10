namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using MyGardenPlanner2026.Components.Layout;
using Xunit;

public class PublicLayoutTests : BunitContext
{
    [Fact]
    public void PublicHeader_RendersBrandAndNavigationButtons()
    {
        // Act
        var cut = Render<PublicHeader>();

        // Assert
        var brandLink = cut.Find("a[href='/']");
        Assert.NotNull(brandLink);
        Assert.Contains("MyGardenPlanner", brandLink.TextContent);

        var loginButton = cut.Find("a[href='/login']");
        Assert.Contains("Log ind", loginButton.TextContent);

        var registerButton = cut.Find("a[href='/register']");
        Assert.Contains("Opret bruger", registerButton.TextContent);
    }

    [Fact]
    public void PublicFooter_RendersFourColumnsAndSemanticFooter()
    {
        // Act
        var cut = Render<PublicFooter>();

        // Assert
        var footer = cut.Find("footer");
        Assert.NotNull(footer);

        var columns = cut.FindAll(".col-12");
        Assert.Equal(4, columns.Count);

        Assert.Contains("MyGardenPlanner", cut.Markup);
        Assert.Contains("Navigation", cut.Markup);
        Assert.Contains("Abonnementer", cut.Markup);
        Assert.Contains("Systemstatus", cut.Markup);
    }

    [Fact]
    public void PublicLayout_RendersSkipLinkHeaderMainAndFooter()
    {
        // Act
        var cut = Render<PublicLayout>();

        // Assert - Verificer tilgængeligheds-skiplink
        var skipLink = cut.Find("a[href='#main-content']");
        Assert.NotNull(skipLink);
        Assert.Contains("Spring til hovedindhold", skipLink.TextContent);

        // Assert - Verificer semantisk hovedområde
        var main = cut.Find("main#main-content");
        Assert.NotNull(main);

        // Assert - Verificer tilstedeværelse af underkomponenter
        Assert.NotNull(cut.FindComponent<PublicHeader>());
        Assert.NotNull(cut.FindComponent<PublicFooter>());
    }
}