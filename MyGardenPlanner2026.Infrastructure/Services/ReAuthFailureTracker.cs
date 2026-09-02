namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Standardimplementering af IReAuthFailureTracker. Skriver via IAdminDbContextFactory,
/// da ReAuthFailureAttempts ligger i admin-schema (samme mønster som JitElevationService/
/// RoleElevationExpirySweepService). SQLite-kompatibilitet: DateTimeOffset-vinduet
/// filtreres i hukommelsen efter en indekseret UserId-forespørgsel, IKKE direkte i SQL
/// (samme begrundelse som RoleElevationExpirySweepService — se dens doc-kommentar).
/// </summary>
public sealed class ReAuthFailureTracker(
    IAdminDbContextFactory contextFactory,
    IOptionsMonitor<ReAuthFailureTrackerOptions> policyOptionsMonitor,
    ISecurityAlertService securityAlertService,
    TimeProvider timeProvider) : IReAuthFailureTracker
{
    public async Task<bool> RecordFailureAsync(
        string userId, string? ipAddress, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();

        await context.ReAuthFailureAttempts.AddAsync(new ReAuthFailureAttempt
        {
            UserId = userId,
            OccurredAtUtc = now,
            IpAddress = ipAddress
        }, CancellationToken.None);
        await context.SaveChangesAsync(cancellationToken);

        var policy = policyOptionsMonitor.CurrentValue;
        var windowStart = now.AddDays(-policy.WindowDays);

        var attemptsForUser = await context.ReAuthFailureAttempts
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);

        var countWithinWindow = attemptsForUser.Count(a => a.OccurredAtUtc >= windowStart);

        if (countWithinWindow != policy.Threshold)
        {
            return false;
        }

        await securityAlertService.AlertFailedReAuthAsync(userId, ipAddress ?? "ukendt", cancellationToken);
        return true;
    }

    public async Task ClearFailuresAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var attempts = await context.ReAuthFailureAttempts
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);

        if (attempts.Count == 0)
        {
            return;
        }

        context.ReAuthFailureAttempts.RemoveRange(attempts);
        await context.SaveChangesAsync(cancellationToken);
    }
}