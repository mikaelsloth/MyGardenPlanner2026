namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Configuration.Authorization;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class RequireRecentAuthenticationHandlerTests
{
    private static ClaimsPrincipal CreatePrincipal() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")], authenticationType: "Test"));

    private static RequireRecentAuthenticationHandler CreateHandler(
        IReAuthenticationService reAuthenticationService, int maxAgeMinutes = 15)
    {
        var monitor = Substitute.For<IOptionsMonitor<ReAuthenticationPolicyOptions>>();
        monitor.CurrentValue.Returns(new ReAuthenticationPolicyOptions { MaxAgeMinutes = maxAgeMinutes });
        return new(reAuthenticationService, monitor);
    }

    [Fact]
    public async Task HandleRequirementAsync_ReAuthValidWithinConfiguredMaxAge_Succeeds()
    {
        var reAuthenticationService = Substitute.For<IReAuthenticationService>();
        reAuthenticationService.IsReAuthValid(TimeSpan.FromMinutes(15)).Returns(true);

        var handler = CreateHandler(reAuthenticationService);
        var context = new AuthorizationHandlerContext(
            [new RequireRecentAuthenticationRequirement()], CreatePrincipal(), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ReAuthExpired_DoesNotSucceed()
    {
        var reAuthenticationService = Substitute.For<IReAuthenticationService>();
        reAuthenticationService.IsReAuthValid(TimeSpan.FromMinutes(15)).Returns(false);

        var handler = CreateHandler(reAuthenticationService);
        var context = new AuthorizationHandlerContext(
            [new RequireRecentAuthenticationRequirement()], CreatePrincipal(), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_UsesConfiguredMaxAge_NotHardcodedDefault()
    {
        var reAuthenticationService = Substitute.For<IReAuthenticationService>();
        reAuthenticationService.IsReAuthValid(TimeSpan.FromMinutes(5)).Returns(true);

        var handler = CreateHandler(reAuthenticationService, maxAgeMinutes: 5);
        var context = new AuthorizationHandlerContext(
            [new RequireRecentAuthenticationRequirement()], CreatePrincipal(), resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        reAuthenticationService.Received(1).IsReAuthValid(TimeSpan.FromMinutes(5));
    }
}