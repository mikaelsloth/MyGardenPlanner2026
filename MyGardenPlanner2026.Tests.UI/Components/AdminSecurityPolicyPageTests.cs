namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Pages.Admin;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class AdminSecurityPolicyPageTests : BunitContext
{
    private static Task<AuthenticationState> CreateAuthStateAsync()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")], authenticationType: "Test");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    private void RegisterFakes()
    {
        var jitService = Substitute.For<IJitElevationPolicyAdminService>();
        jitService.GetAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(new JitElevationPolicyDto(30, 90, 5)));
        Services.AddSingleton(jitService);

        var reAuthService = Substitute.For<IReAuthenticationPolicyAdminService>();
        reAuthService.GetAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(new ReAuthenticationPolicyDto(15)));
        Services.AddSingleton(reAuthService);

        var failureTrackerService = Substitute.For<IReAuthFailureTrackerPolicyAdminService>();
        failureTrackerService.GetAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(new ReAuthFailureTrackerPolicyDto(5, 2)));
        Services.AddSingleton(failureTrackerService);

        var adminApiService = Substitute.For<IAdminApiRateLimitPolicyAdminService>();
        adminApiService.GetAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(new AdminApiRateLimitPolicyDto(100, 60, 6)));
        Services.AddSingleton(adminApiService);

        var loginService = Substitute.For<ILoginRateLimitPolicyAdminService>();
        loginService.GetAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(new LoginRateLimitPolicyDto(5, 60)));
        Services.AddSingleton(loginService);

        var authorizationService = Substitute.For<IAuthorizationService>();
        authorizationService.AuthorizeAsync(
                Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Is(AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy))
            .Returns(Task.FromResult(AuthorizationResult.Success()));
        Services.AddSingleton(authorizationService);

        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(null));
        Services.AddSingleton(userManager);

        Services.AddSingleton(Substitute.For<IReAuthenticationService>());
        Services.AddSingleton(Substitute.For<IReAuthFailureTracker>());
        Services.AddSingleton(Substitute.For<ICurrentUserAccessor>());

        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        rateLimiter.TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        Services.AddSingleton(rateLimiter);
    }

    [Fact]
    public void AdminSecurityPolicyPage_RendersFiveTabs()
    {
        RegisterFakes();

        var cut = Render<AdminSecurityPolicyPage>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.FindAll(".context-tab").Should().HaveCount(5);
    }

    [Fact]
    public void AdminSecurityPolicyPage_DefaultTab_RendersJitElevationEditor()
    {
        RegisterFakes();

        var cut = Render<AdminSecurityPolicyPage>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.FindAll("#jit-min").Should().HaveCount(1);
    }

    [Fact]
    public void AdminSecurityPolicyPage_ClickingLoginRateLimitTab_RendersLoginRateLimitEditor()
    {
        RegisterFakes();
        var cut = Render<AdminSecurityPolicyPage>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.FindAll(".context-tab")[4].Click();

        cut.FindAll("#login-permit").Should().HaveCount(1);
    }
}