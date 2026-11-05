namespace MyGardenPlanner2026.Infrastructure.Services.AuditLog.ExportJobs;

/// <summary>Kastes af <see cref="SizeLimitedStream"/>, når en skrivning ville få filen over grænsen.</summary>
public sealed class ExportSizeLimitExceededException(long limitBytes)
    : Exception($"Eksporten overstiger den tilladte størrelse på {limitBytes} bytes.")
{
    public long LimitBytes { get; } = limitBytes;
}