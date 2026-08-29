namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Periodisk baggrundsjob der markerer udløbne, godkendte JIT-eskaleringer som Expired.
/// Rent datahygiejne/audit-formål: den faktiske adgangskontrol sker allerede i
/// JitElevationService.HasActiveElevationAsync, som tjekker ValidToUtc direkte mod
/// TimeProvider ved hvert kald, uafhængigt af Status. Sweep'et sikrer blot, at
/// Status-feltet i admin.RoleElevationRequests korrekt afspejler Expired i UI/rapportering.
///
/// Bruger normal SaveChangesAsync (IKKE ExecuteUpdateAsync), så AuditLoggingInterceptor
/// korrekt registrerer status-overgangen — se doc-kommentar på RoleElevationRequest.
///
/// SQLite-kompatibilitet: DateTimeOffset-sammenligning (ValidToUtc &lt; now) udføres
/// IKKE i SQL (fejler på SQLite, se PlannerDbContextTests-memory). Approved-poster
/// materialiseres først via ToListAsync, tidsvindue-filtreres derefter i hukommelsen —
/// samme mønster som JitElevationService.HasActiveElevationAsync.
/// </summary>
public sealed partial class RoleElevationExpirySweepService(
    IAdminDbContextFactory contextFactory,
    IOptions<JitElevationPolicyOptions> policyOptions,
    TimeProvider timeProvider,
    ILogger<RoleElevationExpirySweepService> logger) : BackgroundService
{
    [LoggerMessage(EventId = 1012, Level = LogLevel.Information, Message = "Markerede {Count} udløbne JIT-eskaleringer som Expired.")]
    static partial void JitEscalationExpired(ILogger logger, int Count);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(Math.Max(1, policyOptions.Value.SweepIntervalMinutes));

        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                await SweepOnceAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Fejl under sweep af udløbne JIT-eskaleringer.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    /// <summary>
    /// Kører sweep'et én gang. Offentlig og uafhængig af timeren, så den kan testes direkte.
    /// </summary>
    public async Task<int> SweepOnceAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();

        var approvedRequests = await context.RoleElevationRequests
            .Where(r => r.Status == RoleElevationStatus.Approved)
            .ToListAsync(cancellationToken);

        var expired = approvedRequests.Where(r => r.ValidToUtc < now).ToList();

        if (expired.Count == 0)
        {
            return 0;
        }

        foreach (var request in expired)
        {
            request.Status = RoleElevationStatus.Expired;
        }

        await context.SaveChangesAsync(cancellationToken);

        JitEscalationExpired(logger, expired.Count);

        return expired.Count;
    }
}