namespace MyGardenPlanner2026.Tests.UI.Internal_Tests;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class NavigationTestExtensionsTests : BunitContext
{
    /// <summary>Viser den URI, komponenten ser ved render.</summary>
    public sealed class UriProbe : ComponentBase
    {
        [Inject]
        public NavigationManager Navigation { get; set; } = default!;

        protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, Navigation.Uri);
    }

    [Fact]
    public void CurrentUri_ReturnsNavigationManagerUri()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("/pricing");

        this.CurrentUri().Should().EndWith("/pricing");
    }

    [Fact]
    public void RenderAt_NavigatesBeforeRendering_SoComponentSeesTargetUri()
    {
        var cut = this.RenderAt<UriProbe>("/target?x=1");

        cut.Markup.Should().Contain("/target?x=1");
        this.CurrentUri().Should().EndWith("/target?x=1");
    }

    [Fact]
    public void ShouldHaveNavigatedTo_UriContainsFragment_Passes()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("/onboarding/checkout?level=BedDesigner");

        this.ShouldHaveNavigatedTo("/onboarding/checkout");
    }

    [Fact]
    public void ShouldHaveNavigatedTo_UriLacksFragment_Fails()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("/pricing");

        var act = () => this.ShouldHaveNavigatedTo("/onboarding");

        act.Should().Throw<Exception>();
    }
}