namespace MyGardenPlanner2026.Tests.Unit.Services.Jit;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Interceptors;
using MyGardenPlanner2026.Infrastructure.Services;
using MyGardenPlanner2026.Infrastructure.Services.Jit;
using NSubstitute;
using Xunit;

public class RoleElevationExpirySweepServiceTests : TestDbContext
{
    private static readonly DateTimeOffset Now = new(2026, 8, 28, 10, 0, 0, TimeSpan.Zero);

    private RoleElevationExpirySweepService CreateService(
        TestTimeProvider timeProvider, JitElevationPolicyOptions? options = null) =>
        new(CreateAdminDbContextFactory(),
            new TestOptionsMonitor<JitElevationPolicyOptions>(options ?? new JitElevationPolicyOptions()), timeProvider,
            Substitute.For<ILogger<RoleElevationExpirySweepService>>());

    private async Task<Guid> SeedRequestAsync(
        RoleElevationStatus status, DateTimeOffset createdAtUtc, DateTimeOffset? validToUtc = null)
    {
        using var context = CreateDbContext();
        var request = new RoleElevationRequest
        {
            RequesterUserId = "user-1",
            ApproverUserId = status is RoleElevationStatus.Approved or RoleElevationStatus.Rejected ? "user-2" : null,
            RoleName = "SystemAdmin",
            Reason = "Test.",
            RequestedMinutes = 60,
            Status = status,
            CreatedAtUtc = createdAtUtc,
            ValidFromUtc = validToUtc?.AddMinutes(-60),
            ValidToUtc = validToUtc
        };
        context.Add(request);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        return request.Id;
    }

    private async Task<RoleElevationStatus> ReadStatusAsync(Guid id)
    {
        using var context = CreateDbContext();
        var request = await context.RoleElevationRequests
            .SingleAsync(r => r.Id == id, TestContext.Current.CancellationToken);
        return request.Status;
    }

