namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Services;
using NSubstitute;
using Xunit;

public class JitElevationServiceTests : TestDbContext
{
    private JitElevationService CreateService(params string[] knownRoles)
    {
        var store = Substitute.For<IRoleStore<IdentityRole>>();
        var roleManager = Substitute.For<RoleManager<IdentityRole>>(store, null, null, null, null);
        roleManager.RoleExistsAsync(Arg.Any<string>())
            .Returns(callInfo => Task.FromResult(knownRoles.Contains(callInfo.Arg<string>())));

        return new JitElevationService(CreateAdminDbContextFactory(), roleManager);
    }

    [Fact]
    public async Task RequestElevationAsync_ValidRequest_CreatesPendingRequest()
    {
        var service = CreateService("SystemAdmin");

        var dto = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 4, "Skal rette prisfejl.", TestContext.Current.CancellationToken);

        dto.Status.Should().Be(RoleElevationStatus.Pending);
        dto.RequesterUserId.Should().Be("user-1");
        dto.ApproverUserId.Should().BeNull();
        dto.ValidFromUtc.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    public async Task RequestElevationAsync_HoursOutsideOneToEight_ThrowsArgumentOutOfRangeException(int hours)
    {
        var service = CreateService("SystemAdmin");

        var act = async () => await service.RequestElevationAsync(
            "user-1", "SystemAdmin", hours, "Test.", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task RequestElevationAsync_UnknownRole_ThrowsInvalidOperationException()
    {
        var service = CreateService();

        var act = async () => await service.RequestElevationAsync(
            "user-1", "GhostRole", 2, "Test.", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ApproveElevationAsync_DifferentApprover_SetsApprovedStatusAndValidityWindow()
    {
        var service = CreateService("SystemAdmin");
        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 3, "Test.", TestContext.Current.CancellationToken);

        var approved = await service.ApproveElevationAsync(
            "user-2", request.Id, TestContext.Current.CancellationToken);

        approved.Status.Should().Be(RoleElevationStatus.Approved);
        approved.ApproverUserId.Should().Be("user-2");
        approved.ValidFromUtc.Should().NotBeNull();
        approved.ValidToUtc.Should().BeCloseTo(approved.ValidFromUtc!.Value.AddHours(3), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ApproveElevationAsync_SameUserAsRequester_ThrowsInvalidOperationException()
    {
        var service = CreateService("SystemAdmin");
        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 2, "Test.", TestContext.Current.CancellationToken);

        var act = async () => await service.ApproveElevationAsync(
            "user-1", request.Id, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ApproveElevationAsync_AlreadyApproved_ThrowsInvalidOperationException()
    {
        var service = CreateService("SystemAdmin");
        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 2, "Test.", TestContext.Current.CancellationToken);
        await service.ApproveElevationAsync("user-2", request.Id, TestContext.Current.CancellationToken);

        var act = async () => await service.ApproveElevationAsync(
            "user-3", request.Id, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RejectElevationAsync_DifferentApprover_SetsRejectedStatus()
    {
        var service = CreateService("SystemAdmin");
        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 2, "Test.", TestContext.Current.CancellationToken);

        var rejected = await service.RejectElevationAsync(
            "user-2", request.Id, TestContext.Current.CancellationToken);

        rejected.Status.Should().Be(RoleElevationStatus.Rejected);
        rejected.ApproverUserId.Should().Be("user-2");
        rejected.ValidFromUtc.Should().BeNull();
    }

    [Fact]
    public async Task RejectElevationAsync_SameUserAsRequester_ThrowsInvalidOperationException()
    {
        var service = CreateService("SystemAdmin");
        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 2, "Test.", TestContext.Current.CancellationToken);

        var act = async () => await service.RejectElevationAsync(
            "user-1", request.Id, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HasActiveElevationAsync_ApprovedAndWithinWindow_ReturnsTrue()
    {
        var service = CreateService("SystemAdmin");
        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 4, "Test.", TestContext.Current.CancellationToken);
        await service.ApproveElevationAsync("user-2", request.Id, TestContext.Current.CancellationToken);

        var result = await service.HasActiveElevationAsync(
            "user-1", "SystemAdmin", TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasActiveElevationAsync_NoApprovedRequest_ReturnsFalse()
    {
        var service = CreateService("SystemAdmin");
        await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 2, "Test.", TestContext.Current.CancellationToken);

        var result = await service.HasActiveElevationAsync(
            "user-1", "SystemAdmin", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveElevationAsync_DifferentRole_ReturnsFalse()
    {
        var service = CreateService("SystemAdmin", "DataAdmin");
        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 2, "Test.", TestContext.Current.CancellationToken);
        await service.ApproveElevationAsync("user-2", request.Id, TestContext.Current.CancellationToken);

        var result = await service.HasActiveElevationAsync(
            "user-1", "DataAdmin", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }
}