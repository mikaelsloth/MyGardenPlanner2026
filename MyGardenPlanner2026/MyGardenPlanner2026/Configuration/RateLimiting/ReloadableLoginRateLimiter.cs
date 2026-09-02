namespace MyGardenPlanner2026.Configuration.RateLimiting;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Infrastructure.Services;
using System.Threading.RateLimiting;

/// <summary>
/// PartitionedRateLimiter&lt;HttpContext&gt;-wrapper til det globale login rate limit
/// (§4.1). RateLimiterOptions.GlobalLimiter tildeles ÉN gang ved opstart (i
/// AddRateLimiter's configure-callback), så rebuild-mekanikken (samme mønster som
/// AdminActionRateLimiter) ligger INDE i limiter-instansen frem for i DI-registreringen.
/// Abonnerer på IOptionsMonitor.OnChange: en ændring i admin-UI bygger en ny intern
/// limiter og bytter den atomisk ind, mens den gamle disposes.
/// </summary>
public sealed class ReloadableLoginRateLimiter : PartitionedRateLimiter<HttpContext>
{
    private readonly IDisposable? _changeSubscription;
    private PartitionedRateLimiter<HttpContext> _inner;

    public ReloadableLoginRateLimiter(IOptionsMonitor<LoginRateLimitOptions> optionsMonitor)
    {
        _inner = BuildLimiter(optionsMonitor.CurrentValue);
        _changeSubscription = optionsMonitor.OnChange(policy =>
        {
            var newInner = BuildLimiter(policy);
            var oldInner = Interlocked.Exchange(ref _inner, newInner);
            oldInner.Dispose();
        });
    }

    protected override RateLimitLease AttemptAcquireCore(HttpContext resourceID, int permitCount) =>
        Volatile.Read(ref _inner).AttemptAcquire(resourceID, permitCount);

    protected override ValueTask<RateLimitLease> AcquireAsyncCore(
        HttpContext resourceID, int permitCount, CancellationToken cancellationToken) =>
        Volatile.Read(ref _inner).AcquireAsync(resourceID, permitCount, cancellationToken);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _changeSubscription?.Dispose();
            _inner.Dispose();
        }

        base.Dispose(disposing);
    }

    private static PartitionedRateLimiter<HttpContext> BuildLimiter(LoginRateLimitOptions policy) =>
        PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            var isProtected = AdminAuthPathMatcher.IsProtectedAuthRequest(
                httpContext.Request.Method, httpContext.Request.Path.Value ?? string.Empty);

            if (!isProtected)
            {
                return RateLimitPartition.GetNoLimiter("unrestricted");
            }

            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = policy.PermitLimit,
                Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            });
        });

    public override RateLimiterStatistics? GetStatistics(HttpContext resource)
    {
        return null;
    }
}