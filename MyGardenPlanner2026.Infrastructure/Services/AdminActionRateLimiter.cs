namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Threading.RateLimiting;

/// <summary>
/// Singleton-implementering af IAdminActionRateLimiter. Partitionerer via en enkelt,
/// delt PartitionedRateLimiter&lt;string&gt; nøglet på userId — deles på tværs af alle
/// Blazor-circuits, så kvoten IKKE nulstilles ved reconnect (i modsætning til Scoped
/// tilstand som fx IReAuthenticationService).
/// </summary>
public sealed class AdminActionRateLimiter : IAdminActionRateLimiter, IDisposable
{
    private readonly PartitionedRateLimiter<string> _limiter;

    public AdminActionRateLimiter(IOptions<AdminApiRateLimitOptions> options)
    {
        var policy = options.Value;

        _limiter = PartitionedRateLimiter.Create<string, string>(userId =>
            RateLimitPartition.GetSlidingWindowLimiter(userId, _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = policy.PermitLimit,
                Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                SegmentsPerWindow = policy.SegmentsPerWindow,
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    }

    public async Task<bool> TryAcquireAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        using var lease = await _limiter.AcquireAsync(userId, permitCount: 1, cancellationToken);
        return lease.IsAcquired;
    }

    public void Dispose() => _limiter.Dispose();
}