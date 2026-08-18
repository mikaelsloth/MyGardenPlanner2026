namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MyGardenPlanner2026.Components.Layout;
using Xunit;

public class NavDrawerTests : BunitContext
{
    [Fact]
    public void NavDrawer_WhenOpen_RendersOpenClassAndBackdrop()
    {
        var cut = Render<NavDrawer>(p => p.Add(x => x.IsOpen, true));

        cut.Find(".nav-drawer").ClassList.Should().Contain("open");
        cut.FindAll(".drawer-backdrop").Should().HaveCount(1);
    }

    [Fact]
    public void NavDrawer_WhenClosed_DoesNotRenderOpenClassOrBackdrop()
    {
        var cut = Render<NavDrawer>(p => p.Add(x => x.IsOpen, false));

        cut.Find(".nav-drawer").ClassList.Should().NotContain("open");
        cut.FindAll(".drawer-backdrop").Should().BeEmpty();
    }

    [Fact]
    public void NavDrawer_RendersAllFourMenuLinks()
    {
        var cut = Render<NavDrawer>(p => p.Add(x => x.IsOpen, true));

        cut.Find("a[href='/']").TextContent.Should().Contain("Forside");
        cut.Find("a[href='/pricing']").TextContent.Should().Contain("Priser");
        cut.Find("a[href='/login']").TextContent.Should().Contain("Log ind");
        cut.Find("a[href='/register']").TextContent.Should().Contain("Opret bruger");
    }

    [Fact]
    public void NavDrawer_ClickingForsideLink_InvokesOnCloseOnce()
    {
        var closeCount = 0;
        var cut = Render<NavDrawer>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.OnClose, EventCallback.Factory.Create(this, () => closeCount++)));

        cut.Find("a[href='/']").Click();

        closeCount.Should().Be(1);
    }

    [Fact]
    public void NavDrawer_ClickingBackdrop_InvokesOnCloseOnce()
    {
        var closeCount = 0;
        var cut = Render<NavDrawer>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.OnClose, EventCallback.Factory.Create(this, () => closeCount++)));

        cut.Find(".drawer-backdrop").Click();

        closeCount.Should().Be(1);
    }

    [Fact]
    public void NavDrawer_PressingEscape_InvokesOnCloseOnce()
    {
        var closeCount = 0;
        var cut = Render<NavDrawer>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.OnClose, EventCallback.Factory.Create(this, () => closeCount++)));

        cut.Find(".nav-drawer").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        closeCount.Should().Be(1);
    }
}