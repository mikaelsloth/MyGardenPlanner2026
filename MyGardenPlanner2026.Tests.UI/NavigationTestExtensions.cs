namespace MyGardenPlanner2026.Tests.UI;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Fælles hjælpere til navigation i bUnit (bUnits fake NavigationManager).
/// </summary>
public static class NavigationTestExtensions
{
    /// <summary>Den aktuelle URI i bUnits NavigationManager.</summary>
    public static string CurrentUri(this BunitContext context) =>
        context.Services.GetRequiredService<NavigationManager>().Uri;

    /// <summary>Asserter, at den aktuelle URI indeholder <paramref name="expectedUriPart"/>.</summary>
    public static void ShouldHaveNavigatedTo(this BunitContext context, string expectedUriPart) =>
        context.CurrentUri().Should().Contain(expectedUriPart);

    /// <summary>
    /// Navigerer til <paramref name="uri"/> og renderer derefter komponenten.
    /// [SupplyParameterFromQuery]-parametre kan ikke sættes via ComponentParameter.Add i bUnit —
    /// de skal bindes via NavigationManager ved at navigere til URL'en med querystring FØR komponenten renderes.
    /// </summary>
    public static IRenderedComponent<TComponent> RenderAt<TComponent>(this BunitContext context, string uri)
        where TComponent : IComponent
    {
        context.Services.GetRequiredService<NavigationManager>().NavigateTo(uri);
        return context.Render<TComponent>();
    }
}