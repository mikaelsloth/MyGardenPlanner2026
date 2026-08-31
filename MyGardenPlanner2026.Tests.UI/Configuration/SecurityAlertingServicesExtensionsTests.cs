namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using NSubstitute;
using Xunit;

public class SecurityAlertingServicesExtensionsTests
{
    [Fact]
    public void AddSecurityAlertingServices_RegistersSecurityAlertServiceAsSingleton()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddSecurityAlertingServices(configuration);

        services.Should().Contain(d =>
            d.ServiceType == typeof(ISecurityAlertService) && d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddSecurityAlertingServices_RegistersSecurityEmailSenderAsSingleton()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddSecurityAlertingServices(configuration);

        services.Should().Contain(d =>
            d.ServiceType == typeof(ISecurityEmailSender) && d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddSecurityAlertingServices_ResolvesSecurityAlertServiceFromContainer()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddSecurityAlertingServices(configuration);
        services.AddSingleton(Substitute.For<Microsoft.Extensions.Logging.ILoggerFactory>());
        services.AddLogging();

        using var provider = services.BuildServiceProvider();
        var alertService = provider.GetRequiredService<ISecurityAlertService>();

        alertService.Should().NotBeNull();
    }
}