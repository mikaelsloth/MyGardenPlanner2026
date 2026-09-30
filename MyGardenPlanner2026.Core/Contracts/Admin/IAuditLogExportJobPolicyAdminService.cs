namespace MyGardenPlanner2026.Core.Contracts.Admin;

public interface IAuditLogExportJobPolicyAdminService
{
    Task<AuditLogExportJobPolicyDto> GetAsync(CancellationToken cancellationToken = default);

    Task<AuditLogExportJobPolicyDto> UpdateAsync(
        AuditLogExportJobPolicyDto update, string updatedByUserId, CancellationToken cancellationToken = default);
}