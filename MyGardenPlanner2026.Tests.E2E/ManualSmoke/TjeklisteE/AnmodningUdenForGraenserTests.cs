namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteE;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste E1: anmodning uden for JitElevationPolicy-grænser afvises. Standardpolicy er 30-90 minutter
/// (appsettings). Ny AuditViewer anmoder om SystemAdmin med 29 og 91 minutter: begge afvises med
/// fejlen fra JitElevationService, og ingen række oprettes. Kontrol: 30 minutter accepteres, så
/// afvisningen skyldes grænserne. Skærmbilleder: JIT_E1_Fejl_&lt;bredde&gt; (efter 91 minutter).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class AnmodningUdenForGraenserTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste E";
    private const string TestName = "JIT";

    private const int BelowMin = 29;
    private const int AboveMax = 91;
    private const int WithinLimits = 30;

    private const string OutOfRangeText = "Error: RequestedMinutes skal være mellem 30 og 90.";

    [Theory]
    [InlineData(Viewports.Mobile)]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task AnmodningUdenForGraenser_AfvisesOgGemmerSkaermbillede(int width)
    {
        var user = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var page = await JitRequestsPage.OpenAsync(fixture, user, width);

        var belowReason = JitRequestsPage.NewReason();
        var aboveReason = JitRequestsPage.NewReason();
        var okReason = JitRequestsPage.NewReason();

        await JitRequestsPage.SubmitRequestAsync(page, RoleNames.SystemAdmin, BelowMin, belowReason);
        await ExpectOutOfRangeAsync(page, BelowMin);

        await JitRequestsPage.SubmitRequestAsync(page, RoleNames.SystemAdmin, AboveMax, aboveReason);
        await ExpectOutOfRangeAsync(page, AboveMax);

        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "E1_Fejl");

        // Kontrol: inden for grænserne accepteres. Rækken beviser også, at listen er indlæst,
        // før der assertes på fravær af de afviste rækker.
        await JitRequestsPage.RequestAndWaitAsync(page, RoleNames.SystemAdmin, WithinLimits, okReason);
        await Assertions.Expect(JitRequestsPage.StatusBadge(JitRequestsPage.RequestRow(page, okReason)))
            .ToHaveTextAsync(JitRequestsPage.PendingLabel);
        await Assertions.Expect(JitRequestsPage.Error(page)).ToHaveCountAsync(0);

        await JitRequestsPage.ShowHistoryAsync(page);

        await Assertions.Expect(JitRequestsPage.RequestRow(page, belowReason)).ToHaveCountAsync(0);
        await Assertions.Expect(JitRequestsPage.RequestRow(page, aboveReason)).ToHaveCountAsync(0);
    }

    /// <summary>
    /// Fejlen indeholder den afviste værdi ("Actual value was N."). Den adskiller 2. fra 1. fejl,
    /// så testen ikke består på den forrige, endnu synlige fejlbesked.
    /// </summary>
    private static async Task ExpectOutOfRangeAsync(IPage page, int minutes)
    {
        var error = JitRequestsPage.Error(page);

        await Assertions.Expect(error).ToContainTextAsync(OutOfRangeText);
        await Assertions.Expect(error).ToContainTextAsync($"Actual value was {minutes}");
    }
}