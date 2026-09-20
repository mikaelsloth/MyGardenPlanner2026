namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Layout;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public sealed class AppShellTests : BunitContext
{
    private readonly IGardenAccessQueryService queryService = Substitute.For<IGardenAccessQueryService>();

    public AppShellTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(queryService);

        var authContext = AddAuthorization();
        authContext.SetAuthorized("user1@example.com");
        authContext.SetClaims(new Claim(ClaimTypes.NameIdentifier, "user-1"));

        queryService.HasAnyGardenAccessAsync("user-1", Arg.Any<CancellationToken>()).Returns(true);
    }

    [Fact]
    public void RendersSkipLinkDesktopSidebarAndMobileHeader()
    {
        var cut = Render<AppShell>(p => p.Add(l => l.Body, "<p class=\"page-content\">Side-indhold</p>"));

        cut.Find(".sr-only-focusable").TextContent.Should().Contain("Spring til hovedindhold");
        cut.Find(".desktop-sidebar").Should().NotBeNull();
        cut.Find(".mobile-header").Should().NotBeNull();
    }

    [Fact]
    public void HasGardenAccess_RendersBodyContent()
    {
        var cut = Render<AppShell>(p => p.Add(l => l.Body, "<p class=\"page-content\">Side-indhold</p>"));

        cut.Find(".page-content").TextContent.Should().Be("Side-indhold");
    }

    [Fact]
    public void ClickingHamburger_OpensNavDrawer()
    {
        var cut = Render<AppShell>(p => p.Add(l => l.Body, "<p>Side-indhold</p>"));

        cut.Find(".mobile-header button").Click();

        cut.Find(".nav-drawer").ClassList.Should().Contain("open");
    }

    [Fact]
    public void ClickingHamburgerTwice_ClosesNavDrawerAgain()
    {
        var cut = Render<AppShell>(p => p.Add(l => l.Body, "<p>Side-indhold</p>"));

        cut.Find(".mobile-header button").Click();
        cut.Find(".mobile-header button").Click();

        cut.Find(".nav-drawer").ClassList.Should().NotContain("open");
    }

    [Fact]
    public void SidebarShowsAuthenticatedUserEmail_AndLogoutForm()
    {
        var cut = Render<AppShell>(p => p.Add(l => l.Body, "<p>Side-indhold</p>"));

        cut.Find(".desktop-sidebar").TextContent.Should().Contain("user1@example.com");
        cut.Find(".desktop-sidebar form[action=\"Account/Logout\"]").Should().NotBeNull();
    }
}