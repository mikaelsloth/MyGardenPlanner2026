namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Pages;
using Xunit;

public sealed class DemoDashboardPageTests : BunitContext
{
    [Fact]
    public void RendersWarningStatusBanner_WithUpgradeCta()
    {
        var cut = Render<DemoDashboardPage>();

        var banner = cut.Find(".status-banner");
        banner.ClassList.Should().Contain("status-banner-warning");
        banner.TextContent.Should().Contain("gratis prøveversion");

        var cta = cut.Find(".status-banner a.btn");
        cta.TextContent.Should().Be("Opgrader abonnement nu");
        cta.GetAttribute("href").Should().Be("/onboarding/checkout");
    }

    [Fact]
    public void CreateBedButton_IsDisabled_AndShowsPermissionHint()
    {
        var cut = Render<DemoDashboardPage>();

        cut.Find(".permission-gate-disabled button").HasAttribute("disabled").Should().BeTrue();
        cut.Find(".permission-hint").TextContent.Should().Be("Tilkøb abonnement for at oprette ubegrænsede bede");
    }
}