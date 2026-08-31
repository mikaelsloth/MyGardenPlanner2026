namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Configuration.RateLimiting;
using System.Threading.RateLimiting;

public static class RateLimitingServicesExtensions
{
    /// <summary>Label brugt i logs — IKKE en navngivet policy, se PR-beskrivelse for begrundelse.</summary>
    public const string AdminAuthPolicyName = "AdminAuthPolicy";

    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
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
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });

            options.OnRejected = (context, cancellationToken) =>
            {
                var loggerFactory = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("MyGardenPlanner2026.RateLimiting");

                var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                // TODO (PR3): kald ISecurityAlertService.AlertFailedReAuthAsync herfra ved gentagne afvisninger.
                logger.LogWarning(
                    "{Policy}: rate limit overskredet for IP {IpAddress} på {Path}.",
                    AdminAuthPolicyName, ipAddress, context.HttpContext.Request.Path);

                return ValueTask.CompletedTask;
            };
        });

        return services;
    }
}