namespace MyGardenPlanner2026.Tests.E2E.Screenshots;

using Microsoft.Playwright;

/// <summary>Faste viewport-størrelser til tjekliste-tests. Bredden bruges som nøgle (og i filnavne).</summary>
public static class Viewports
{
    public const int Mobile = 375;
    public const int Tablet = 768;
    public const int BelowBreakpoint = 939;
    public const int AtBreakpoint = 940;
    public const int Desktop = 1280;

    private static readonly (int Width, int Height)[] Presets =
    [
        (Mobile, 812),
        (Tablet, 1024),
        (BelowBreakpoint, 900),
        (AtBreakpoint, 900),
        (Desktop, 800),
    ];

    public static IReadOnlyList<int> All { get; } = [.. Presets.Select(p => p.Width)];

    public static ViewportSize For(int width)
    {
        foreach (var (Width, Height) in Presets)
        {
            if (Width == width)
            {
                return new ViewportSize { Width = Width, Height = Height };
            }
        }

        throw new ArgumentOutOfRangeException(
            nameof(width), width, "Ukendt viewport-bredde. Brug en konstant fra Viewports.");
    }
}