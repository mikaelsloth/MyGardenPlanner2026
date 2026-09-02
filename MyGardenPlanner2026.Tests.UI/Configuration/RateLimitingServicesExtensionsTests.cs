namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Configuration.Extensions;
using Xunit;

public class RateLimitingServicesExtensionsTests
{
    [Fact]
    public void AddRateLimitingServices_ConfiguresRejectionStatusCode429()
    {
        var services = new ServiceCollection();
        services.AddRateLimitingServices();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value;

        options.RejectionStatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
    }

    [Fact]
    public void AddRateLimitingServices_ConfiguresGlobalLimiter()
    {
        var services = new ServiceCollection();
        services.AddRateLimitingServices();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value;

        options.GlobalLimiter.Should().NotBeNull();
    }

    [Fact]
    public void AddRateLimitingServices_ConfiguresGlobalLimiter_AsReloadableLoginRateLimiterSingleton()
    {
        var services = new ServiceCollection();
        services.AddRateLimitingServices();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value;
        var limiterInstance = provider.GetRequiredService<MyGardenPlanner2026.Configuration.RateLimiting.ReloadableLoginRateLimiter>();

        options.GlobalLimiter.Should().BeSameAs(limiterInstance);
    }
}