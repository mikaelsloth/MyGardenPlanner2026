namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Layout;
using Xunit;

public sealed class AppNavDrawerTests : BunitContext
{
    public AppNavDrawerTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        AddAuthorization().SetAuthorized("user1@example.com");
    }

    [Fact]
    public void WhenOpen_RendersOpenClassAndBackdrop()
    {
        var cut = Render<AppNavDrawer>(p => p.Add(d => d.IsOpen, true));

        cut.Find(".nav-drawer").ClassList.Should().Contain("open");
        cut.Find(".drawer-backdrop").Should().NotBeNull();
    }

    [Fact]
    public void WhenClosed_DoesNotRenderOpenClassOrBackdrop()
    {
        var cut = Render<AppNavDrawer>(p => p.Add(d => d.IsOpen, false));

        cut.Find(".nav-drawer").ClassList.Should().NotContain("open");
        cut.FindAll(".drawer-backdrop").Should().BeEmpty();
    }

    [Fact]
    public void RendersAuthenticatedUserEmail_AndLogoutForm()
    {
        var cut = Render<AppNavDrawer>(p => p.Add(d => d.IsOpen, true));

        cut.Find(".app-nav-drawer-email").TextContent.Should().Be("user1@example.com");
        cut.Find("form[action=\"Account/Logout\"]").Should().NotBeNull();
    }

    [Fact]
    public void ClickingBackdrop_InvokesOnCloseOnce()
    {
        var callCount = 0;
        var cut = Render<AppNavDrawer>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.OnClose, () => callCount++));

        cut.Find(".drawer-backdrop").Click();

        callCount.Should().Be(1);
    }

    [Fact]
    public void PressingEscape_InvokesOnCloseOnce()
    {
        var callCount = 0;
        var cut = Render<AppNavDrawer>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.OnClose, () => callCount++));

        cut.Find(".nav-drawer").KeyDown("Escape");

        callCount.Should().Be(1);
    }
}