namespace MyGardenPlanner2026.Tests.Unit.Infrastructure;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Interceptors;
using NSubstitute;
using Xunit;

public class RoleElevationRequestPersistenceTests : TestDbContext
{
    [Fact]
    public async Task CanInsertAndRetrieve_PendingElevationRequest()
    {
        using var context = CreateDbContext();
        var request = new RoleElevationRequest
        {
            RequesterUserId = "user-1",
            RoleName = "SystemAdmin",
            Reason = "Skal rette en fejlkonfigureret rabat-trappe.",
            RequestedHours = 4
        };

        context.Add(request);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var saved = await verifyContext.RoleElevationRequests
            .SingleAsync(r => r.Id == request.Id, TestContext.Current.CancellationToken);

        saved.Status.Should().Be(RoleElevationStatus.Pending);
        saved.ApproverUserId.Should().BeNull();
        saved.ValidFromUtc.Should().BeNull();
        saved.ValidToUtc.Should().BeNull();
    }

    [Fact]
    public async Task CanApprove_SetsApproverAndValidityWindow()
    {
        using var context = CreateDbContext();
        var request = new RoleElevationRequest
        {
            RequesterUserId = "user-1",
            RoleName = "DataAdmin",
            Reason = "Nulstiller volumenrabat-katalog.",
            RequestedHours = 2
        };
        context.Add(request);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        request.Status = RoleElevationStatus.Approved;
        request.ApproverUserId = "user-2";
        request.ValidFromUtc = DateTimeOffset.UtcNow;
        request.ValidToUtc = DateTimeOffset.UtcNow.AddHours(2);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var saved = await verifyContext.RoleElevationRequests
            .SingleAsync(r => r.Id == request.Id, TestContext.Current.CancellationToken);

        saved.Status.Should().Be(RoleElevationStatus.Approved);
        saved.ApproverUserId.Should().Be("user-2");
        saved.ValidFromUtc.Should().NotBeNull();
        saved.ValidToUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task DefaultQuery_ExcludesSoftDeletedRequests()
    {
        var currentUser = Substitute.For<ICurrentUserAccessor>();
        currentUser.GetCurrent().Returns(new CurrentUserInfo("user-3", "user3@example.dk", "127.0.0.1"));

        using var context = CreateDbContextWithInterceptors(new SoftDeleteInterceptor(currentUser));

        var request = new RoleElevationRequest
        {
            RequesterUserId = "user-3",
            RoleName = "AuditViewer",
            Reason = "Test.",
            RequestedHours = 1
        };
        context.Add(request);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.Remove(request);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var visible = await verifyContext.RoleElevationRequests
            .SingleOrDefaultAsync(r => r.Id == request.Id, TestContext.Current.CancellationToken);

        visible.Should().BeNull();
    }
}