namespace MyGardenPlanner2026.Tests.UI.Configuration.Extensions;

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;
using MyGardenPlanner2026.Infrastructure.Services.AuditLog.ExportJobs;
using Xunit;

public sealed class AuditLogExportJobServicesExtensionsTests
{
    [Fact]
    public void AddAuditLogExportJobs_RegistersBackgroundServiceAsHostedService()
    {
        var services = new ServiceCollection();

        services.AddAuditLogExportJobs(new ConfigurationBuilder().Build());

        services.Should().Contain(d =>
            d.ServiceType == typeof(IHostedService)
            && d.ImplementationType == typeof(AuditLogExportBackgroundService));
    }

    [Fact]
    public void AddAuditLogExportJobs_RegistersProcessorAsScoped()
    {
        var services = new ServiceCollection();

        services.AddAuditLogExportJobs(new ConfigurationBuilder().Build());

        services.Should().Contain(d =>
            d.ServiceType == typeof(IAuditLogExportJobProcessor)
            && d.ImplementationType == typeof(AuditLogExportJobProcessor)
            && d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddAuditLogExportJobs_NoConfiguration_UsesDefaultOptions()
    {
        var services = new ServiceCollection();
        services.AddAuditLogExportJobs(new ConfigurationBuilder().Build());
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<AuditLogExportJobOptions>>().Value;

        options.RetentionHours.Should().Be(24);
        options.MaxActiveJobsPerUser.Should().Be(3);
        options.PollIntervalSeconds.Should().Be(10);
        options.StaleRunningMinutes.Should().Be(30);
        options.MaxFileSizeMegabytes.Should().Be(50);
    }

    [Fact]
    public void AddAuditLogExportJobs_ConfigurationPresent_BindsOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AuditLogExportJob:RetentionHours"] = "48",
                ["AuditLogExportJob:MaxActiveJobsPerUser"] = "5"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddAuditLogExportJobs(configuration);
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<AuditLogExportJobOptions>>().Value;

        options.RetentionHours.Should().Be(48);
        options.MaxActiveJobsPerUser.Should().Be(5);
    }
}