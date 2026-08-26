namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using MyGardenPlanner2026.Components.Layout;
using Xunit;

public class PublicHeaderNavDrawerIntegrationTests : BunitContext
{
    [Fact]
    public void PublicHeader_InitialRender_NavDrawerIsClosed()
    {
        var module = JSInterop.SetupModule("./Components/Layout/NavDrawer.razor.js");
        module.SetupVoid("activate", _ => true).SetVoidResult();
        module.SetupVoid("deactivate").SetVoidResult();
        var cut = Render<PublicHeader>();

        cut.Find(".nav-drawer").ClassList.Should().NotContain("open");
        cut.FindAll(".drawer-backdrop").Should().BeEmpty();
    }

    [Fact]
    public void PublicHeader_ClickingHamburgerButton_OpensNavDrawer()
    {
        var module = JSInterop.SetupModule("./Components/Layout/NavDrawer.razor.js");
        module.SetupVoid("activate", _ => true).SetVoidResult();
        module.SetupVoid("deactivate").SetVoidResult();
        var cut = Render<PublicHeader>();

        cut.Find("button[aria-label='Åbn menu']").Click();

        cut.Find(".nav-drawer").ClassList.Should().Contain("open");
        cut.FindAll(".drawer-backdrop").Should().HaveCount(1);
    }

    [Fact]
    public void PublicHeader_ClickingHamburgerTwice_ClosesNavDrawerAgain()
    {
        var module = JSInterop.SetupModule("./Components/Layout/NavDrawer.razor.js");
        module.SetupVoid("activate", _ => true).SetVoidResult();
        module.SetupVoid("deactivate").SetVoidResult();
        var cut = Render<PublicHeader>();
        var button = cut.Find("button[aria-label='Åbn menu']");

        button.Click();
        button.Click();

        cut.Find(".nav-drawer").ClassList.Should().NotContain("open");
    }

    [Fact]
    public void PublicHeader_ClickingBackdropAfterOpening_ClosesNavDrawer()
    {
        var module = JSInterop.SetupModule("./Components/Layout/NavDrawer.razor.js");
        module.SetupVoid("activate", _ => true).SetVoidResult();
        module.SetupVoid("deactivate").SetVoidResult();
        var cut = Render<PublicHeader>();
        cut.Find("button[aria-label='Åbn menu']").Click();

        cut.Find(".drawer-backdrop").Click();

        cut.Find(".nav-drawer").ClassList.Should().NotContain("open");
    }

    [Fact]
    public void PublicHeader_PressingEscapeAfterOpening_ClosesNavDrawer()
    {
        var module = JSInterop.SetupModule("./Components/Layout/NavDrawer.razor.js");
        module.SetupVoid("activate", _ => true).SetVoidResult();
        module.SetupVoid("deactivate").SetVoidResult();
        var cut = Render<PublicHeader>();
        cut.Find("button[aria-label='Åbn menu']").Click();

        cut.Find(".nav-drawer").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        cut.Find(".nav-drawer").ClassList.Should().NotContain("open");
    }

    [Fact]
    public void PublicHeader_ClickingHamburgerButton_TogglesAriaExpanded()
    {
        var module = JSInterop.SetupModule("./Components/Layout/NavDrawer.razor.js");
        module.SetupVoid("activate", _ => true).SetVoidResult();
        module.SetupVoid("deactivate").SetVoidResult();
        var cut = Render<PublicHeader>();
        var button = cut.Find("button[aria-label='Åbn menu']");

        button.GetAttribute("aria-expanded").Should().Be("False");

        button.Click();

        cut.Find("button[aria-label='Åbn menu']").GetAttribute("aria-expanded").Should().Be("True");
    }
}