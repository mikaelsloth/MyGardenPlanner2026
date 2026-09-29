namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>Filindholdet for et færdigt eksportjob, til download.</summary>
public sealed record AuditLogExportJobFileDto(string FileName, string ContentType, byte[] Content);