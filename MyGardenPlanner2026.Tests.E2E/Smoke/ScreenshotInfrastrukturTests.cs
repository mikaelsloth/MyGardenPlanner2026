namespace MyGardenPlanner2026.Tests.E2E.Smoke;

using FluentAssertions;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class ScreenshotInfrastrukturTests(PlaywrightAppFixture fixture)
{
    [Fact]
    public async Task CaptureAsync_MedViewport_GemmerPngMedForventetStiOgBredde()
    {
        var outputRoot = Path.Combine(Path.GetTempPath(), $"mgp-screenshots-{Guid.NewGuid():N}");

        try
        {
            var page = await fixture.NewPageAsync(Viewports.Mobile);
            await page.GotoAsync(fixture.RootUri);

            var path = await ScreenshotRecorder.CaptureAsync(
                page, "0", "Infrastruktur", "Verificering", "Forside", outputRoot);

            path.Should().Be(Path.Combine(outputRoot, "0", "Infrastruktur", "Verificering_Forside_375.png"));
            new FileInfo(path).Length.Should().BeGreaterThan(0);
        }
        finally
        {
            if (Directory.Exists(outputRoot))
            {
                Directory.Delete(outputRoot, recursive: true);
            }
        }
    }
}