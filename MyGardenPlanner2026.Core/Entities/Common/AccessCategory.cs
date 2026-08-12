namespace MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Adgangskategori (rettighedsniveau) inden for et GardenAccessLevel.
/// Ordnet efter privilegie: Viewer &lt; ViewerPlus &lt; Editor &lt; Administrator.
/// </summary>
public enum AccessCategory
{
    Viewer = 0,
    ViewerPlus = 1,
    Editor = 2,
    Administrator = 3
}