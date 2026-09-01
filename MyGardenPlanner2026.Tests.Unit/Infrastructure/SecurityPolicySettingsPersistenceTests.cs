namespace MyGardenPlanner2026.Tests.Unit.Infrastructure;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Admin;
using Xunit;

public class SecurityPolicySettingsPersistenceTests : TestDbContext
{
    [Fact]
    public async Task CanInsertAndRetrieve_JitElevationPolicySettings()
    {
        using var context = CreateDbContext();
        context.Add(new JitElevationPolicySettings
        {
            MinRequestedMinutes = 30,
            MaxRequestedMinutes = 90,
            SweepIntervalMinutes = 5
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var saved = await verifyContext.JitElevationPolicySettings.SingleAsync(TestContext.Current.CancellationToken);

        saved.Id.Should().Be(JitElevationPolicySettings.SingletonId);
        saved.MaxRequestedMinutes.Should().Be(90);
    }

    [Fact]
    public async Task CanInsertAndRetrieve_ReAuthenticationPolicySettings()
    {
        using var context = CreateDbContext();
        context.Add(new ReAuthenticationPolicySettings { MaxAgeMinutes = 15 });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var saved = await verifyContext.ReAuthenticationPolicySettings.SingleAsync(TestContext.Current.CancellationToken);

        saved.Id.Should().Be(ReAuthenticationPolicySettings.SingletonId);
        saved.MaxAgeMinutes.Should().Be(15);
    }

    [Fact]
    public async Task CanInsertAndRetrieve_ReAuthFailureTrackerSettings()
    {
        using var context = CreateDbContext();
        context.Add(new ReAuthFailureTrackerSettings { Threshold = 5, WindowDays = 2 });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var saved = await verifyContext.ReAuthFailureTrackerSettings.SingleAsync(TestContext.Current.CancellationToken);

        saved.Id.Should().Be(ReAuthFailureTrackerSettings.SingletonId);
        saved.Threshold.Should().Be(5);
    }

    [Fact]
    public async Task CanInsertAndRetrieve_AdminApiRateLimitSettings()
    {
        using var context = CreateDbContext();
        context.Add(new AdminApiRateLimitSettings { PermitLimit = 100, WindowSeconds = 60, SegmentsPerWindow = 6 });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var saved = await verifyContext.AdminApiRateLimitSettings.SingleAsync(TestContext.Current.CancellationToken);

        saved.Id.Should().Be(AdminApiRateLimitSettings.SingletonId);
        saved.SegmentsPerWindow.Should().Be(6);
    }

    [Fact]
    public async Task CanInsertAndRetrieve_LoginRateLimitSettings()
    {
        using var context = CreateDbContext();
        context.Add(new LoginRateLimitSettings { PermitLimit = 5, WindowSeconds = 60 });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var saved = await verifyContext.LoginRateLimitSettings.SingleAsync(TestContext.Current.CancellationToken);

        saved.Id.Should().Be(LoginRateLimitSettings.SingletonId);
        saved.PermitLimit.Should().Be(5);
    }
}