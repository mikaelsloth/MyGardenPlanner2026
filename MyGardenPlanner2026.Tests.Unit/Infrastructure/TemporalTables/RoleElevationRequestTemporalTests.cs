namespace MyGardenPlanner2026.Tests.Unit.Infrastructure.TemporalTables;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

[Trait("Category", "SqlServerIntegration")]
public class RoleElevationRequestTemporalTests : TestSqlExpressDbContext
{
    [Fact]
    public async Task ApprovingRequest_IsRecoverable_ViaTemporalHistoryQuery()
    {
        using var context = CreateDbContext();

        var request = new RoleElevationRequest
        {
            RequesterUserId = "user-1",
            RoleName = "SystemAdmin",
            Reason = "Temporal test.",
            RequestedHours = 2
        };
        context.Add(request);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var beforeApproval = DateTime.UtcNow;
        await Task.Delay(50, TestContext.Current.CancellationToken);

        request.Status = RoleElevationStatus.Approved;
        request.ApproverUserId = "user-2";
        request.ValidFromUtc = DateTimeOffset.UtcNow;
        request.ValidToUtc = DateTimeOffset.UtcNow.AddHours(2);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var historicValue = await context.RoleElevationRequests
            .TemporalAsOf(beforeApproval)
            .SingleAsync(r => r.Id == request.Id, TestContext.Current.CancellationToken);

        historicValue.Status.Should().Be(RoleElevationStatus.Pending);

        var currentValue = await context.RoleElevationRequests
            .SingleAsync(r => r.Id == request.Id, TestContext.Current.CancellationToken);

        currentValue.Status.Should().Be(RoleElevationStatus.Approved);
    }

    [Fact]
    public void Database_UsesAdminSchemaForRoleElevationRequests()
    {
        using var context = CreateDbContext();

        context.Model.FindEntityType(typeof(RoleElevationRequest))!.GetSchema().Should().Be("admin");
    }
}