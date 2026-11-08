namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Configuration.RateLimiting;
using System.Globalization;
using System.Threading.RateLimiting;

public static partial class RateLimitingServicesExtensions
{
    /// <summary>Label brugt i logs — IKKE en navngivet policy, se PR-beskrivelse for begrundelse.</summary>
    public const string AdminAuthPolicyName = "AdminAuthPolicy";
    /// <summary>Brugerrettet besked ved overskredet login rate limit (429).</summary>
    public const string LoginRateLimitedMessage = "For mange loginforsøg. Vent et øjeblik og prøv igen.";

    [LoggerMessage(EventId = 1023, Level = LogLevel.Warning, Message = "{Policy}: rate limit overskredet for IP {IpAddress} på {Path}.")]
    static partial void RateLimitExceeded(ILogger logger, string Policy, string IpAddress, string Path);

    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
    {
        services.AddSingleton<ReloadableLoginRateLimiter>();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;
                var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("MyGardenPlanner2026.RateLimiting");

                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                RateLimitExceeded(logger, AdminAuthPolicyName, ipAddress, httpContext.Request.Path.Value ?? string.Empty);

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    httpContext.Response.Headers.RetryAfter =
                        Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                httpContext.Response.ContentType = "text/plain; charset=utf-8";
                await httpContext.Response.WriteAsync(LoginRateLimitedMessage, cancellationToken);
            };
        });

        // GlobalLimiter sættes via en separat Configure-registrering, så
        // ReloadableLoginRateLimiter (der selv abonnerer på policy-ændringer) kan
        // injiceres fra DI i stedet for at blive bygget statisk i configure-callbacken.
        services.AddOptions<RateLimiterOptions>()
            .Configure<ReloadableLoginRateLimiter>((options, limiter) => options.GlobalLimiter = limiter);

        return services;
    }
}