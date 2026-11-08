namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Ændring af retention-/maks.-aktive/Xlsx-policyen for AuditLog-baggrundseksporter er en
/// følsom "core policy"-handling (§3.2) og kræver step-up re-autentificering
/// (se <see cref="PolicyEditorBase"/>).
/// </summary>
public partial class AuditLogExportJobPolicyEditor
{
    private const int MaxXlsxRows = IAuditLogExportService.HardRowLimit;

    [Inject]
    private IAuditLogExportJobPolicyAdminService AdminService { get; set; } = default!;

    [Inject]
    private ILogger<AuditLogExportJobPolicyEditor> Logger { get; set; } = default!;

    private AuditLogExportJobPolicyDto? settings;
    private int retentionHoursEdit;
    private int maxActiveEdit;
    private int xlsxMaxRowsEdit;

    protected override string SavedMessage => "AuditLog-eksportjob-policyen er opdateret og trådt i kraft.";

    [LoggerMessage(EventId = 1130, Level = LogLevel.Information, Message = "Opdatering af AuditLog-eksportjob-policy afvist: {Reason}")]
    static partial void ExportJobPolicySaveFailed(ILogger logger, string Reason);

    protected override void LogSaveRejected(string reason) => ExportJobPolicySaveFailed(Logger, reason);

    protected override async Task LoadAsync()
    {
        settings = await AdminService.GetAsync();
        retentionHoursEdit = settings.RetentionHours;
        maxActiveEdit = settings.MaxActiveJobsPerUser;
        xlsxMaxRowsEdit = settings.XlsxMaxRows;
    }

    protected override Task UpdateAsync(string userId) =>
        AdminService.UpdateAsync(new AuditLogExportJobPolicyDto(retentionHoursEdit, maxActiveEdit, xlsxMaxRowsEdit), userId);
}