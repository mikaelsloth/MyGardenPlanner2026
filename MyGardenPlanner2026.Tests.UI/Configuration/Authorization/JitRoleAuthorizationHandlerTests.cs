namespace MyGardenPlanner2026.Tests.UI.Configuration.Authorization;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Configuration.Authorization;
using MyGardenPlanner2026.Core.Contracts.Admin;
using NSubstitute;
using Xunit;

public class JitRoleAuthorizationHandlerTests
{
    private const string RequiredRole = "SystemAdmin";

    [Fact]
    public async Task HandleRequirementAsync_UserInRole_Succeeds()
    {
        var jitService = Substitute.For<IJitElevationService>();
        var handler = new JitRoleAuthorizationHandler(jitService);
        var context = new AuthorizationHandlerContext(
            [new JitRoleRequirement(RequiredRole)], TestPrincipals.Create("user-1", RequiredRole), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        await jitService.DidNotReceive().HasActiveElevationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleRequirementAsync_NotInRoleButHasActiveElevation_Succeeds()
    {
        var jitService = Substitute.For<IJitElevationService>();
        jitService.HasActiveElevationAsync("user-1", RequiredRole, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var handler = new JitRoleAuthorizationHandler(jitService);
        var context = new AuthorizationHandlerContext(
            [new JitRoleRequirement(RequiredRole)], TestPrincipals.Create("user-1"), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_NotInRoleAndNoActiveElevation_DoesNotSucceed()
    {
        var jitService = Substitute.For<IJitElevationService>();
        jitService.HasActiveElevationAsync("user-1", RequiredRole, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var handler = new JitRoleAuthorizationHandler(jitService);
        var context = new AuthorizationHandlerContext(
            [new JitRoleRequirement(RequiredRole)], TestPrincipals.Create("user-1"), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_NoUserIdClaim_DoesNotSucceed_AndSkipsJitLookup()
    {
        var jitService = Substitute.For<IJitElevationService>();
        var handler = new JitRoleAuthorizationHandler(jitService);
        var context = new AuthorizationHandlerContext(
            [new JitRoleRequirement(RequiredRole)], TestPrincipals.Create(userId: null), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        await jitService.DidNotReceive().HasActiveElevationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}