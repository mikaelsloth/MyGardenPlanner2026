namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Layout;
using MyGardenPlanner2026.Configuration.Extensions;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class AdminNavMenuTests : BunitContext
{
    private readonly IAuthorizationService authorizationService = Substitute.For<IAuthorizationService>();

    public AdminNavMenuTests()
    {
        Services.AddSingleton(authorizationService);

        // Default: alle policies fejler, medmindre andet er sat op eksplicit i den enkelte test.
        authorizationService
            .AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<string>())
            .Returns(AuthorizationResult.Failed());
    }

    private void SetSucceedingPolicies(params string[] policyNames)
    {
        foreach (var policy in policyNames)
        {
            authorizationService
                .AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Is<string>(p => p == policy))
                .Returns(AuthorizationResult.Success());
        }
    }

    private IRenderedComponent<AdminNavMenu> RenderWithAuthState(ClaimsPrincipal user)
    {
        var authState = Task.FromResult(new AuthenticationState(user));

        return Render<AdminNavMenu>(parameters => parameters
            .AddCascadingValue(authState));
    }

    private static ClaimsPrincipal AuthenticatedUser() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")], "TestAuth"));

    [Fact]
    public void AllPoliciesSucceed_RendersAllFourNavLinks()
    {
        SetSucceedingPolicies(
            AuthorizationServicesExtensions.RequireGlobalAdminPolicy,
            AuthorizationServicesExtensions.RequirePolicyAdminPolicy,
            AuthorizationServicesExtensions.RequireAnyAdminRolePolicy,
            AuthorizationServicesExtensions.RequireAuditViewerPolicy);

        var cut = RenderWithAuthState(AuthenticatedUser());

        cut.FindAll("a.nav-link-item").Should().HaveCount(4);
    }

    [Fact]
    public void OnlyAuditViewerPolicySucceeds_RendersOnlyAuditLogLink()
    {
        SetSucceedingPolicies(AuthorizationServicesExtensions.RequireAuditViewerPolicy);

        var cut = RenderWithAuthState(AuthenticatedUser());

        var link = cut.FindAll("a.nav-link-item").Should().ContainSingle().Subject;
        link.TextContent.Trim().Should().Be("AuditLog");
        link.GetAttribute("href").Should().Be("admin/audit-log");
    }

    [Fact]
    public void NoPoliciesSucceed_RendersNoLinks()
    {
        var cut = RenderWithAuthState(AuthenticatedUser());

        cut.FindAll("a.nav-link-item").Should().BeEmpty();
    }

    [Fact]
    public void NullAuthenticationStateTask_RendersNoLinks_WithoutCallingAuthorizationService()
    {
        var cut = Render<AdminNavMenu>();

        cut.FindAll("a.nav-link-item").Should().BeEmpty();
        _ = authorizationService.DidNotReceive().AuthorizeAsync(
            Arg.Any<ClaimsPrincipal>(), Arg.Any<object?>(), Arg.Any<string>());
    }

    [Fact]
    public void JitRequestsLink_UsesAnyAdminRolePolicy()
    {
        // Bekræfter blot at nav-menuen spørger på RequireAnyAdminRolePolicy for
        // JIT-siden -- selve JIT-vs-direkte-rolle-logikken ligger i
        // AnyAdminRoleAuthorizationHandler og duplikeres bevidst ikke her.
        SetSucceedingPolicies(AuthorizationServicesExtensions.RequireAnyAdminRolePolicy);

        var cut = RenderWithAuthState(AuthenticatedUser());

        var link = cut.FindAll("a.nav-link-item").Should().ContainSingle().Subject;
        link.GetAttribute("href").Should().Be("admin/jit-requests");
    }
}