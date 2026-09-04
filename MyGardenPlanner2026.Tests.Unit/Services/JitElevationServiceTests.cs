namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Services;
using NSubstitute;
using Xunit;

public class JitElevationServiceTests : TestDbContext
{
    private readonly ISecurityAlertService securityAlertService = Substitute.For<ISecurityAlertService>();

    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(store, null, null, null, null, null, null, null, null);
    }

    private static UserManager<ApplicationUser> CreateUserManagerWithRoles(string userId, params string[] roles)
    {
        var user = new ApplicationUser { Id = userId };
        var userManager = CreateUserManager();
        userManager.FindByIdAsync(userId).Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.GetRolesAsync(user).Returns(Task.FromResult<IList<string>>([.. roles]));
        return userManager;
    }

    private JitElevationService CreateService(params string[] knownRoles) =>
        CreateServiceWithPolicy(new JitElevationPolicyOptions(), knownRoles);

    private JitElevationService CreateServiceWithPolicy(JitElevationPolicyOptions policy, params string[] knownRoles)
    {
        var store = Substitute.For<IRoleStore<IdentityRole>>();
        var roleManager = Substitute.For<RoleManager<IdentityRole>>(store, null, null, null, null);
        roleManager.RoleExistsAsync(Arg.Any<string>())
            .Returns(callInfo => Task.FromResult(knownRoles.Contains(callInfo.Arg<string>())));

        return new JitElevationService(
            CreateAdminDbContextFactory(), roleManager, CreateUserManager(), new TestOptionsMonitor<JitElevationPolicyOptions>(policy), TimeProvider.System, securityAlertService);
    }

    private JitElevationService CreateServiceWithTimeProvider(TimeProvider timeProvider, params string[] knownRoles)
    {
        var store = Substitute.For<IRoleStore<IdentityRole>>();
        var roleManager = Substitute.For<RoleManager<IdentityRole>>(store, null, null, null, null);
        roleManager.RoleExistsAsync(Arg.Any<string>())
            .Returns(callInfo => Task.FromResult(knownRoles.Contains(callInfo.Arg<string>())));

        return new JitElevationService(
            CreateAdminDbContextFactory(), roleManager, CreateUserManager(), new TestOptionsMonitor<JitElevationPolicyOptions>(new JitElevationPolicyOptions()), timeProvider, securityAlertService);
    }

    [Fact]
    public async Task RequestElevationAsync_ValidRequest_CreatesPendingRequest()
    {
        var service = CreateService("SystemAdmin");

        var dto = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 60, "Skal rette prisfejl.", TestContext.Current.CancellationToken);

        dto.Status.Should().Be(RoleElevationStatus.Pending);
        dto.RequesterUserId.Should().Be("user-1");
        dto.RequestedMinutes.Should().Be(60);
        dto.ApproverUserId.Should().BeNull();
        dto.ValidFromUtc.Should().BeNull();
    }

    [Theory]
    [InlineData(29)]
    [InlineData(91)]
    public async Task RequestElevationAsync_MinutesOutsideDefaultPolicyRange_ThrowsArgumentOutOfRangeException(int minutes)
    {
        var service = CreateService("SystemAdmin");

        var act = async () => await service.RequestElevationAsync(
            "user-1", "SystemAdmin", minutes, "Test.", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(30)]
    [InlineData(90)]
    public async Task RequestElevationAsync_MinutesAtPolicyBoundaries_Succeeds(int minutes)
    {
        var service = CreateService("SystemAdmin");

        var dto = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", minutes, "Test.", TestContext.Current.CancellationToken);

        dto.RequestedMinutes.Should().Be(minutes);
    }

    [Fact]
    public async Task RequestElevationAsync_CustomPolicy_RespectsConfiguredBounds()
    {
        var policy = new JitElevationPolicyOptions { MinRequestedMinutes = 10, MaxRequestedMinutes = 15 };
        var service = CreateServiceWithPolicy(policy, "SystemAdmin");

        var withinBounds = async () => await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 12, "Test.", TestContext.Current.CancellationToken);
        var outsideBounds = async () => await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 20, "Test.", TestContext.Current.CancellationToken);

        await withinBounds.Should().NotThrowAsync();
        await outsideBounds.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task RequestElevationAsync_MisconfiguredPolicy_ThrowsInvalidOperationException()
    {
        var policy = new JitElevationPolicyOptions { MinRequestedMinutes = 90, MaxRequestedMinutes = 30 };
        var service = CreateServiceWithPolicy(policy, "SystemAdmin");

        var act = async () => await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 45, "Test.", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RequestElevationAsync_UnknownRole_ThrowsInvalidOperationException()
    {
        var service = CreateService();

        var act = async () => await service.RequestElevationAsync(
            "user-1", "GhostRole", 45, "Test.", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ApproveElevationAsync_DifferentApprover_SetsApprovedStatusAndValidityWindow()
    {
        var service = CreateService("SystemAdmin");
        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 45, "Test.", TestContext.Current.CancellationToken);

        var approved = await service.ApproveElevationAsync(
            "user-2", request.Id, TestContext.Current.CancellationToken);

        approved.Status.Should().Be(RoleElevationStatus.Approved);
        approved.ApproverUserId.Should().Be("user-2");
        approved.ValidFromUtc.Should().NotBeNull();
        approved.ValidToUtc.Should().BeCloseTo(approved.ValidFromUtc!.Value.AddMinutes(45), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ApproveElevationAsync_SameUserAsRequester_ThrowsInvalidOperationException()
    {
        var service = CreateService("SystemAdmin");
        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 45, "Test.", TestContext.Current.CancellationToken);

        var act = async () => await service.ApproveElevationAsync(
            "user-1", request.Id, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ApproveElevationAsync_AlreadyApproved_ThrowsInvalidOperationException()
    {
        var service = CreateService("SystemAdmin");
        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 45, "Test.", TestContext.Current.CancellationToken);
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
            "user-1", "SystemAdmin", 45, "Test.", TestContext.Current.CancellationToken);

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
            "user-1", "SystemAdmin", 45, "Test.", TestContext.Current.CancellationToken);

        var act = async () => await service.RejectElevationAsync(
            "user-1", request.Id, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HasActiveElevationAsync_ApprovedAndWithinWindow_ReturnsTrue()
    {
        var service = CreateService("SystemAdmin");
        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 60, "Test.", TestContext.Current.CancellationToken);
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
            "user-1", "SystemAdmin", 45, "Test.", TestContext.Current.CancellationToken);

        var result = await service.HasActiveElevationAsync(
            "user-1", "SystemAdmin", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveElevationAsync_DifferentRole_ReturnsFalse()
    {
        var service = CreateService("SystemAdmin", "DataAdmin");
        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 45, "Test.", TestContext.Current.CancellationToken);
        await service.ApproveElevationAsync("user-2", request.Id, TestContext.Current.CancellationToken);

        var result = await service.HasActiveElevationAsync(
            "user-1", "DataAdmin", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveElevationAsync_ExactlyAtValidToUtc_ReturnsTrue()
    {
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 8, 28, 10, 0, 0, TimeSpan.Zero));
        var service = CreateServiceWithTimeProvider(timeProvider, "SystemAdmin");

        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 60, "Test.", TestContext.Current.CancellationToken);
        await service.ApproveElevationAsync("user-2", request.Id, TestContext.Current.CancellationToken);

        timeProvider.Advance(TimeSpan.FromMinutes(60));

        var result = await service.HasActiveElevationAsync(
            "user-1", "SystemAdmin", TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasActiveElevationAsync_OneSecondAfterValidToUtc_ReturnsFalse()
    {
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 8, 28, 10, 0, 0, TimeSpan.Zero));
        var service = CreateServiceWithTimeProvider(timeProvider, "SystemAdmin");

        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 60, "Test.", TestContext.Current.CancellationToken);
        await service.ApproveElevationAsync("user-2", request.Id, TestContext.Current.CancellationToken);

        timeProvider.Advance(TimeSpan.FromMinutes(60) + TimeSpan.FromSeconds(1));

        var result = await service.HasActiveElevationAsync(
            "user-1", "SystemAdmin", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ApproveElevationAsync_DifferentApprover_SendsSecurityAlert()
    {
        var service = CreateService("SystemAdmin");
        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 45, "Test.", TestContext.Current.CancellationToken);

        await service.ApproveElevationAsync("user-2", request.Id, TestContext.Current.CancellationToken);

        await securityAlertService.Received(1).AlertJitRequestedAsync(
            "user-1", "SystemAdmin", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectElevationAsync_DoesNotSendSecurityAlert()
    {
        var service = CreateService("SystemAdmin");
        var request = await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 45, "Test.", TestContext.Current.CancellationToken);

        await service.RejectElevationAsync("user-2", request.Id, TestContext.Current.CancellationToken);

        await securityAlertService.DidNotReceive().AlertJitRequestedAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RequestElevationAsync_PolicyChangedAfterConstruction_UsesUpdatedBoundsImmediately()
    {
        var monitor = new TestOptionsMonitor<JitElevationPolicyOptions>(
            new JitElevationPolicyOptions { MinRequestedMinutes = 30, MaxRequestedMinutes = 90 });

        var store = Substitute.For<IRoleStore<IdentityRole>>();
        var roleManager = Substitute.For<RoleManager<IdentityRole>>(store, null, null, null, null);
        roleManager.RoleExistsAsync("SystemAdmin").Returns(Task.FromResult(true));

        var service = new JitElevationService(
                    CreateAdminDbContextFactory(), roleManager, CreateUserManager(), monitor, TimeProvider.System, securityAlertService);
        var stillOldBounds = async () => await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 120, "Test.", TestContext.Current.CancellationToken);
        await stillOldBounds.Should().ThrowAsync<ArgumentOutOfRangeException>();

        monitor.Set(new JitElevationPolicyOptions { MinRequestedMinutes = 30, MaxRequestedMinutes = 150 });

        var withinNewBounds = async () => await service.RequestElevationAsync(
            "user-1", "SystemAdmin", 120, "Test.", TestContext.Current.CancellationToken);
        await withinNewBounds.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetRequestsForUserAsync_ReturnsOnlyRequestsForSpecifiedUser()
    {
        var service = CreateService("SystemAdmin");
        await service.RequestElevationAsync("user-1", "SystemAdmin", 45, "Test 1.", TestContext.Current.CancellationToken);
        await service.RequestElevationAsync("user-2", "SystemAdmin", 45, "Test 2.", TestContext.Current.CancellationToken);

        var result = await service.GetRequestsForUserAsync("user-1", TestContext.Current.CancellationToken);

        result.Should().ContainSingle().Which.RequesterUserId.Should().Be("user-1");
    }

    [Fact]
    public async Task GetRequestsForUserAsync_ReturnsAllStatuses_NewestFirst()
    {
        var service = CreateService("SystemAdmin");
        var first = await service.RequestElevationAsync("user-1", "SystemAdmin", 45, "Først.", TestContext.Current.CancellationToken);
        var second = await service.RequestElevationAsync("user-1", "SystemAdmin", 45, "Sidst.", TestContext.Current.CancellationToken);
        await service.RejectElevationAsync("user-2", first.Id, TestContext.Current.CancellationToken);

        var result = await service.GetRequestsForUserAsync("user-1", TestContext.Current.CancellationToken);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(second.Id);
        result.Should().Contain(r => r.Status == RoleElevationStatus.Rejected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetRequestsForUserAsync_MissingUserId_ThrowsArgumentException(string? userId)
    {
        var service = CreateService();

        var act = async () => await service.GetRequestsForUserAsync(userId!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetPendingRequestsForApprovalAsync_ApproverHasMatchingRole_ReturnsRequest()
    {
        var store = Substitute.For<IRoleStore<IdentityRole>>();
        var roleManager = Substitute.For<RoleManager<IdentityRole>>(store, null, null, null, null);
        roleManager.RoleExistsAsync("SystemAdmin").Returns(Task.FromResult(true));

        var service = new JitElevationService(
            CreateAdminDbContextFactory(), roleManager, CreateUserManagerWithRoles("approver-1", "SystemAdmin"),
            new TestOptionsMonitor<JitElevationPolicyOptions>(new JitElevationPolicyOptions()), TimeProvider.System, securityAlertService);

        await service.RequestElevationAsync("requester-1", "SystemAdmin", 45, "Test.", TestContext.Current.CancellationToken);

        var result = await service.GetPendingRequestsForApprovalAsync("approver-1", TestContext.Current.CancellationToken);

        result.Should().ContainSingle().Which.RoleName.Should().Be("SystemAdmin");
    }

    [Fact]
    public async Task GetPendingRequestsForApprovalAsync_ApproverDoesNotHaveMatchingRole_ReturnsEmpty()
    {
        var store = Substitute.For<IRoleStore<IdentityRole>>();
        var roleManager = Substitute.For<RoleManager<IdentityRole>>(store, null, null, null, null);
        roleManager.RoleExistsAsync("SystemAdmin").Returns(Task.FromResult(true));

        var service = new JitElevationService(
            CreateAdminDbContextFactory(), roleManager, CreateUserManagerWithRoles("approver-1", "DataAdmin"),
            new TestOptionsMonitor<JitElevationPolicyOptions>(new JitElevationPolicyOptions()), TimeProvider.System, securityAlertService);

        await service.RequestElevationAsync("requester-1", "SystemAdmin", 45, "Test.", TestContext.Current.CancellationToken);

        var result = await service.GetPendingRequestsForApprovalAsync("approver-1", TestContext.Current.CancellationToken);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPendingRequestsForApprovalAsync_ExcludesOwnRequests_DualCustody()
    {
        var store = Substitute.For<IRoleStore<IdentityRole>>();
        var roleManager = Substitute.For<RoleManager<IdentityRole>>(store, null, null, null, null);
        roleManager.RoleExistsAsync("SystemAdmin").Returns(Task.FromResult(true));

        var service = new JitElevationService(
            CreateAdminDbContextFactory(), roleManager, CreateUserManagerWithRoles("user-1", "SystemAdmin"),
            new TestOptionsMonitor<JitElevationPolicyOptions>(new JitElevationPolicyOptions()), TimeProvider.System, securityAlertService);

        await service.RequestElevationAsync("user-1", "SystemAdmin", 45, "Egen anmodning.", TestContext.Current.CancellationToken);

        var result = await service.GetPendingRequestsForApprovalAsync("user-1", TestContext.Current.CancellationToken);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPendingRequestsForApprovalAsync_ExcludesNonPendingRequests()
    {
        var store = Substitute.For<IRoleStore<IdentityRole>>();
        var roleManager = Substitute.For<RoleManager<IdentityRole>>(store, null, null, null, null);
        roleManager.RoleExistsAsync("SystemAdmin").Returns(Task.FromResult(true));

        var service = new JitElevationService(
            CreateAdminDbContextFactory(), roleManager, CreateUserManagerWithRoles("approver-1", "SystemAdmin"),
            new TestOptionsMonitor<JitElevationPolicyOptions>(new JitElevationPolicyOptions()), TimeProvider.System, securityAlertService);

        var request = await service.RequestElevationAsync("requester-1", "SystemAdmin", 45, "Test.", TestContext.Current.CancellationToken);
        await service.ApproveElevationAsync("approver-1", request.Id, TestContext.Current.CancellationToken);

        var result = await service.GetPendingRequestsForApprovalAsync("approver-1", TestContext.Current.CancellationToken);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPendingRequestsForApprovalAsync_UnknownApprover_ReturnsEmpty_WithoutQueryingRoles()
    {
        var service = CreateService("SystemAdmin");

        var result = await service.GetPendingRequestsForApprovalAsync("ghost-user", TestContext.Current.CancellationToken);

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetPendingRequestsForApprovalAsync_MissingApproverUserId_ThrowsArgumentException(string? approverUserId)
    {
        var service = CreateService();

        var act = async () => await service.GetPendingRequestsForApprovalAsync(approverUserId!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}