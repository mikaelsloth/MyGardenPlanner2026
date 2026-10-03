namespace MyGardenPlanner2026.Tests.UI.Internal_Tests;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities;
using System.Security.Claims;
using Xunit;

public class AdminSecurityTestDoublesTests : BunitContext
{
    [Fact]
    public void AddAdminSecurityServices_RegistersAllExpectedServices()
    {
        this.AddAdminSecurityServices();

        Services.GetService<IAuthorizationService>().Should().NotBeNull();
        Services.GetService<UserManager<ApplicationUser>>().Should().NotBeNull();
        Services.GetService<IReAuthenticationService>().Should().NotBeNull();
        Services.GetService<IReAuthFailureTracker>().Should().NotBeNull();
        Services.GetService<ICurrentUserAccessor>().Should().NotBeNull();
        Services.GetService<IAdminActionRateLimiter>().Should().NotBeNull();
    }

    [Fact]
    public async Task AddAdminSecurityServices_ReAuthSucceedsTrue_AuthorizesSuccessfully()
    {
        var (authService, _) = this.AddAdminSecurityServices(reAuthSucceeds: true);

        var result = await authService.AuthorizeAsync(
            new ClaimsPrincipal(), null, AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task AddAdminSecurityServices_ReAuthSucceedsFalse_AuthorizationFails()
    {
        var (authService, _) = this.AddAdminSecurityServices(reAuthSucceeds: false);

        var result = await authService.AuthorizeAsync(
            new ClaimsPrincipal(), null, AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task AddAdminSecurityServices_RateLimitAllowedFalse_RejectsAcquisition()
    {
        var (_, rateLimiter) = this.AddAdminSecurityServices(rateLimitAllowed: false);

        var allowed = await rateLimiter.TryAcquireAsync("test-key", CancellationToken.None);

        allowed.Should().BeFalse();
    }

    [Fact]
    public async Task SetReAuthResult_UpdatesExistingAuthorizationService()
    {
        var (authService, _) = this.AddAdminSecurityServices(reAuthSucceeds: true);
        authService.SetReAuthResult(false);

        var result = await authService.AuthorizeAsync(
            new ClaimsPrincipal(), null, AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy);

        result.Succeeded.Should().BeFalse();
    }
}