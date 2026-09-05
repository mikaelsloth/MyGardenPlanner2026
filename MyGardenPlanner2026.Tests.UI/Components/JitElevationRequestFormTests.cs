namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class JitElevationRequestFormTests : BunitContext
{
    private static Task<AuthenticationState> CreateAuthStateAsync(string userId = "user-1")
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], authenticationType: "Test");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    private static RoleElevationRequestDto CreateDto(RoleElevationStatus status, string roleName = "SystemAdmin") => new(
        Id: Guid.NewGuid(),
        RequesterUserId: "user-1",
        ApproverUserId: status == RoleElevationStatus.Pending ? null : "approver-1",
        RoleName: roleName,
        Status: status,
        Reason: "Test.",
        RequestedMinutes: 45,
        ValidFromUtc: null,
        ValidToUtc: null,
        CreatedAtUtc: DateTimeOffset.UtcNow);

    private IJitElevationService RegisterFakes(
        IReadOnlyList<RoleElevationRequestDto>? initialRequests = null, bool rateLimiterPermits = true)
    {
        var service = Substitute.For<IJitElevationService>();
        service.GetRequestsForUserAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(initialRequests ?? (IReadOnlyList<RoleElevationRequestDto>)[]));
        Services.AddSingleton(service);

        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        rateLimiter.TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(rateLimiterPermits));
        Services.AddSingleton(rateLimiter);

        return service;
    }

    [Fact]
    public void JitElevationRequestForm_RendersFourRoleOptions()
    {
        RegisterFakes();

        var cut = Render<JitElevationRequestForm>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.FindAll("#jit-request-role option").Should().HaveCount(4);
    }

    [Fact]
    public void JitElevationRequestForm_DefaultView_ExcludesRejectedAndExpiredRequests()
    {
        RegisterFakes(
        [
            CreateDto(RoleElevationStatus.Pending),
            CreateDto(RoleElevationStatus.Approved),
            CreateDto(RoleElevationStatus.Rejected),
            CreateDto(RoleElevationStatus.Expired)
        ]);

        var cut = Render<JitElevationRequestForm>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.FindAll("tbody tr").Should().HaveCount(2);
        cut.Markup.Should().Contain("Afventer");
        cut.Markup.Should().Contain("Godkendt");
        cut.Markup.Should().NotContain("Afvist");
        cut.Markup.Should().NotContain("Udløbet");
    }

    [Fact]
    public void TogglingShowHistory_AlsoRendersRejectedAndExpiredRequests()
    {
        RegisterFakes(
        [
            CreateDto(RoleElevationStatus.Pending),
            CreateDto(RoleElevationStatus.Rejected),
            CreateDto(RoleElevationStatus.Expired)
        ]);

        var cut = Render<JitElevationRequestForm>(p => p.AddCascadingValue(CreateAuthStateAsync()));
        cut.Find("#jit-request-show-history").Change(true);

        cut.FindAll("tbody tr").Should().HaveCount(3);
        cut.Markup.Should().Contain("Afvist");
        cut.Markup.Should().Contain("Udløbet");
    }

    [Fact]
    public void SubmitRequestAsync_ValidInput_CallsRequestElevationAsyncWithCurrentUserId()
    {
        var service = RegisterFakes();

        var cut = Render<JitElevationRequestForm>(p => p.AddCascadingValue(CreateAuthStateAsync()));
        cut.Find("#jit-request-minutes").Change("45");
        cut.Find("#jit-request-reason").Change("Skal rette en fejl.");
        cut.Find("button.btn-primary").Click();

        _ = service.Received().RequestElevationAsync(
            "user-1", RoleNames.SystemAdmin, 45, "Skal rette en fejl.", Arg.Any<CancellationToken>());
    }

    [Fact]
    public void SubmitRequestAsync_ServiceThrowsArgumentOutOfRangeException_ShowsErrorMessage()
    {
        var service = RegisterFakes();
        service.RequestElevationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<RoleElevationRequestDto>(_ => throw new ArgumentOutOfRangeException(
                "minutes", "RequestedMinutes skal være mellem 30 og 90."));

        var cut = Render<JitElevationRequestForm>(p => p.AddCascadingValue(CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();

        cut.Markup.Should().Contain("RequestedMinutes skal være mellem 30 og 90.");
    }

    [Fact]
    public void RateLimited_Submitting_DoesNotCallRequestElevationAsync_AndShowsErrorMessage()
    {
        var service = RegisterFakes(rateLimiterPermits: false);

        var cut = Render<JitElevationRequestForm>(p => p.AddCascadingValue(CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();

        _ = service.DidNotReceive().RequestElevationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        cut.Markup.Should().Contain("For mange handlinger");
    }
}