namespace MyGardenPlanner2026.Infrastructure.Services.AuditLog.ExportJobs;

using MyGardenPlanner2026.Core.Entities.Admin;

/// <summary>
/// Day-0 defaults for AuditLog-eksportjobs, bundet fra appsettings under "AuditLogExportJob".
/// RetentionHours, MaxActiveJobsPerUser og XlsxMaxRows er runtime-konfigurerbare via DB
/// (samme mønster som de øvrige sikkerhedspolicies).
/// </summary>
public sealed class AuditLogExportJobOptions
{
    public const string SectionName = "AuditLogExportJob";

    public int RetentionHours { get; set; } = 24;
    public int MaxActiveJobsPerUser { get; set; } = 3;
    public int PollIntervalSeconds { get; set; } = 10;
    public int StaleRunningMinutes { get; set; } = 30;
    public int MaxFileSizeMegabytes { get; set; } = 50;
    public int XlsxMaxRows { get; set; } = AuditLogExportJobPolicySettings.DefaultXlsxMaxRows;
}