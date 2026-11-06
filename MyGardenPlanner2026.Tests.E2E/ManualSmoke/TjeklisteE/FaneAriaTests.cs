namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteE;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// R6.2: fanerne på /admin/jit-requests renderer aria-selected som "true"/"false"
/// (tidligere tom attribut ved true og ingen attribut ved false).
/// Ikke en tjekliste-test. Ingen skærmbilleder. Frisk AuditViewer pr. test (egen circuit/session).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class FaneAriaTests(PlaywrightAppFixture fixture)
{
    [Fact]
    public async Task Faner_RendererAriaSelectedSomTrueOgFalse()
    {
        var user = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var page = await JitRequestsPage.OpenAsync(fixture, user, Viewports.Desktop);

        // OpenAsync slutter på "Anmod om adgang".
        await Assertions.Expect(JitRequestsPage.Tab(page, JitRequestsPage.RequestTabName))
            .ToHaveAttributeAsync("aria-selected", "true");
        await Assertions.Expect(JitRequestsPage.Tab(page, JitRequestsPage.ApproveTabName))
            .ToHaveAttributeAsync("aria-selected", "false");

        await JitRequestsPage.SelectTabAsync(page, JitRequestsPage.ApproveTabName);

        await Assertions.Expect(JitRequestsPage.Tab(page, JitRequestsPage.ApproveTabName))
            .ToHaveAttributeAsync("aria-selected", "true");
        await Assertions.Expect(JitRequestsPage.Tab(page, JitRequestsPage.RequestTabName))
            .ToHaveAttributeAsync("aria-selected", "false");
    }
}