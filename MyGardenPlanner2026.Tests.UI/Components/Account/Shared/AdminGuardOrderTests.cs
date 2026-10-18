namespace MyGardenPlanner2026.Tests.UI.Components.Account.Shared;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Core.Contracts.Admin;
using NSubstitute;
using System.Security.Claims;
using Xunit;

/// <summary>
/// Låser rækkefølgen, som alle admin-komponenter bruger: StepUpGuard yderst, AdminActionGuard indeni.
/// Rate limit-permit må først forbruges, når step-up er gennemført og handlingen skal udføres.
/// </summary>
public class AdminGuardOrderTests
{
    private const string PolicyName = "RequireRecentAuthentication";

    private static StepUpGuard CreateStepUpGuard(bool reAuthValid)
    {
        var service = Substitute.For<IAuthorizationService>();
        service.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Is(PolicyName))
            .Returns(Task.FromResult(reAuthValid ? AuthorizationResult.Success() : AuthorizationResult.Failed()));
        return new StepUpGuard(service, PolicyName);
    }

    private static IAdminActionRateLimiter CreateRateLimiter(bool permits)
    {
        var limiter = Substitute.For<IAdminActionRateLimiter>();
        limiter.TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(permits));
        return limiter;
    }

    private static Task RunAsync(StepUpGuard stepUp, AdminActionGuard admin, Func<Task> core, Action<string> onRejected)
    {
        var authState = TestPrincipals.CreateAuthStateAsync();
        return stepUp.RunAsync(authState, () => admin.RunAsync(authState, core, onRejected));
    }

    [Fact]
    public async Task ReAuthExpired_ShowsModal_WithoutConsumingPermit_OrExecuting()
    {
        var limiter = CreateRateLimiter(permits: true);
        var stepUp = CreateStepUpGuard(reAuthValid: false);
        var executed = false;

        await RunAsync(stepUp, new AdminActionGuard(limiter), () => { executed = true; return Task.CompletedTask; }, _ => { });

        stepUp.ShowModal.Should().BeTrue();
        executed.Should().BeFalse();
        await limiter.DidNotReceive().TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReAuthExpired_CancelledModal_NeverConsumesPermit()
    {
        var limiter = CreateRateLimiter(permits: true);
        var stepUp = CreateStepUpGuard(reAuthValid: false);

        await RunAsync(stepUp, new AdminActionGuard(limiter), () => Task.CompletedTask, _ => { });
        stepUp.Cancel();
        await stepUp.ExecutePendingActionAsync();

        await limiter.DidNotReceive().TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReAuthExpired_AfterStepUp_ConsumesPermitOnce_AndExecutes()
    {
        var limiter = CreateRateLimiter(permits: true);
        var stepUp = CreateStepUpGuard(reAuthValid: false);
        var executed = false;
        await RunAsync(stepUp, new AdminActionGuard(limiter), () => { executed = true; return Task.CompletedTask; }, _ => { });

        await stepUp.ExecutePendingActionAsync();

        executed.Should().BeTrue();
        await limiter.Received(1).TryAcquireAsync("user-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReAuthExpired_AfterStepUp_PermitDenied_ReportsRateLimit_AndDoesNotExecute()
    {
        var limiter = CreateRateLimiter(permits: false);
        var stepUp = CreateStepUpGuard(reAuthValid: false);
        var executed = false;
        string? message = null;
        await RunAsync(stepUp, new AdminActionGuard(limiter), () => { executed = true; return Task.CompletedTask; }, m => message = m);

        await stepUp.ExecutePendingActionAsync();

        executed.Should().BeFalse();
        message.Should().Be("Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen.");
    }

    [Fact]
    public async Task ReAuthValid_ConsumesPermitOnce_AndExecutes()
    {
        var limiter = CreateRateLimiter(permits: true);
        var executed = false;

        await RunAsync(CreateStepUpGuard(reAuthValid: true), new AdminActionGuard(limiter),
            () => { executed = true; return Task.CompletedTask; }, _ => { });

        executed.Should().BeTrue();
        await limiter.Received(1).TryAcquireAsync("user-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReAuthValid_PermitDenied_ReportsRateLimit_AndDoesNotExecute()
    {
        var limiter = CreateRateLimiter(permits: false);
        var executed = false;
        string? message = null;

        await RunAsync(CreateStepUpGuard(reAuthValid: true), new AdminActionGuard(limiter),
            () => { executed = true; return Task.CompletedTask; }, m => message = m);

        executed.Should().BeFalse();
        message.Should().Contain("For mange handlinger");
    }
}