namespace MyGardenPlanner2026.Core.Entities.Common;

public static class GardenAccessLevelExtensions
{
    public static string ToDisplayName(this GardenAccessLevel level) => level switch
    {
        GardenAccessLevel.HaveArkitekt => "Have Arkitekt",
        GardenAccessLevel.BedDesigner => "Bed Designer",
        GardenAccessLevel.Planlaegger => "Planlægger",
        _ => level.ToString()
    };
}