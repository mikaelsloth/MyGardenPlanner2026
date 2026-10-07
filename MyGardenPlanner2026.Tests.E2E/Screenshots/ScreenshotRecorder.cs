namespace MyGardenPlanner2026.Tests.E2E.Screenshots;

using Microsoft.Playwright;

/// <summary>
/// Gemmer fuldside-skærmbilleder under TestResults/Screenshots/&lt;kapitel&gt;/&lt;tjekliste&gt;/
/// som &lt;testnavn&gt;_&lt;komponent&gt;_&lt;bredde&gt;.png. Bredden læses fra sidens viewport.
/// </summary>
public static class ScreenshotRecorder
{
    private static readonly char[] InvalidNameChars =
        [.. Path.GetInvalidFileNameChars(), '\\', ':', '*', '?', '"', '<', '>', '|'];

    public static string DefaultOutputRoot() =>
        Path.Combine(RepoPaths.FindRepoRoot(), "TestResults", "Screenshots");

    public static async Task<string> CaptureAsync(
        IPage page, string chapter, string checklist, string testName, string component,
        string? outputRoot = null)
    {
        var width = page.ViewportSize?.Width
            ?? throw new InvalidOperationException(
                "Siden har ingen viewport-størrelse. Opret den via PlaywrightAppFixture.NewPageAsync(width).");

        var path = BuildPath(outputRoot ?? DefaultOutputRoot(), chapter, checklist, testName, component, width);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
        return path;
    }

    public static string BuildPath(
        string outputRoot, string chapter, string checklist, string testName, string component, int width) =>
        Path.Combine(
            outputRoot,
            Sanitize(chapter, nameof(chapter)),
            Sanitize(checklist, nameof(checklist)),
            $"{Sanitize(testName, nameof(testName))}_{Sanitize(component, nameof(component))}_{width}.png");

    private static string Sanitize(string value, string parameterName)
    {
        var cleaned = new string([.. value.Select(c => InvalidNameChars.Contains(c) ? '_' : c)])
            .Trim()
            .TrimEnd('.');

        return cleaned.Length == 0
            ? throw new ArgumentException(
                "Må ikke være tom eller kun bestå af mellemrum/punktummer.", parameterName)
            : cleaned;
    }
}