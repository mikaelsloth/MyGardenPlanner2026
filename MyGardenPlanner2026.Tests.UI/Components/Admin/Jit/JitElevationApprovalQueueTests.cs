namespace MyGardenPlanner2026.Tests.UI.Components.Admin.Jit;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using Xunit;

public class JitElevationApprovalQueueTests : BunitContext
{
    private static readonly Guid Request1Id = Guid.Parse("66666666-6666-6666-6666-666666666666");

    private static readonly RoleElevationRequestDto Request1 = new(
        Id: Request1Id,
        RequesterUserId: "requester-1",
        ApproverUserId: null,
        RoleName: "SystemAdmin",
        Status: RoleElevationStatus.Pending,
        Reason: "Skal rette en fejlkonfigureret rabat-trappe.",
        RequestedMinutes: 45,
        ValidFromUtc: null,
        ValidToUtc: null,
        CreatedAtUtc: DateTimeOffset.UtcNow);

    private IJitElevationService RegisterFakes(
        bool reAuthSucceeds = true, bool rateLimiterPermits = true, IReadOnlyList<RoleElevationRequestDto>? pendingRequests = null)
    {
        var service = Substitute.For<IJitElevationService>();
        service.GetPendingRequestsForApprovalAsync("approver-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(pendingRequests ?? [Request1]));

        // NSubstitute returnerer null for uopstillede metoder der returnerer Task<T> (reference type),
        // hvilket giver NullReferenceException ved await. Stub derfor altid et default-svar.
        service.ApproveElevationAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Request1 with { Status = RoleElevationStatus.Approved, ApproverUserId = "approver-1" }));
        service.RejectElevationAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Request1 with { Status = RoleElevationStatus.Rejected, ApproverUserId = "approver-1" }));

        Services.AddSingleton(service);
        this.RegisterAdminStepUpFakes<IJitElevationService>(reAuthSucceeds, rateLimiterPermits);

        return service;
    }

    [Fact]
    public void JitElevationApprovalQueue_RendersOneRowPerPendingRequest()
    {
        RegisterFakes();

        var cut = Render<JitElevationApprovalQueue>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync("approver-1")));

        cut.FindAll("tbody tr").Should().HaveCount(1);
        cut.Markup.Should().Contain("requester-1");
        cut.Markup.Should().Contain("SystemAdmin");
    }

    [Fact]
    public void JitElevationApprovalQueue_NoPendingRequests_ShowsEmptyMessage()
    {
        RegisterFakes(pendingRequests: []);

        var cut = Render<JitElevationApprovalQueue>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync("approver-1")));

        cut.Markup.Should().Contain("Ingen ventende anmodninger");
    }

    [Fact]
    public void ReAuthValid_ClickingGodkend_CallsApproveElevationAsyncImmediately_WithoutOpeningModal()
    {
        var service = RegisterFakes(reAuthSucceeds: true);

        var cut = Render<JitElevationApprovalQueue>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync("approver-1")));
        cut.Find("button.btn-primary").Click();

        _ = service.Received().ApproveElevationAsync("approver-1", Request1Id, Arg.Any<CancellationToken>());
        cut.ShouldNotShowStepUpModal();
    }

    [Fact]
    public void ReAuthValid_ClickingGodkend_InvokesOnStatusMessage()
    {
        var service = RegisterFakes(reAuthSucceeds: true);
        string? receivedMessage = null;

        var cut = Render<JitElevationApprovalQueue>(p => p
            .Add(x => x.OnStatusMessage, EventCallback.Factory.Create<string>(this, m => receivedMessage = m))
            .AddCascadingValue(TestPrincipals.CreateAuthStateAsync("approver-1")));

        cut.Find("button.btn-primary").Click();

        receivedMessage.Should().NotBeNull();
        receivedMessage.Should().Contain("godkendt");
    }

    [Fact]
    public void ReAuthValid_ClickingAfvis_CallsRejectElevationAsyncImmediately_WithoutOpeningModal()
    {
        var service = RegisterFakes(reAuthSucceeds: true);

        var cut = Render<JitElevationApprovalQueue>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync("approver-1")));
        cut.Find("button.btn-danger").Click();

        _ = service.Received().RejectElevationAsync("approver-1", Request1Id, Arg.Any<CancellationToken>());
        cut.ShouldNotShowStepUpModal();
    }

    [Fact]
    public void ReAuthExpired_ClickingGodkend_OpensStepUpModal_WithoutApproving()
    {
        var service = RegisterFakes(reAuthSucceeds: false);

        var cut = Render<JitElevationApprovalQueue>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync("approver-1")));
        cut.Find("button.btn-primary").Click();

        cut.ShouldShowStepUpModal();
        _ = service.DidNotReceive().ApproveElevationAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_ClickingAfvis_OpensStepUpModal_WithoutRejecting()
    {
        var service = RegisterFakes(reAuthSucceeds: false);

        var cut = Render<JitElevationApprovalQueue>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync("approver-1")));
        cut.Find("button.btn-danger").Click();

        cut.ShouldShowStepUpModal();
        _ = service.DidNotReceive().RejectElevationAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_CancellingStepUpModal_ClosesModal_WithoutApproving()
    {
        var service = RegisterFakes(reAuthSucceeds: false);

        var cut = Render<JitElevationApprovalQueue>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync("approver-1")));
        cut.Find("button.btn-primary").Click();
        cut.Find(".confirm-dialog button.btn-secondary").Click();

        cut.ShouldNotShowStepUpModal();
        _ = service.DidNotReceive().ApproveElevationAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void RateLimited_ClickingGodkend_DoesNotCallApproveElevationAsync_AndShowsErrorMessage()
    {
        var service = RegisterFakes(reAuthSucceeds: true, rateLimiterPermits: false);

        var cut = Render<JitElevationApprovalQueue>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync("approver-1")));
        cut.Find("button.btn-primary").Click();

        _ = service.DidNotReceive().ApproveElevationAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        cut.ShouldShowRateLimitMessage();
    }

    [Fact]
    public void ApproveCoreAsync_ServiceThrowsInvalidOperationException_ShowsErrorMessage()
    {
        var service = RegisterFakes(reAuthSucceeds: true);
        service.ApproveElevationAsync("approver-1", Request1Id, Arg.Any<CancellationToken>())
            .Returns<RoleElevationRequestDto>(_ => throw new InvalidOperationException(
                "Anmodningen kan ikke godkendes af ansøgeren selv (dual-custody / peer approval)."));

        var cut = Render<JitElevationApprovalQueue>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync("approver-1")));
        cut.Find("button.btn-primary").Click();

        cut.Markup.Should().Contain("dual-custody");
    }
}