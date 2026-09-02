namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Services;
using NSubstitute;
using Xunit;

public class SecurityPolicyRuntimeReloadTests : TestDbContext
{
    private ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton(CreateAdminDbContextFactory());
        services.AddSingleton<SecurityPolicyChangeSignal>();
        services.AddSingleton<ISecurityPolicyChangeSignal>(sp => sp.GetRequiredService<SecurityPolicyChangeSignal>());
        services.AddSingleton<IConfigureOptions<JitElevationPolicyOptions>, JitElevationPolicyOptionsConfigurator>();
        services.AddSingleton<IOptionsChangeTokenSource<JitElevationPolicyOptions>>(sp =>
            new SecurityPolicyOptionsChangeTokenSource<JitElevationPolicyOptions>(sp.GetRequiredService<SecurityPolicyChangeSignal>()));

        return services.BuildServiceProvider();
    }

    private async Task SeedAsync(int maxRequestedMinutes)
    {
        using var context = CreateDbContext();
        context.Add(new JitElevationPolicySettings
        {
            MinRequestedMinutes = 30,
            MaxRequestedMinutes = maxRequestedMinutes,
            SweepIntervalMinutes = 5
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task IOptionsMonitor_ReflectsDatabaseValue_WithoutAnyTrigger()
    {
        await SeedAsync(maxRequestedMinutes: 90);
        using var provider = BuildProvider();

        var monitor = provider.GetRequiredService<IOptionsMonitor<JitElevationPolicyOptions>>();

        monitor.CurrentValue.MaxRequestedMinutes.Should().Be(90);
    }

    [Fact]
    public async Task TriggerChange_AfterDatabaseUpdate_UpdatesCurrentValue_WithoutRestart()
    {
        await SeedAsync(maxRequestedMinutes: 90);
        using var provider = BuildProvider();
        var monitor = provider.GetRequiredService<IOptionsMonitor<JitElevationPolicyOptions>>();
        var signal = provider.GetRequiredService<ISecurityPolicyChangeSignal>();

        monitor.CurrentValue.MaxRequestedMinutes.Should().Be(90); // cache'es første gang

        var adminService = new JitElevationPolicyAdminService(
            CreateAdminDbContextFactory(), signal, Substitute.For<ISecurityAlertService>());

        await adminService.UpdateAsync(
            new JitElevationPolicyDto(30, 120, 5), "user-1", TestContext.Current.CancellationToken);

        monitor.CurrentValue.MaxRequestedMinutes.Should().Be(120);
    }

    [Fact]
    public async Task TriggerChange_ForDifferentOptionsType_DoesNotAffectUnrelatedMonitor()
    {
        await SeedAsync(maxRequestedMinutes: 90);
        using var provider = BuildProvider();
        var monitor = provider.GetRequiredService<IOptionsMonitor<JitElevationPolicyOptions>>();
        var signal = provider.GetRequiredService<ISecurityPolicyChangeSignal>();

        _ = monitor.CurrentValue; // cache'er første gang

        signal.TriggerChange<ReAuthenticationPolicyOptions>(); // urelateret type

        monitor.CurrentValue.MaxRequestedMinutes.Should().Be(90); // uændret
    }
}