    [Fact]
    public async Task SweepOnceAsync_ApprovedRequestPastValidToUtc_MarksAsExpired()
    {
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 8, 28, 10, 0, 0, TimeSpan.Zero));

        using var context = CreateDbContext();
        var request = new RoleElevationRequest
        {
            RequesterUserId = "user-1",
            ApproverUserId = "user-2",
            RoleName = "SystemAdmin",
            Reason = "Test.",
            RequestedMinutes = 60,
            Status = RoleElevationStatus.Approved,
            ValidFromUtc = timeProvider.GetUtcNow().AddMinutes(-90),
            ValidToUtc = timeProvider.GetUtcNow().AddMinutes(-30)
        };
        context.Add(request);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = CreateService(timeProvider);

        var sweptCount = await service.SweepOnceAsync(TestContext.Current.CancellationToken);

        sweptCount.Should().Be(1);

        using var verifyContext = CreateDbContext();
        var updated = await verifyContext.RoleElevationRequests
            .SingleAsync(r => r.Id == request.Id, TestContext.Current.CancellationToken);
        updated.Status.Should().Be(RoleElevationStatus.Expired);
    }

    [Fact]
    public async Task SweepOnceAsync_ApprovedRequestStillWithinWindow_LeavesStatusUnchanged()
    {
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 8, 28, 10, 0, 0, TimeSpan.Zero));

        using var context = CreateDbContext();
        var request = new RoleElevationRequest
        {
            RequesterUserId = "user-1",
            ApproverUserId = "user-2",
            RoleName = "SystemAdmin",
            Reason = "Test.",
            RequestedMinutes = 60,
            Status = RoleElevationStatus.Approved,
            ValidFromUtc = timeProvider.GetUtcNow(),
            ValidToUtc = timeProvider.GetUtcNow().AddMinutes(60)
        };
        context.Add(request);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = CreateService(timeProvider);

        var sweptCount = await service.SweepOnceAsync(TestContext.Current.CancellationToken);

        sweptCount.Should().Be(0);

        using var verifyContext = CreateDbContext();
        var unchanged = await verifyContext.RoleElevationRequests
            .SingleAsync(r => r.Id == request.Id, TestContext.Current.CancellationToken);
        unchanged.Status.Should().Be(RoleElevationStatus.Approved);
    }

    [Fact]
    public async Task SweepOnceAsync_TransitionToExpired_WritesAuditLogUpdateEntry()
    {
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 8, 28, 10, 0, 0, TimeSpan.Zero));
        var currentUser = FakeCurrentUser.Create("system", "system@mygardenplanner.dk", null);
        var contextFactory = CreateAdminDbContextFactoryWithInterceptors(
            new SoftDeleteInterceptor(currentUser), new AuditLoggingInterceptor(currentUser));

        using var seedContext = CreateDbContext();
        var seedRequest = new RoleElevationRequest
        {
            RequesterUserId = "user-1",
            ApproverUserId = "user-2",
            RoleName = "SystemAdmin",
            Reason = "Test.",
            RequestedMinutes = 60,
            Status = RoleElevationStatus.Approved,
            ValidFromUtc = timeProvider.GetUtcNow().AddMinutes(-90),
            ValidToUtc = timeProvider.GetUtcNow().AddMinutes(-30)
        };
        seedContext.Add(seedRequest);
        await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new RoleElevationExpirySweepService(
            contextFactory, new TestOptionsMonitor<JitElevationPolicyOptions>(new JitElevationPolicyOptions()), timeProvider,
            Substitute.For<ILogger<RoleElevationExpirySweepService>>());

        await service.SweepOnceAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var updateLog = await verifyContext.AuditLogs
            .Where(l => l.EntityName == nameof(RoleElevationRequest)
                && l.EntityId == seedRequest.Id.ToString()
                && l.Action == AuditAction.Update)
            .SingleAsync(TestContext.Current.CancellationToken);

        updateLog.NewValues.Should().Contain("Expired");
    }

    [Fact]
    public async Task WaitForNextSweepAsync_IntervalChangedDuringWait_ReturnsWithoutWaitingFullOldInterval()
    {
        var monitor = new TestOptionsMonitor<JitElevationPolicyOptions>(
            new JitElevationPolicyOptions { MinRequestedMinutes = 30, MaxRequestedMinutes = 90, SweepIntervalMinutes = 60 });
        var service = new RoleElevationExpirySweepService(
            CreateAdminDbContextFactory(), monitor, TimeProvider.System,
            Substitute.For<ILogger<RoleElevationExpirySweepService>>());

        var waitTask = service.WaitForNextSweepAsync(TestContext.Current.CancellationToken);

        await Task.Delay(50, TestContext.Current.CancellationToken);
        monitor.Set(new JitElevationPolicyOptions { MinRequestedMinutes = 30, MaxRequestedMinutes = 90, SweepIntervalMinutes = 1 });

        var completedTask = await Task.WhenAny(waitTask, Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

        completedTask.Should().Be(waitTask);
    }

    [Fact]
    public async Task SweepOnceAsync_PendingRequestOlderThanExpiry_MarksAsCancelled()
    {
        var id = await SeedRequestAsync(RoleElevationStatus.Pending, Now.AddHours(-49));

        var sweptCount = await CreateService(new TestTimeProvider(Now)).SweepOnceAsync(TestContext.Current.CancellationToken);

        sweptCount.Should().Be(1);
        (await ReadStatusAsync(id)).Should().Be(RoleElevationStatus.Cancelled);
    }

    [Fact]
    public async Task SweepOnceAsync_PendingRequestWithinExpiry_IsNotAffected()
    {
        var id = await SeedRequestAsync(RoleElevationStatus.Pending, Now.AddHours(-47));

        var sweptCount = await CreateService(new TestTimeProvider(Now)).SweepOnceAsync(TestContext.Current.CancellationToken);

        sweptCount.Should().Be(0);
        (await ReadStatusAsync(id)).Should().Be(RoleElevationStatus.Pending);
    }

    [Fact]
    public async Task SweepOnceAsync_PendingRequestExactlyAtExpiry_IsNotAffected()
    {
        var id = await SeedRequestAsync(RoleElevationStatus.Pending, Now.AddHours(-48));

        var sweptCount = await CreateService(new TestTimeProvider(Now)).SweepOnceAsync(TestContext.Current.CancellationToken);

        sweptCount.Should().Be(0);
        (await ReadStatusAsync(id)).Should().Be(RoleElevationStatus.Pending);
    }

    [Fact]
    public async Task SweepOnceAsync_CustomPendingExpiry_IsRespected()
    {
        var id = await SeedRequestAsync(RoleElevationStatus.Pending, Now.AddHours(-2));
        var options = new JitElevationPolicyOptions { PendingExpiryHours = 1 };

        var sweptCount = await CreateService(new TestTimeProvider(Now), options)
            .SweepOnceAsync(TestContext.Current.CancellationToken);

        sweptCount.Should().Be(1);
        (await ReadStatusAsync(id)).Should().Be(RoleElevationStatus.Cancelled);
    }

    [Fact]
    public async Task SweepOnceAsync_ApprovedAndPendingBothDue_HandlesBothAndReturnsTotal()
    {
        var approvedId = await SeedRequestAsync(RoleElevationStatus.Approved, Now.AddHours(-3), Now.AddMinutes(-30));
        var pendingId = await SeedRequestAsync(RoleElevationStatus.Pending, Now.AddHours(-49));

        var sweptCount = await CreateService(new TestTimeProvider(Now)).SweepOnceAsync(TestContext.Current.CancellationToken);

        sweptCount.Should().Be(2);
        (await ReadStatusAsync(approvedId)).Should().Be(RoleElevationStatus.Expired);
        (await ReadStatusAsync(pendingId)).Should().Be(RoleElevationStatus.Cancelled);
    }

    [Fact]
    public async Task SweepOnceAsync_ApprovedRequestStillValid_IsNotCancelledEvenIfOld()
    {
        // Pending-udløbet må ikke ramme godkendte anmodninger: de styres kun af ValidToUtc.
        var id = await SeedRequestAsync(RoleElevationStatus.Approved, Now.AddHours(-100), Now.AddMinutes(30));

        var sweptCount = await CreateService(new TestTimeProvider(Now)).SweepOnceAsync(TestContext.Current.CancellationToken);

        sweptCount.Should().Be(0);
        (await ReadStatusAsync(id)).Should().Be(RoleElevationStatus.Approved);
    }

    [Theory]
    [InlineData(RoleElevationStatus.Rejected)]
    [InlineData(RoleElevationStatus.Expired)]
    [InlineData(RoleElevationStatus.Cancelled)]
    public async Task SweepOnceAsync_FinalStatusOlderThanExpiry_IsNotAffected(RoleElevationStatus status)
    {
        var id = await SeedRequestAsync(status, Now.AddHours(-100));

        var sweptCount = await CreateService(new TestTimeProvider(Now)).SweepOnceAsync(TestContext.Current.CancellationToken);

        sweptCount.Should().Be(0);
        (await ReadStatusAsync(id)).Should().Be(status);
    }

    [Fact]
    public async Task SweepOnceAsync_TransitionToCancelled_WritesAuditLogUpdateEntry()
    {
        var currentUser = FakeCurrentUser.Create("system", "system@mygardenplanner.dk", null);
        var contextFactory = CreateAdminDbContextFactoryWithInterceptors(
            new SoftDeleteInterceptor(currentUser), new AuditLoggingInterceptor(currentUser));
        var id = await SeedRequestAsync(RoleElevationStatus.Pending, Now.AddHours(-49));

        var service = new RoleElevationExpirySweepService(
            contextFactory, new TestOptionsMonitor<JitElevationPolicyOptions>(new JitElevationPolicyOptions()),
            new TestTimeProvider(Now), Substitute.For<ILogger<RoleElevationExpirySweepService>>());

        await service.SweepOnceAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var updateLog = await verifyContext.AuditLogs
            .Where(l => l.EntityName == nameof(RoleElevationRequest)
                && l.EntityId == id.ToString()
                && l.Action == AuditAction.Update)
            .SingleAsync(TestContext.Current.CancellationToken);

        updateLog.OldValues.Should().Contain("Pending");
        updateLog.NewValues.Should().Contain("Cancelled");
    }
}