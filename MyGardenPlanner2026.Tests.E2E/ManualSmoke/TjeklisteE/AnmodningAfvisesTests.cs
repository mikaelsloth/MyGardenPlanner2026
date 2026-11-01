namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteE;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste E6: anmodning afvises, hvis ansøger allerede er direkte medlem af rollen eller har en
/// afventende anmodning om den. Ny AuditViewer anmoder om AuditViewer (medlem) og afvises. Anmodning om
/// SystemAdmin accepteres; en ny anmodning om SystemAdmin afvises som dublet. Kun den første række findes.
/// Skærmbilleder: JIT_E6_Fejl_&lt;bredde&gt; (efter dublet-afvisning).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class AnmodningAfvisesTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste E";
    private const string TestName = "JIT";

    private const int Minutes = 30;

    [Theory]
    [InlineData(Viewports.Mobile)]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task Anmodning_AfvisesNaarAnsoegerErMedlemEllerHarAabenAnmodning(int width)
    {
        var user = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var page = await JitRequestsPage.OpenAsync(fixture, user, width);

        var memberReason = JitRequestsPage.NewReason();
        var firstReason = JitRequestsPage.NewReason();
        var duplicateReason = JitRequestsPage.NewReason();

        // a. Allerede medlem af rollen.
        await JitRequestsPage.SubmitRequestAsync(page, RoleNames.AuditViewer, Minutes, memberReason);
        await Assertions.Expect(JitRequestsPage.Error(page))
            .ToContainTextAsync($"Du er allerede medlem af rollen '{RoleNames.AuditViewer}'.");

        // b. Kontrol: en rolle, brugeren ikke har, accepteres.
        await JitRequestsPage.RequestAndWaitAsync(page, RoleNames.SystemAdmin, Minutes, firstReason);

        // c. Dublet: afventende anmodning om samme rolle.
        await JitRequestsPage.SubmitRequestAsync(page, RoleNames.SystemAdmin, Minutes, duplicateReason);
        await Assertions.Expect(JitRequestsPage.Error(page))
            .ToContainTextAsync($"Du har allerede en afventende eller aktiv anmodning om rollen '{RoleNames.SystemAdmin}'.");

        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "E6_Fejl");

        // Kun den første række findes (b beviser, at listen er indlæst).
        await JitRequestsPage.ShowHistoryAsync(page);
        await Assertions.Expect(JitRequestsPage.RequestRow(page, firstReason)).ToBeVisibleAsync();
        await Assertions.Expect(JitRequestsPage.RequestRow(page, memberReason)).ToHaveCountAsync(0);
        await Assertions.Expect(JitRequestsPage.RequestRow(page, duplicateReason)).ToHaveCountAsync(0);
    }
}