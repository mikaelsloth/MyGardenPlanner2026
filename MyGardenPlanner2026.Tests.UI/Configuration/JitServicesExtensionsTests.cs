namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public class JitServicesExtensionsTests
{
    [Fact]
    public void AddJitElevationServices_RegistersRoleElevationExpirySweepServiceAsHostedService()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddJitElevationServices(configuration);

        services.Should().Contain(d =>
            d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(RoleElevationExpirySweepService));
    }
}