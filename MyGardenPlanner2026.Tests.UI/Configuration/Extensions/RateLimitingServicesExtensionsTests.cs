namespace MyGardenPlanner2026.Tests.UI.Configuration.Extensions;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Configuration.Extensions;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using Xunit;

public class RateLimitingServicesExtensionsTests
{
    private static async Task<DefaultHttpContext> InvokeOnRejectedAsync(RateLimitLease lease, CancellationToken cancellationToken)
    {
        var services = new ServiceCollection();
        services.AddRateLimitingServices();
        services.AddLogging();

        await using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value;

        var httpContext = new DefaultHttpContext { RequestServices = provider };
        httpContext.Request.Method = "POST";
        httpContext.Request.Path = "/Account/Login";
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
        httpContext.Response.Body = new MemoryStream();

        await options.OnRejected!(new OnRejectedContext { HttpContext = httpContext, Lease = lease }, cancellationToken);

        return httpContext;
    }

    private static string ReadBody(HttpContext httpContext)
    {
        httpContext.Response.Body.Position = 0;
        return new StreamReader(httpContext.Response.Body, Encoding.UTF8).ReadToEnd();
    }

    [Fact]
    public async Task OnRejected_WritesDanishPlainTextBody()
    {
        var httpContext = await InvokeOnRejectedAsync(new FailedLease(null), TestContext.Current.CancellationToken);

        httpContext.Response.ContentType.Should().StartWith("text/plain");
        ReadBody(httpContext).Should().Be("For mange loginforsøg. Vent et øjeblik og prøv igen.");
    }

    [Fact]
    public async Task OnRejected_WithRetryAfterMetadata_SetsRetryAfterHeaderRoundedUp()
    {
        var httpContext = await InvokeOnRejectedAsync(
            new FailedLease(TimeSpan.FromSeconds(59.2)), TestContext.Current.CancellationToken);

        httpContext.Response.Headers.RetryAfter.ToString().Should().Be("60");
    }

    [Fact]
    public async Task OnRejected_WithoutRetryAfterMetadata_OmitsRetryAfterHeader()
    {
        var httpContext = await InvokeOnRejectedAsync(new FailedLease(null), TestContext.Current.CancellationToken);

        httpContext.Response.Headers.ContainsKey("Retry-After").Should().BeFalse();
    }

    private sealed class FailedLease(TimeSpan? retryAfter) : RateLimitLease
    {
        public override bool IsAcquired => false;

        public override IEnumerable<string> MetadataNames =>
            retryAfter is null ? [] : [MetadataName.RetryAfter.Name];

        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            if (retryAfter is not null && metadataName == MetadataName.RetryAfter.Name)
            {
                metadata = retryAfter.Value;
                return true;
            }

            metadata = null;
            return false;
        }
    }

    [Fact]
    public void AddRateLimitingServices_ConfiguresRejectionStatusCode429()
    {
        var services = new ServiceCollection();
        services.AddRateLimitingServices();
        services.AddLogging();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value;

        options.RejectionStatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
    }

    [Fact]
    public void AddRateLimitingServices_ConfiguresGlobalLimiter()
    {
        var services = new ServiceCollection();
        services.AddRateLimitingServices();
        services.AddLogging();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value;

        options.GlobalLimiter.Should().NotBeNull();
    }

    [Fact]
    public void AddRateLimitingServices_ConfiguresGlobalLimiter_AsReloadableLoginRateLimiterSingleton()
    {
        var services = new ServiceCollection();
        services.AddRateLimitingServices();
        services.AddLogging();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value;
        var limiterInstance = provider.GetRequiredService<MyGardenPlanner2026.Configuration.RateLimiting.ReloadableLoginRateLimiter>();

        options.GlobalLimiter.Should().BeSameAs(limiterInstance);
    }
}