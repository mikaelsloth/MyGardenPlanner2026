namespace MyGardenPlanner2026.Tests.UI.Components.Account.Shared;

using FluentAssertions;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Core.Contracts.Admin;
using NSubstitute;
using Xunit;

public class AdminActionGuardTests
{
    [Fact]
    public async Task RunAsync_PermitAcquired_ExecutesActionImmediately_AndDoesNotSetRateLimited()
    {
        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        rateLimiter.TryAcquireAsync("user-1", Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        var guard = new AdminActionGuard(rateLimiter);
        var executed = false;

        await guard.RunAsync(TestPrincipals.CreateAuthStateAsync(), () => { executed = true; return Task.CompletedTask; });

        executed.Should().BeTrue();
        guard.IsRateLimited.Should().BeFalse();
    }

    [Fact]
    public async Task RunAsync_PermitDenied_DoesNotExecuteAction_AndSetsRateLimited()
    {
        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        rateLimiter.TryAcquireAsync("user-1", Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        var guard = new AdminActionGuard(rateLimiter);
        var executed = false;

        await guard.RunAsync(TestPrincipals.CreateAuthStateAsync(), () => { executed = true; return Task.CompletedTask; });

        executed.Should().BeFalse();
        guard.IsRateLimited.Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_NullAuthenticationStateTask_SetsUserUnknown_NotRateLimited_WithoutCallingLimiter()
    {
        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        var guard = new AdminActionGuard(rateLimiter);
        var executed = false;

        await guard.RunAsync(null, () => { executed = true; return Task.CompletedTask; });

        executed.Should().BeFalse();
        guard.IsUserUnknown.Should().BeTrue();
        guard.IsRateLimited.Should().BeFalse();
        await rateLimiter.DidNotReceive().TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_PassesUserIdFromClaimsPrincipal_ToRateLimiter()
    {
        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        rateLimiter.TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        var guard = new AdminActionGuard(rateLimiter);

        await guard.RunAsync(TestPrincipals.CreateAuthStateAsync("user-42"), () => Task.CompletedTask);

        await rateLimiter.Received(1).TryAcquireAsync("user-42", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_SecondCallAfterDenial_ResetsIsRateLimitedIfPermitted()
    {
        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        rateLimiter.TryAcquireAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false), Task.FromResult(true));
        var guard = new AdminActionGuard(rateLimiter);

        await guard.RunAsync(TestPrincipals.CreateAuthStateAsync(), () => Task.CompletedTask);
        guard.IsRateLimited.Should().BeTrue();

        await guard.RunAsync(TestPrincipals.CreateAuthStateAsync(), () => Task.CompletedTask);
        guard.IsRateLimited.Should().BeFalse();
    }

    [Fact]
    public async Task RunAsync_AuthenticatedUserWithoutNameIdentifier_SetsUserUnknown_WithoutCallingLimiter()
    {
        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        var guard = new AdminActionGuard(rateLimiter);

        await guard.RunAsync(TestPrincipals.CreateAuthStateAsync(userId: null), () => Task.CompletedTask);

        guard.IsUserUnknown.Should().BeTrue();
        guard.IsRateLimited.Should().BeFalse();
        await rateLimiter.DidNotReceive().TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_UserUnknown_InvokesOnRejectedWithUserUnknownMessage()
    {
        var guard = new AdminActionGuard(Substitute.For<IAdminActionRateLimiter>());
        string? message = null;

        await guard.RunAsync(null, () => Task.CompletedTask, m => message = m);

        message.Should().Be("Error: Kunne ikke bestemme den aktuelle bruger.");
    }

    [Fact]
    public async Task RunAsync_PermitDenied_InvokesOnRejectedWithRateLimitMessage()
    {
        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        rateLimiter.TryAcquireAsync("user-1", Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        var guard = new AdminActionGuard(rateLimiter);
        string? message = null;

        await guard.RunAsync(TestPrincipals.CreateAuthStateAsync(), () => Task.CompletedTask, m => message = m);

        message.Should().Be("Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen.");
    }

    [Fact]
    public async Task RunAsync_PermitAcquired_DoesNotInvokeOnRejected()
    {
        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        rateLimiter.TryAcquireAsync("user-1", Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        var guard = new AdminActionGuard(rateLimiter);
        var rejected = false;

        await guard.RunAsync(TestPrincipals.CreateAuthStateAsync(), () => Task.CompletedTask, _ => rejected = true);

        rejected.Should().BeFalse();
    }

    [Fact]
    public async Task RunAsync_AfterUserUnknown_ResetsFlagsOnNextPermittedCall()
    {
        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        rateLimiter.TryAcquireAsync("user-1", Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        var guard = new AdminActionGuard(rateLimiter);

        await guard.RunAsync(null, () => Task.CompletedTask);
        guard.IsUserUnknown.Should().BeTrue();

        await guard.RunAsync(TestPrincipals.CreateAuthStateAsync(), () => Task.CompletedTask);
        guard.IsUserUnknown.Should().BeFalse();
        guard.IsRateLimited.Should().BeFalse();
    }
}