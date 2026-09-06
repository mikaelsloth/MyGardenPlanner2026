namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Filformater understøttet ved eksport af AuditLog-søgeresultater.
/// </summary>
public enum AuditLogExportFormat
{
    Csv = 0,
    Json = 1,
    Xlsx = 2
}