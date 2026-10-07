namespace MyGardenPlanner2026.Tests.E2E;

/// <summary>Finder repo-roden (mappen med .slnx) opad fra testoutput-mappen.</summary>
internal static class RepoPaths
{
    public static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && dir.GetFiles("*.slnx").Length == 0)
        {
            dir = dir.Parent;
        }

        return dir?.FullName
            ?? throw new InvalidOperationException(
                "Kunne ikke finde repo-roden (ingen .slnx fundet opad fra testoutput-mappen).");
    }
}