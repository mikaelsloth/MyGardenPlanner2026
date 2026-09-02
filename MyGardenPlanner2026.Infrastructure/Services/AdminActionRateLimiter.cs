namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Threading.RateLimiting;

/// <summary>
/// Singleton-implementering af IAdminActionRateLimiter. Partitionerer via en enkelt,
/// delt PartitionedRateLimiter&lt;string&gt; nøglet på userId — deles på tværs af alle
/// Blazor-circuits, så kvoten IKKE nulstilles ved reconnect (i modsætning til Scoped
/// tilstand som fx IReAuthenticationService).
///
/// PermitLimit/WindowSeconds/SegmentsPerWindow er runtime-konfigurerbare (§3.2,
/// admin-UI). Da PartitionedRateLimiter ikke understøtter at ændre sine grænser efter
/// oprettelse, abonneres der på IOptionsMonitor.OnChange: en ændring bygger en ny intern
/// limiter og bytter den atomisk ind (Interlocked.Exchange), mens den gamle disposes.
/// </summary>
public sealed class AdminActionRateLimiter : IAdminActionRateLimiter, IDisposable
{
    private readonly IDisposable? _changeSubscription;
    private PartitionedRateLimiter<string> _limiter;

    public AdminActionRateLimiter(IOptionsMonitor<AdminApiRateLimitOptions> optionsMonitor)
    {
        _limiter = BuildLimiter(optionsMonitor.CurrentValue);
        _changeSubscription = optionsMonitor.OnChange(policy =>
        {
            var newLimiter = BuildLimiter(policy);
            var oldLimiter = Interlocked.Exchange(ref _limiter, newLimiter);
            oldLimiter.Dispose();
        });
    }

    public async Task<bool> TryAcquireAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        using var lease = await Volatile.Read(ref _limiter).AcquireAsync(userId, permitCount: 1, cancellationToken);
        return lease.IsAcquired;
    }

    public void Dispose()
    {
        _changeSubscription?.Dispose();
        _limiter.Dispose();
    }

    private static PartitionedRateLimiter<string> BuildLimiter(AdminApiRateLimitOptions policy) =>
        PartitionedRateLimiter.Create<string, string>(userId =>
            RateLimitPartition.GetSlidingWindowLimiter(userId, _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = policy.PermitLimit,
                Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                SegmentsPerWindow = policy.SegmentsPerWindow,
                QueueLimit = 0,
                AutoReplenishment = true
            }));
}