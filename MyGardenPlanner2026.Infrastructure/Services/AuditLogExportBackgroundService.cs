namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Poller efter Pending-eksportjobs. Én DI-scope pr. job (processoren er Scoped, da
/// IAuditLogExportService/IAuditLogQueryService er Scoped). Fejl i en poll-runde logges
/// og stopper aldrig løkken — fx hvis migrationen endnu ikke er kørt.
/// </summary>
public sealed partial class AuditLogExportBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<AuditLogExportJobOptions> optionsMonitor,
    ILogger<AuditLogExportBackgroundService> logger) : BackgroundService
{
    [LoggerMessage(EventId = 1127, Level = LogLevel.Error, Message = "Fejl under poll af AuditLog-eksportjobs.")]
    static partial void PollFailed(ILogger logger, Exception ex);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                PollFailed(logger, ex);
            }

            try
            {
                var interval = TimeSpan.FromSeconds(Math.Max(1, optionsMonitor.CurrentValue.PollIntervalSeconds));
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    /// <summary>
    /// Én poll-runde: rydder hængende jobs og behandler derefter Pending-jobs, til der
    /// ikke er flere. Offentlig, så den kan testes uden timer. Returnerer antal behandlede jobs.
    /// </summary>
    public async Task<int> RunOnceAsync(CancellationToken cancellationToken = default)
    {
        await using (var recoveryScope = scopeFactory.CreateAsyncScope())
        {
            var recovery = recoveryScope.ServiceProvider.GetRequiredService<IAuditLogExportJobProcessor>();
            await recovery.FailStaleRunningJobsAsync(cancellationToken);
            await recovery.DeleteExpiredJobsAsync(cancellationToken);
        }

        var processed = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<IAuditLogExportJobProcessor>();

            if (!await processor.ProcessNextAsync(cancellationToken))
            {
                break;
            }

            processed++;
        }

        return processed;
    }
}