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

public class AdminJitRequestsPageTests : BunitContext
{
    private static Task<AuthenticationState> CreateAuthStateAsync(string userId = "user-1")
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], authenticationType: "Test");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    private void RegisterFakes()
    {
        var jitService = Substitute.For<IJitElevationService>();
        jitService.GetRequestsForUserAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RoleElevationRequestDto>>([]));
        jitService.GetPendingRequestsForApprovalAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RoleElevationRequestDto>>([]));
        Services.AddSingleton(jitService);

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
    public void AdminJitRequestsPage_RendersTwoTabs()
    {
        RegisterFakes();

        var cut = Render<AdminJitRequestsPage>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.FindAll(".context-tab").Should().HaveCount(2);
    }

    [Fact]
    public void AdminJitRequestsPage_DefaultTab_RendersRequestForm()
    {
        RegisterFakes();

        var cut = Render<AdminJitRequestsPage>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.FindAll("#jit-request-role").Should().HaveCount(1);
    }

    [Fact]
    public void AdminJitRequestsPage_ClickingApproveTab_RendersApprovalQueue()
    {
        RegisterFakes();
        var cut = Render<AdminJitRequestsPage>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.FindAll(".context-tab")[1].Click();

        cut.Markup.Should().Contain("Anmodninger til godkendelse");
    }
}