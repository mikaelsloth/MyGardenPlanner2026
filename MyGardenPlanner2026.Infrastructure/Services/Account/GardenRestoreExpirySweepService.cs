namespace MyGardenPlanner2026.Infrastructure.Services.Account;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Periodisk baggrundsjob der sletter haver permanent, når deres gendannelsesperiode er udløbet
/// (PendingGardenRestore.ExpiresUtc &lt; nu, se AccountDeletionService/R10.1b). Haven, ejerens
/// bevarede (soft-slettede) medlemskab/entitlement og selve gendannelsesrækken slettes fysisk via
/// cascade fra Gardens.
///
/// Sikkerhedsnet: haven slettes kun, hvis den stadig er markeret IsDeleted. En gendannelsesrække,
/// hvis have ikke er slettet (fx fordi haven blev gendannet, mens sweep'et kørte), fjernes uden
/// at røre haven. Gendannelse og sweep bruger samme grænse: ExpiresUtc == nu kan stadig gendannes.
///
/// SQLite-kompatibilitet: DateTimeOffset-sammenligning udføres IKKE i SQL. Gendannelsesrækkerne
/// materialiseres først og tidsvindue-filtreres i hukommelsen (samme mønster som
/// RoleElevationExpirySweepService). ExecuteDelete går uden om interceptors og skriver derfor ingen
/// AuditLog-rækker (R10.4 tilføjer audit).
/// </summary>
public sealed partial class GardenRestoreExpirySweepService(
    IDbContextFactory<PlannerDbContext> contextFactory,
    IOptionsMonitor<GardenRestoreOptions> restoreOptions,
    TimeProvider timeProvider,
    ILogger<GardenRestoreExpirySweepService> logger) : BackgroundService
{
    [LoggerMessage(EventId = 1165, Level = LogLevel.Information, Message = "Slettede {Count} have(r) permanent efter udløbet gendannelsesperiode.")]
    static partial void ExpiredGardensPurged(ILogger logger, int Count);

    [LoggerMessage(EventId = 1166, Level = LogLevel.Warning, Message = "Fjernede {Count} forældet(e) gendannelsesrække(r), hvor haven ikke var markeret som slettet. Haven blev ikke slettet.")]
    static partial void StaleRestoreTicketsRemoved(ILogger logger, int Count);

    [LoggerMessage(EventId = 1167, Level = LogLevel.Error, Message = "Fejl under oprydning af udløbne gendannelige haver.")]
    static partial void SweepFailed(ILogger logger, Exception ex);

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

            var interval = TimeSpan.FromMinutes(Math.Max(1, restoreOptions.CurrentValue.SweepIntervalMinutes));

            try
            {
                await Task.Delay(interval, timeProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    /// <summary>
    /// Kører sweep'et én gang. Offentlig og uafhængig af timeren, så den kan testes direkte.
    /// Returnerer antal udløbne gendannelsesrækker, der er behandlet.
    /// </summary>
    public async Task<int> SweepOnceAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();

        var tickets = await context.PendingGardenRestores
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var expired = tickets.Where(t => t.ExpiresUtc < now).ToList();
        if (expired.Count == 0)
        {
            return 0;
        }

        var gardenIds = expired.Select(t => t.GardenId).ToList();
        var ticketIds = expired.Select(t => t.Id).ToList();

        // Cascade fjerner havens medlemskaber, entitlements, invitationer og gendannelsesrækken.
        var purged = await context.Gardens.IgnoreQueryFilters()
            .Where(g => gardenIds.Contains(g.Id) && g.IsDeleted)
            .ExecuteDeleteAsync(cancellationToken);

        // Rækker, der ikke blev fjernet via cascade, hørte til en have, der ikke var slettet.
        var stale = await context.PendingGardenRestores
            .Where(p => ticketIds.Contains(p.Id))
            .ExecuteDeleteAsync(cancellationToken);

        if (purged > 0)
        {
            ExpiredGardensPurged(logger, purged);
        }

        if (stale > 0)
        {
            StaleRestoreTicketsRemoved(logger, stale);
        }

        return expired.Count;
    }
}