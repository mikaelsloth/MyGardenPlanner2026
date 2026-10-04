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
    public async Task RunAsync_NullAuthenticationStateTask_TreatsAsRateLimited_WithoutCallingLimiter()
    {
        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        var guard = new AdminActionGuard(rateLimiter);
        var executed = false;

        await guard.RunAsync(null, () => { executed = true; return Task.CompletedTask; });

        executed.Should().BeFalse();
        guard.IsRateLimited.Should().BeTrue();
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
}