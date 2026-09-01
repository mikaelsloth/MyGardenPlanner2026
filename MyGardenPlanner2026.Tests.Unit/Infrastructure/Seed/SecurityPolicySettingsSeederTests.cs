namespace MyGardenPlanner2026.Tests.Unit.Infrastructure.Seed;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using Xunit;

public class SecurityPolicySettingsSeederTests : TestDbContext
{
    [Fact]
    public async Task SeedAsync_JitElevationPolicySettings_IsIdempotent()
        => await AssertSeedIsIdempotentAsync(() => new JitElevationPolicySettings
        {
            MinRequestedMinutes = 30,
            MaxRequestedMinutes = 90,
            SweepIntervalMinutes = 5
        });

    [Fact]
    public async Task SeedAsync_ReAuthenticationPolicySettings_IsIdempotent()
        => await AssertSeedIsIdempotentAsync(() => new ReAuthenticationPolicySettings { MaxAgeMinutes = 15 });

    [Fact]
    public async Task SeedAsync_ReAuthFailureTrackerSettings_IsIdempotent()
        => await AssertSeedIsIdempotentAsync(() => new ReAuthFailureTrackerSettings { Threshold = 5, WindowDays = 2 });

    [Fact]
    public async Task SeedAsync_AdminApiRateLimitSettings_IsIdempotent()
        => await AssertSeedIsIdempotentAsync(() => new AdminApiRateLimitSettings
        {
            PermitLimit = 100,
            WindowSeconds = 60,
            SegmentsPerWindow = 6
        });

    [Fact]
    public async Task SeedAsync_LoginRateLimitSettings_IsIdempotent()
        => await AssertSeedIsIdempotentAsync(() => new LoginRateLimitSettings { PermitLimit = 5, WindowSeconds = 60 });

    [Fact]
    public async Task SeedAsync_EmptyTable_InsertsProvidedDefaultValues()
    {
        var seeder = new SecurityPolicySettingsSeeder<JitElevationPolicySettings>(
            CreateAdminDbContextFactory(),
            () => new JitElevationPolicySettings { MinRequestedMinutes = 45, MaxRequestedMinutes = 60, SweepIntervalMinutes = 10 });

        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        using var context = CreateDbContext();
        var saved = await context.JitElevationPolicySettings.SingleAsync(TestContext.Current.CancellationToken);
        saved.MinRequestedMinutes.Should().Be(45);
    }

    private async Task AssertSeedIsIdempotentAsync<TEntity>(Func<TEntity> createDefault)
        where TEntity : class, ISingletonSettings
    {
        var contextFactory = CreateAdminDbContextFactory();
        var seeder = new SecurityPolicySettingsSeeder<TEntity>(contextFactory, createDefault);

        await seeder.SeedAsync(TestContext.Current.CancellationToken);
        await seeder.SeedAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var count = await verifyContext.Set<TEntity>().CountAsync(TestContext.Current.CancellationToken);
        count.Should().Be(1);
    }
}