namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.Services.Jit;

/// <summary>
/// Periodisk baggrundsjob der (1) markerer udløbne, godkendte JIT-eskaleringer som Expired og
/// (2) markerer ventende anmodninger, der har stået ubehandlet længere end PendingExpiryHours, som Cancelled.
/// Den faktiske adgangskontrol sker uafhængigt af Status i JitElevationService (ValidToUtc tjekkes direkte,
/// og udløbne Pending afvises ved godkend/afvis). Sweep'et sikrer, at Status-feltet i
/// admin.RoleElevationRequests afspejler virkeligheden i UI/rapportering.
///
/// Bruger normal SaveChangesAsync (IKKE ExecuteUpdateAsync), så AuditLoggingInterceptor
/// korrekt registrerer status-overgangen — se doc-kommentar på RoleElevationRequest.
///
/// SQLite-kompatibilitet: DateTimeOffset-sammenligning (ValidToUtc &lt; now) udføres
/// IKKE i SQL (fejler på SQLite, se PlannerDbContextTests-memory). Approved-poster
/// materialiseres først via ToListAsync, tidsvindue-filtreres derefter i hukommelsen —
/// samme mønster som JitElevationService.HasActiveElevationAsync.
///
/// SweepIntervalMinutes er runtime-konfigurerbar (§3.2, admin-UI). I stedet for en fast
/// PeriodicTimer bruges Task.Delay, der afbrydes med det samme via
/// IOptionsMonitor.OnChange, hvis intervallet ændres, mens jobbet venter — så en
/// forkortet ventetid træder i kraft uden at vente den gamle periode ud først.
/// </summary>
public sealed partial class RoleElevationExpirySweepService(
    IAdminDbContextFactory contextFactory,
    IOptionsMonitor<JitElevationPolicyOptions> policyOptionsMonitor,
    TimeProvider timeProvider,
    ILogger<RoleElevationExpirySweepService> logger) : BackgroundService
{
    [LoggerMessage(EventId = 1012, Level = LogLevel.Information, Message = "Markerede {Count} udløbne JIT-eskaleringer som Expired.")]
    static partial void JitEscalationExpired(ILogger logger, int Count);

    [LoggerMessage(EventId = 1022, Level = LogLevel.Error, Message = "Fejl under sweep af udløbne JIT-eskaleringer.")]
    static partial void SweepFailed(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 1146, Level = LogLevel.Information, Message = "Markerede {Count} ventende JIT-anmodninger som Cancelled (ubehandlet i over {Hours} timer).")]
    static partial void JitPendingRequestsCancelled(ILogger logger, int Count, double Hours);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SweepOnceAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                SweepFailed(logger, ex);
            }

            await WaitForNextSweepAsync(stoppingToken);
        }
    }

    /// <summary>
    /// Venter til næste sweep. Offentlig og uafhængig af ExecuteAsync-løkken, så
    /// reload-adfærden (afbrydes øjeblikkeligt ved ændret SweepIntervalMinutes) kan
    /// testes direkte — samme begrundelse som SweepOnceAsync er public.
    /// </summary>
    public async Task WaitForNextSweepAsync(CancellationToken cancellationToken = default)
    {
        using var intervalChangedSource = new CancellationTokenSource();
        using var changeSubscription = policyOptionsMonitor.OnChange(_ => intervalChangedSource.Cancel());
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, intervalChangedSource.Token);

        var interval = TimeSpan.FromMinutes(Math.Max(1, policyOptionsMonitor.CurrentValue.SweepIntervalMinutes));

        try
        {
            await Task.Delay(interval, linkedSource.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Intervallet blev ændret i admin-UI — ventetiden afbrydes med vilje, så
            // den nye værdi træder i kraft ved næste iteration.
        }
    }

    /// <summary>
    /// Kører sweep'et én gang. Offentlig og uafhængig af timeren, så den kan testes direkte.
    /// Returnerer antal ændrede anmodninger (Approved → Expired plus Pending → Cancelled).
    /// </summary>
    public async Task<int> SweepOnceAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();
        var pendingExpiry = policyOptionsMonitor.CurrentValue.PendingExpiry;

        var openRequests = await context.RoleElevationRequests
            .Where(r => r.Status == RoleElevationStatus.Approved || r.Status == RoleElevationStatus.Pending)
            .ToListAsync(cancellationToken);

        var expired = openRequests
            .Where(r => r.Status == RoleElevationStatus.Approved && r.ValidToUtc < now)
            .ToList();
        var cancelled = openRequests
            .Where(r => r.Status == RoleElevationStatus.Pending && r.CreatedAtUtc.Add(pendingExpiry) < now)
            .ToList();

        if (expired.Count == 0 && cancelled.Count == 0)
        {
            return 0;
        }

        foreach (var request in expired)
        {
            request.Status = RoleElevationStatus.Expired;
        }

        foreach (var request in cancelled)
        {
            request.Status = RoleElevationStatus.Cancelled;
        }

        await context.SaveChangesAsync(cancellationToken);

        if (expired.Count > 0)
        {
            JitEscalationExpired(logger, expired.Count);
        }

        if (cancelled.Count > 0)
        {
            JitPendingRequestsCancelled(logger, cancelled.Count, pendingExpiry.TotalHours);
        }

        return expired.Count + cancelled.Count;
    }
}