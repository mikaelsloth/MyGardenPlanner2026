namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Configuration.Authorization;
using MyGardenPlanner2026.Core.Contracts.Admin;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class AnyAdminRoleAuthorizationHandlerTests
{
    private static readonly string[] AdminRoles = ["SystemAdmin", "DataAdmin", "PolicyAdmin", "AuditViewer"];

    private static ClaimsPrincipal CreatePrincipal(string? userId, params string[] roles)
    {
        var claims = new List<Claim>();
        if (userId is not null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "Test");
        return new ClaimsPrincipal(identity);
    }

    [Theory]
    [InlineData("SystemAdmin")]
    [InlineData("DataAdmin")]
    [InlineData("PolicyAdmin")]
    [InlineData("AuditViewer")]
    public async Task HandleRequirementAsync_UserInAnyAdminRole_Succeeds(string role)
    {
        var jitService = Substitute.For<IJitElevationService>();
        var handler = new AnyAdminRoleAuthorizationHandler(jitService);
        var context = new AuthorizationHandlerContext(
            [new AnyAdminRoleRequirement(AdminRoles)], CreatePrincipal("user-1", role), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        await jitService.DidNotReceive().HasActiveElevationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleRequirementAsync_NotInAnyRoleButHasActiveElevationToOne_Succeeds()
    {
        var jitService = Substitute.For<IJitElevationService>();
        jitService.HasActiveElevationAsync("user-1", "PolicyAdmin", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var handler = new AnyAdminRoleAuthorizationHandler(jitService);
        var context = new AuthorizationHandlerContext(
            [new AnyAdminRoleRequirement(AdminRoles)], CreatePrincipal("user-1"), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_NotInAnyRoleAndNoActiveElevation_DoesNotSucceed()
    {
        var jitService = Substitute.For<IJitElevationService>();
        jitService.HasActiveElevationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var handler = new AnyAdminRoleAuthorizationHandler(jitService);
        var context = new AuthorizationHandlerContext(
            [new AnyAdminRoleRequirement(AdminRoles)], CreatePrincipal("user-1"), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_NoUserIdClaim_DoesNotSucceed_AndSkipsJitLookup()
    {
        var jitService = Substitute.For<IJitElevationService>();
        var handler = new AnyAdminRoleAuthorizationHandler(jitService);
        var context = new AuthorizationHandlerContext(
            [new AnyAdminRoleRequirement(AdminRoles)], CreatePrincipal(userId: null), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        await jitService.DidNotReceive().HasActiveElevationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleRequirementAsync_ChecksElevationForEachRoleUntilOneSucceeds()
    {
        var jitService = Substitute.For<IJitElevationService>();
        jitService.HasActiveElevationAsync("user-1", "SystemAdmin", Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        jitService.HasActiveElevationAsync("user-1", "DataAdmin", Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        jitService.HasActiveElevationAsync("user-1", "PolicyAdmin", Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));

        var handler = new AnyAdminRoleAuthorizationHandler(jitService);
        var context = new AuthorizationHandlerContext(
            [new AnyAdminRoleRequirement(AdminRoles)], CreatePrincipal("user-1"), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        await jitService.DidNotReceive().HasActiveElevationAsync("user-1", "AuditViewer", Arg.Any<CancellationToken>());
    }
}