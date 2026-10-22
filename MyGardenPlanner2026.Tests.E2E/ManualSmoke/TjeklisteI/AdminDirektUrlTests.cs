namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteI;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Text.RegularExpressions;
using AdminPage = MyGardenPlanner2026.Tests.E2E.AdminAccessMatrix.AdminPage;

/// <summary>
/// Tjekliste I2: direkte URL til admin-side uden rolle. I2a: anonym bruger sendes til login med ReturnUrl til den
/// forespurgte side (ikke AccessDenied). I2b: #1-#4 får AccessDenied på alle sider uden for
/// <see cref="AdminAccessMatrix"/>, sidens indhold og navigationen vises ikke, og personaen kan stadig åbne sin egen
/// side. Requester/NoMfa/Plain er dækket af ModalVisesTests, PersonaAccessTests og KorrektStepUpTests.
/// Skærmbilleder: Adgangskontrol_I2_Anonym_&lt;Side&gt;_&lt;bredde&gt; og
/// Adgangskontrol_I2_AccessDenied_&lt;Persona&gt;_&lt;Side&gt;_&lt;bredde&gt;.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed partial class AdminDirektUrlTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste I";
    private const string TestName = "Adgangskontrol";

    [GeneratedRegex(@"/account/login(\?|$)", RegexOptions.IgnoreCase)]
    private static partial Regex LoginUrl { get; }

    [GeneratedRegex(@"/account/accessdenied(\?|$)", RegexOptions.IgnoreCase)]
    private static partial Regex AccessDeniedUrl { get; }

    public static TheoryData<string, int> AnonymousCases()
    {
        var data = new TheoryData<string, int>();
        foreach (var page in AdminAccessMatrix.AllPages)
        {
            data.Add(page.Route, Viewports.Tablet);
            data.Add(page.Route, Viewports.Desktop);
        }

        return data;
    }

    public static TheoryData<string, int> PersonaCases()
    {
        var data = new TheoryData<string, int>();
        foreach (var persona in AdminAccessMatrix.Personas)
        {
            data.Add(persona, Viewports.Tablet);
            data.Add(persona, Viewports.Desktop);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(AnonymousCases))]
    public async Task Anonym_DirekteUrlTilAdminSide_RedirectesTilLoginMedReturnUrl(string route, int width)
    {
        var target = AdminAccessMatrix.AllPages.Single(p => p.Route == route);
        var page = await fixture.NewPageAsync(width);

        await page.GotoAsync($"{fixture.RootUri}{target.Route}");

        // Fejler denne, er destinationen enten AccessDenied eller noget andet: Playwright viser den faktiske URL.
        await Assertions.Expect(page).ToHaveURLAsync(LoginUrl);
        await SmokePageAssertions.ExpectPageAsync(page, "Log ind – MyGardenPlanner", "Log ind");

        LoginRedirect.ReturnPath(page.Url)?.TrimEnd('/')
            .Should().BeEquivalentTo(target.Route, "ReturnUrl skal føre tilbage til den forespurgte side. URL: {0}", page.Url);

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = target.Heading, Exact = true }))
            .ToHaveCountAsync(0);
        await Assertions.Expect(AdminNavigation.Nav(page)).ToHaveCountAsync(0);

        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, $"I2_Anonym_{target.LinkText}");
    }

    [Theory]
    [MemberData(nameof(PersonaCases))]
    public async Task KrydsAdgang_SideUdenforMatricen_GiverAccessDeniedUdenIndholdOgNavigation(string persona, int width)
    {
        var access = AdminAccessMatrix.For(persona);
        var denied = AdminAccessMatrix.DeniedPages(persona);

        // Login og tilbage via ReturnUrl lander på den første afviste side.
        var page = await fixture.LoginAndGotoAsync(persona, denied[0].Route, width);

        for (var i = 0; i < denied.Count; i++)
        {
            var target = denied[i];

            if (i > 0)
            {
                await page.GotoAsync($"{fixture.RootUri}{target.Route}");
            }

            await ExpectDeniedAsync(page, target);
            await ScreenshotRecorder.CaptureAsync(
                page, Chapter, Checklist, TestName, $"I2_AccessDenied_{persona}_{target.LinkText}");
        }

        // Kontrol: fejlen er rollen, ikke login. Personaen kan stadig åbne sin egen side.
        await page.GotoAsync($"{fixture.RootUri}{access.Start.Route}");
        await SmokePageAssertions.ExpectPageAsync(page, access.Start.Title, access.Start.Heading);
    }

    private static async Task ExpectDeniedAsync(IPage page, AdminPage target)
    {
        await Assertions.Expect(page).ToHaveURLAsync(AccessDeniedUrl);
        await AccessAssertions.ExpectAccessDeniedAsync(page);

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = target.Heading, Exact = true }))
            .ToHaveCountAsync(0);
        await Assertions.Expect(AdminNavigation.Nav(page)).ToHaveCountAsync(0);
    }
}