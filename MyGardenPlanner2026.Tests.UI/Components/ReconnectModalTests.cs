namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Layout;
using Xunit;

public class ReconnectModalTests : BunitContext
{
    [Fact]
    public void ReconnectModal_PreservesBlazorFrameworkIdsAndClasses()
    {
        var cut = Render<ReconnectModal>();

        // Disse id'er/klasser er kontrakt med Blazors indbyggede reconnect-JS
        // og MÅ ALDRIG ændres, uanset branding.
        cut.Find("#components-reconnect-modal").Should().NotBeNull();
        cut.Find("#components-seconds-to-next-attempt").Should().NotBeNull();
        cut.Find("#components-reconnect-button").Should().NotBeNull();
        cut.Find("#components-resume-button").Should().NotBeNull();
        cut.Find(".components-reconnect-first-attempt-visible").Should().NotBeNull();
        cut.Find(".components-reconnect-repeated-attempt-visible").Should().NotBeNull();
        cut.Find(".components-reconnect-failed-visible").Should().NotBeNull();
        cut.Find(".components-pause-visible").Should().NotBeNull();
        cut.Find(".components-resume-failed-visible").Should().NotBeNull();
    }

    [Fact]
    public void ReconnectModal_RendersDanishText()
    {
        var cut = Render<ReconnectModal>();

        cut.Markup.Should().Contain("Genopretter forbindelse til serveren");
        cut.Markup.Should().Contain("Kunne ikke genoprette forbindelsen");
        cut.Find("#components-reconnect-button").TextContent.Should().Contain("Prøv igen");
        cut.Markup.Should().Contain("Sessionen er blevet sat på pause");
        cut.Find("#components-resume-button").TextContent.Should().Contain("Genoptag");
    }
}