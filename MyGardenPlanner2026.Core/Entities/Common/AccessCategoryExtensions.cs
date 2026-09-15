namespace MyGardenPlanner2026.Core.Entities.Common;

public static class AccessCategoryExtensions
{
    public static string ToDisplayName(this AccessCategory category) => category switch
    {
        AccessCategory.Administrator => "Administrator",
        AccessCategory.Editor => "Redaktør",
        AccessCategory.ViewerPlus => "Læser+",
        AccessCategory.Viewer => "Læser",
        _ => category.ToString()
    };
}