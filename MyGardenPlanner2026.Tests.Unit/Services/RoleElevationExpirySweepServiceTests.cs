namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Interceptors;
using MyGardenPlanner2026.Infrastructure.Services;
using NSubstitute;
using Xunit;

public class RoleElevationExpirySweepServiceTests : TestDbContext
{
    private RoleElevationExpirySweepService CreateService(TestTimeProvider timeProvider) =>
        new(CreateAdminDbContextFactory(), Options.Create(new JitElevationPolicyOptions()), timeProvider,
            Substitute.For<ILogger<RoleElevationExpirySweepService>>());

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
    public async Task SweepOnceAsync_PendingRequestPastWindow_IsNotAffected()
    {
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 8, 28, 10, 0, 0, TimeSpan.Zero));

        using var context = CreateDbContext();
        var request = new RoleElevationRequest
        {
            RequesterUserId = "user-1",
            RoleName = "SystemAdmin",
            Reason = "Test.",
            RequestedMinutes = 60,
            Status = RoleElevationStatus.Pending
        };
        context.Add(request);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = CreateService(timeProvider);

        var sweptCount = await service.SweepOnceAsync(TestContext.Current.CancellationToken);

        sweptCount.Should().Be(0);

        using var verifyContext = CreateDbContext();
        var unchanged = await verifyContext.RoleElevationRequests
            .SingleAsync(r => r.Id == request.Id, TestContext.Current.CancellationToken);
        unchanged.Status.Should().Be(RoleElevationStatus.Pending);
    }

    [Fact]
    public async Task SweepOnceAsync_TransitionToExpired_WritesAuditLogUpdateEntry()
    {
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 8, 28, 10, 0, 0, TimeSpan.Zero));
        var currentUser = Substitute.For<ICurrentUserAccessor>();
        currentUser.GetCurrent().Returns(new CurrentUserInfo("system", "system@mygardenplanner.dk", null));

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
            contextFactory, Options.Create(new JitElevationPolicyOptions()), timeProvider,
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
}