namespace MyGardenPlanner2026.Infrastructure.Data.Seed;

public sealed class InitialAdminOptions
{
    public const string SectionName = "InitialAdmin";

    public string? Email { get; set; }
    public string? Password { get; set; }
}