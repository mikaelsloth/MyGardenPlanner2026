namespace MyGardenPlanner2026.Core.Contracts.Admin;

public sealed record AuditLogExportJobPolicyDto(int RetentionHours, int MaxActiveJobsPerUser);