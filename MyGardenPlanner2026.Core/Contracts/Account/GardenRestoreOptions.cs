namespace MyGardenPlanner2026.Core.Contracts.Account;

/// <summary>Konfiguration for opbevaring af slettede haver (appsettings-sektionen "GardenRestore").</summary>
public sealed class GardenRestoreOptions
{
    public const string SectionName = "GardenRestore";

    /// <summary>Antal dage en slettet have kan gendannes, før den slettes fysisk (1-365).</summary>
    public int RetentionDays { get; set; } = 30;
}