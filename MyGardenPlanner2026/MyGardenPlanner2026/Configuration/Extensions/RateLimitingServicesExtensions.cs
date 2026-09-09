namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Configuration.RateLimiting;

public static partial class RateLimitingServicesExtensions
{
    /// <summary>Label brugt i logs — IKKE en navngivet policy, se PR-beskrivelse for begrundelse.</summary>
    public const string AdminAuthPolicyName = "AdminAuthPolicy";

    [LoggerMessage(EventId = 1023, Level = LogLevel.Warning, Message = "{Policy}: rate limit overskredet for IP {IpAddress} på {Path}.")]
    static partial void RateLimitExceeded(ILogger logger, string Policy, string IpAddress, string Path);

    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
    {
        services.AddSingleton<ReloadableLoginRateLimiter>();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = (context, cancellationToken) =>
            {
                var loggerFactory = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("MyGardenPlanner2026.RateLimiting");

                var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                RateLimitExceeded(logger, AdminAuthPolicyName, ipAddress, context.HttpContext.Request.Path.Value ?? string.Empty);

                return ValueTask.CompletedTask;
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