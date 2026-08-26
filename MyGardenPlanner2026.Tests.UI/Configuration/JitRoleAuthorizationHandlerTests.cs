namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Configuration.Authorization;
using MyGardenPlanner2026.Core.Contracts.Admin;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class JitRoleAuthorizationHandlerTests
{
    private const string RequiredRole = "SystemAdmin";

    private static ClaimsPrincipal CreatePrincipal(string? userId, bool inRole)
    {
        var claims = new List<Claim>();
        if (userId is not null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }
        if (inRole)
        {
            claims.Add(new Claim(ClaimTypes.Role, RequiredRole));
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "Test");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserInRole_Succeeds()
    {
        var jitService = Substitute.For<IJitElevationService>();
        var handler = new JitRoleAuthorizationHandler(jitService);
        var context = new AuthorizationHandlerContext(
            [new JitRoleRequirement(RequiredRole)], CreatePrincipal("user-1", inRole: true), resource: null);

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
            [new JitRoleRequirement(RequiredRole)], CreatePrincipal("user-1", inRole: false), resource: null);

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
            [new JitRoleRequirement(RequiredRole)], CreatePrincipal("user-1", inRole: false), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_NoUserIdClaim_DoesNotSucceed_AndSkipsJitLookup()
    {
        var jitService = Substitute.For<IJitElevationService>();
        var handler = new JitRoleAuthorizationHandler(jitService);
        var context = new AuthorizationHandlerContext(
            [new JitRoleRequirement(RequiredRole)], CreatePrincipal(userId: null, inRole: false), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        await jitService.DidNotReceive().HasActiveElevationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}