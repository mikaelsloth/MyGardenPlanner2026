namespace MyGardenPlanner2026.Infrastructure.Services.Account;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Entities.Common;
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
/// RoleElevationExpirySweepService). ExecuteDelete går uden om interceptors, så oprydningen dokumenteres eksplicit i AuditLogs (R10.4).
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
        var expiryByGarden = expired.ToDictionary(t => t.GardenId, t => t.ExpiresUtc);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var candidateIds = await context.Gardens.IgnoreQueryFilters()
            .Where(g => gardenIds.Contains(g.Id) && g.IsDeleted)
            .Select(g => g.Id)
            .ToListAsync(cancellationToken);

        // Cascade fjerner havens medlemskaber, entitlements, invitationer og gendannelsesrækken.
        // IsDeleted-tjekket gentages, så en have, der netop er gendannet, aldrig slettes.
        if (candidateIds.Count > 0)
        {
            await context.Gardens.IgnoreQueryFilters()
                .Where(g => candidateIds.Contains(g.Id) && g.IsDeleted)
                .ExecuteDeleteAsync(cancellationToken);
        }

        List<Guid> survivors = candidateIds.Count == 0
            ? []
            : await context.Gardens.IgnoreQueryFilters()
                .Where(g => candidateIds.Contains(g.Id))
                .Select(g => g.Id)
                .ToListAsync(cancellationToken);

        var purgedIds = candidateIds.Except(survivors).ToList();

        // Rækker, der ikke blev fjernet via cascade, hørte til en have, der ikke var slettet.
        var stale = await context.PendingGardenRestores
            .Where(p => ticketIds.Contains(p.Id))
            .ExecuteDeleteAsync(cancellationToken);

        // R10.4: ExecuteDelete går uden om interceptors, så oprydningen dokumenteres eksplicit (aktør = system).
        context.AuditLogs.AddRange(purgedIds.Select(id => GardenLifecycleAudit.Entry(
            GardenLifecycleAudit.SystemActor, now, AuditAction.Delete, GardenLifecycleAudit.GardenEntity, id.ToString(),
            new { IsDeleted = true, RestoreExpiredUtc = expiryByGarden[id] })));

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        if (purgedIds.Count > 0)
        {
            ExpiredGardensPurged(logger, purgedIds.Count);
        }

        if (stale > 0)
        {
            StaleRestoreTicketsRemoved(logger, stale);
        }

        return expired.Count;
    }
}