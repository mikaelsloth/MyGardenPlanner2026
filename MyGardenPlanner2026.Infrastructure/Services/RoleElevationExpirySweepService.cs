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
                logger.LogError(ex, "Fejl under sweep af udløbne JIT-eskaleringer.");
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