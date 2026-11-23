namespace MyGardenPlanner2026.Core.Entities.Admin;

/// <summary>
/// En række i Data Protection-nøgleringen (admin.DataProtectionKeys). Xml er nøgle-XML'en (krypteret med
/// certifikat i Production). Implementerer bevidst IKKE ISoftDelete, så AuditLoggingInterceptor aldrig
/// logger nøglemateriale i AuditLogs. Indeholder ingen persondata.
/// </summary>
public class DataProtectionKeyRecord
{
    public int Id { get; set; }
    public string? FriendlyName { get; set; }
    public string Xml { get; set; } = string.Empty;
}