namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Eksporterer AuditLog-søgeresultater (samme filter som visningen, uden
/// paginering) til CSV, JSON eller Xlsx. Hård grænse på HardRowLimit rækker —
/// overskrides denne, kastes InvalidOperationException uden at skrive noget
/// til destination. Kaldere med mere end 50.000 rækker bør indhente eksplicit
/// brugerbekræftelse FØR dette kald (håndhæves i UI-laget, ikke her).
/// </summary>
public interface IAuditLogExportService
{
    const int HardRowLimit = 250_000;

    Task ExportAsync(
        AuditLogFilterDto filter,
        AuditLogExportFormat format,
        Stream destination,
        CancellationToken cancellationToken = default);
}