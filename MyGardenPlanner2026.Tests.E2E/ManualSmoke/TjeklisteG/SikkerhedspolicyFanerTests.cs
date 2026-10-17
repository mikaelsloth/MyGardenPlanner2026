namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteG;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Globalization;
using Ids = MyGardenPlanner2026.Tests.E2E.SecurityPolicyPage.FieldIds;

/// <summary>
/// Tjekliste G1: alle 6 faner på /admin/security-policies viser de seedede værdier. Forventede
/// værdier er faste tal (ikke udledt af produktionskoden). Reauth og Login afviger fra appsettings,
/// fordi PlaywrightAppFixture overstyrer dem (ReAuthenticationPolicy__MaxAgeMinutes = 1,
/// LoginRateLimit__PermitLimit = 1000). Skærmbilleder: Sikkerhedspolicies_G1_&lt;Fane&gt; (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class SikkerhedspolicyFanerTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste G";
    private const string TestName = "Sikkerhedspolicies";

    private sealed record ExpectedField(string Id, int Value);

    private sealed record TabExpectation(SecurityPolicyPage.PolicyTab Tab, ExpectedField[] Fields);

    private static readonly TabExpectation[] Expectations =
    [
        new(SecurityPolicyPage.Jit,
        [
            new(Ids.JitMin, 30),
            new(Ids.JitMax, 90),
            new(Ids.JitSweep, 5),
        ]),
        new(SecurityPolicyPage.ReAuth,
        [
            new(Ids.ReAuthMaxAge, PlaywrightAppFixture.ReAuthMaxAgeMinutes),
        ]),
        new(SecurityPolicyPage.FailureTracker,
        [
            new(Ids.FailureThreshold, 5),
            new(Ids.FailureWindowDays, 2),
        ]),
        new(SecurityPolicyPage.AdminApiRateLimit,
        [
            new(Ids.AdminApiPermit, 100),
            new(Ids.AdminApiWindow, 60),
            new(Ids.AdminApiSegments, 6),
        ]),
        new(SecurityPolicyPage.LoginRateLimit,
        [
            // PlaywrightAppFixture sætter LoginRateLimit__PermitLimit = 1000 (appsettings: 5).
            new(Ids.LoginPermit, 1000),
            new(Ids.LoginWindow, 60),
        ]),
        new(SecurityPolicyPage.ExportJob,
        [
            new(Ids.ExportRetention, 24),
            new(Ids.ExportMaxActive, 3),
        ]),
    ];

    [Theory]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task AlleFaner_ViserSeededeVaerdierOgGemmerSkaermbilleder(int width)
    {
        var user = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);
        var page = await fixture.LoginAndGotoAsync(user, E2ERoutes.AdminSecurityPolicies, width);

        await SmokePageAssertions.ExpectPageAsync(
            page, "Sikkerhedspolicies – MyGardenPlanner", "Sikkerhedspolicies");

        // Fanehop væk og tilbage beviser et forbundet kredsløb, før værdierne læses og siden fotograferes.
        await SecurityPolicyPage.OpenJitTabAsync(page);

        // En ny fane i siden uden forventning her skal opdages.
        await Assertions.Expect(page.GetByRole(AriaRole.Tab))
            .ToHaveCountAsync(SecurityPolicyPage.AllTabs.Count);

        foreach (var expectation in Expectations)
        {
            await SecurityPolicyPage.SelectTabAsync(page, expectation.Tab.Name);

            foreach (var field in expectation.Fields)
            {
                await Assertions.Expect(SecurityPolicyPage.Field(page, field.Id))
                    .ToHaveValueAsync(field.Value.ToString(CultureInfo.InvariantCulture));
            }

            await Assertions.Expect(SecurityPolicyPage.Error(page)).ToHaveCountAsync(0);
            await Assertions.Expect(page.Locator("[class*='skeleton']")).ToHaveCountAsync(0);

            await ScreenshotRecorder.CaptureAsync(
                page, Chapter, Checklist, TestName, $"G1_{expectation.Tab.Component}");
        }
    }
}