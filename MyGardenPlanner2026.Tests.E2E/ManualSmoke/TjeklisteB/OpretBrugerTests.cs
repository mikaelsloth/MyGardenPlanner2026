namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteB;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Text.RegularExpressions;

/// <summary>
/// Tjekliste B1: opret bruger. Fra forsiden via headerens "Opret bruger" til /Account/Register,
/// derefter /Account/RegisterConfirmation med bekræftelseslinket (no-op sender). Alle faste
/// viewports. Skærmbilleder: Oprettelse_Register_&lt;bredde&gt; og Oprettelse_RegisterConfirmation_&lt;bredde&gt;.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed partial class OpretBrugerTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste B";
    private const string TestName = "Oprettelse";

    [GeneratedRegex("/account/register$", RegexOptions.IgnoreCase)]
    private static partial Regex RegisterUrl { get; }

    [GeneratedRegex(@"/account/registerconfirmation\?", RegexOptions.IgnoreCase)]
    private static partial Regex RegisterConfirmationUrl { get; }

    public static TheoryData<int> Widths()
    {
        var data = new TheoryData<int>();
        foreach (var width in Viewports.All)
        {
            data.Add(width);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Widths))]
    public async Task OpretBruger_FraForsiden_ViserBekraeftelseslinkOgGemmerSkaermbilleder(int width)
    {
        var page = await fixture.NewPageAsync(width);

        var pageErrors = new List<string>();
        page.PageError += (_, message) => pageErrors.Add(message);

        await page.GotoAsync(fixture.RootUri);
        await PublicNavigation.ClickRegisterAsync(page);

        await Assertions.Expect(page).ToHaveURLAsync(RegisterUrl);
        await SmokePageAssertions.ExpectPageAsync(page, "Opret bruger – MyGardenPlanner", "Opret bruger");
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "Register");

        await RegisteredUserFlow.RegisterAsync(page);

        await Assertions.Expect(page).ToHaveURLAsync(RegisterConfirmationUrl);
        await SmokePageAssertions.ExpectPageAsync(page, "Bekræft registrering – MyGardenPlanner", "Bekræft registrering");
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Klik her for at bekræfte din konto" }))
            .ToBeVisibleAsync();
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "RegisterConfirmation");

        pageErrors.Should().BeEmpty();
    }

    // Verificerer hjælperen RegisteredUserFlow, som B3-B5 bygger på (ikke en del af tjeklistens skærmbilleder).
    [Fact]
    public async Task Bekraeftelseslink_BekraefterKonto_OgBrugerenKanLoggeInd()
    {
        var page = await fixture.NewPageAsync();
        var user = await RegisteredUserFlow.RegisterAndConfirmAsync(page, fixture.RootUri);

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
        await LoginFlow.LoginAsync(page, user);

        page.Url.Should().NotContainEquivalentOf(E2ERoutes.Login);
    }
}