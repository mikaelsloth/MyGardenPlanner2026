namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using Xunit;

public class ReAuthFailureTrackingServicesExtensionsTests
{
    [Fact]
    public void AddReAuthFailureTracking_RegistersTrackerAsScoped()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddReAuthFailureTracking(configuration);

        services.Should().Contain(d =>
            d.ServiceType == typeof(IReAuthFailureTracker) && d.Lifetime == ServiceLifetime.Scoped);
    }
}