namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteA;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste A1: de offentlige sider loader uden fejl. Hver side testes i alle faste
/// viewports og gemmer et fuldside-skærmbillede (Loadside_&lt;Side&gt;_&lt;bredde&gt;.png).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class OffentligeSiderLoadTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste A";
    private const string TestName = "Loadside";

    private sealed record PublicPageSpec(string Route, string Component, string Title, string? Heading);

    // Forsidens h1 ligger i HeroBanner (ikke uploadet), så dens tekst kontrolleres ikke.
    private static readonly PublicPageSpec[] Specs =
    [
        new(E2ERoutes.Landing, "LandingPage", "MyGardenPlanner – Planlæg din drømmehave", null),
        new(E2ERoutes.Pricing, "PricingPage", "Priser & Abonnement – MyGardenPlanner", "Priser & Abonnement"),
        new(E2ERoutes.About, "AboutPage", "Om platformen – MyGardenPlanner", "Om MyGardenPlanner"),
        new(E2ERoutes.Terms, "TermsPage", "Handelsbetingelser – MyGardenPlanner", "Handelsbetingelser"),
        new(E2ERoutes.Privacy, "PrivacyPage", "Privatlivspolitik – MyGardenPlanner", "Privatlivspolitik"),
    ];

    public static TheoryData<string, int> Cases()
    {
        var data = new TheoryData<string, int>();
        foreach (var spec in Specs)
        {
            foreach (var width in Viewports.All)
            {
                data.Add(spec.Component, width);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Loadside_OffentligSide_LoaderUdenFejlOgGemmerSkaermbillede(string component, int width)
    {
        var spec = Specs.Single(s => s.Component == component);
        var page = await fixture.NewPageAsync(width);

        var pageErrors = new List<string>();
        page.PageError += (_, message) => pageErrors.Add(message);

        var response = await page.GotoAsync($"{fixture.RootUri}{spec.Route}");

        // Skeletons (fx forsidens priskort) skal være væk, før skærmbilledet tages.
        await Assertions.Expect(page.Locator("[class*='skeleton']")).ToHaveCountAsync(0);

        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, spec.Component);

        response.Should().NotBeNull();
        response!.Ok.Should().BeTrue();
        (await page.TitleAsync()).Should().Be(spec.Title);

        var h1 = page.GetByRole(AriaRole.Heading, new() { Level = 1 });
        await Assertions.Expect(h1).ToHaveCountAsync(1);
        if (spec.Heading is not null)
        {
            await Assertions.Expect(h1).ToContainTextAsync(spec.Heading);
        }

        await Assertions.Expect(page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
        pageErrors.Should().BeEmpty();
    }
}