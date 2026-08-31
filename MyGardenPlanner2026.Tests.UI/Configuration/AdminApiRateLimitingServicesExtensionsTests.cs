namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public class AdminApiRateLimitingServicesExtensionsTests
{
    [Fact]
    public void AddAdminApiRateLimiting_RegistersRateLimiterAsSingleton()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddAdminApiRateLimiting(configuration);

        services.Should().Contain(d =>
            d.ServiceType == typeof(IAdminActionRateLimiter)
            && d.ImplementationType == typeof(AdminActionRateLimiter)
            && d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddAdminApiRateLimiting_ResolvesRateLimiterFromContainer()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddAdminApiRateLimiting(configuration);

        using var provider = services.BuildServiceProvider();
        var limiter = provider.GetRequiredService<IAdminActionRateLimiter>();

        limiter.Should().NotBeNull();
    }
}