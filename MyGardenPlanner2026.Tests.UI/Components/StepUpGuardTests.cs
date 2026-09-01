namespace MyGardenPlanner2026.Tests.UI.Components;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class StepUpGuardTests
{
    private const string PolicyName = "RequireRecentAuthentication";

    private static Task<AuthenticationState> CreateAuthStateAsync()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")], authenticationType: "Test");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    private static IAuthorizationService CreateAuthorizationService(bool succeeds)
    {
        var service = Substitute.For<IAuthorizationService>();
        service.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Is(PolicyName))
            .Returns(Task.FromResult(succeeds ? AuthorizationResult.Success() : AuthorizationResult.Failed()));
        return service;
    }

    [Fact]
    public async Task RunAsync_ReAuthValid_ExecutesActionImmediately_AndDoesNotShowModal()
    {
        var guard = new StepUpGuard(CreateAuthorizationService(succeeds: true), PolicyName);
        var executed = false;

        await guard.RunAsync(CreateAuthStateAsync(), () => { executed = true; return Task.CompletedTask; });

        executed.Should().BeTrue();
        guard.ShowModal.Should().BeFalse();
    }

    [Fact]
    public async Task RunAsync_ReAuthExpired_DoesNotExecuteAction_AndShowsModal()
    {
        var guard = new StepUpGuard(CreateAuthorizationService(succeeds: false), PolicyName);
        var executed = false;

        await guard.RunAsync(CreateAuthStateAsync(), () => { executed = true; return Task.CompletedTask; });

        executed.Should().BeFalse();
        guard.ShowModal.Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_NullAuthenticationStateTask_TreatsAsUnauthenticated_AndShowsModal()
    {
        var guard = new StepUpGuard(CreateAuthorizationService(succeeds: true), PolicyName);
        var executed = false;

        await guard.RunAsync(null, () => { executed = true; return Task.CompletedTask; });

        executed.Should().BeFalse();
        guard.ShowModal.Should().BeTrue();
    }

    [Fact]
    public async Task ExecutePendingActionAsync_AfterDeferredRun_ExecutesPendingActionAndHidesModal()
    {
        var guard = new StepUpGuard(CreateAuthorizationService(succeeds: false), PolicyName);
        var executed = false;
        await guard.RunAsync(CreateAuthStateAsync(), () => { executed = true; return Task.CompletedTask; });

        await guard.ExecutePendingActionAsync();

        executed.Should().BeTrue();
        guard.ShowModal.Should().BeFalse();
    }

    [Fact]
    public async Task ExecutePendingActionAsync_CalledTwice_OnlyExecutesActionOnce()
    {
        var guard = new StepUpGuard(CreateAuthorizationService(succeeds: false), PolicyName);
        var executionCount = 0;
        await guard.RunAsync(CreateAuthStateAsync(), () => { executionCount++; return Task.CompletedTask; });

        await guard.ExecutePendingActionAsync();
        await guard.ExecutePendingActionAsync();

        executionCount.Should().Be(1);
    }

    [Fact]
    public async Task Cancel_AfterDeferredRun_DiscardsPendingAction_AndHidesModal()
    {
        var guard = new StepUpGuard(CreateAuthorizationService(succeeds: false), PolicyName);
        var executed = false;
        await guard.RunAsync(CreateAuthStateAsync(), () => { executed = true; return Task.CompletedTask; });

        guard.Cancel();
        await guard.ExecutePendingActionAsync();

        executed.Should().BeFalse();
        guard.ShowModal.Should().BeFalse();
    }

    [Fact]
    public async Task RunAsync_PassesConfiguredPolicyName_ToAuthorizationService()
    {
        var authorizationService = CreateAuthorizationService(succeeds: true);
        var guard = new StepUpGuard(authorizationService, PolicyName);

        await guard.RunAsync(CreateAuthStateAsync(), () => Task.CompletedTask);

        await authorizationService.Received(1).AuthorizeAsync(
            Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Is(PolicyName));
    }
}