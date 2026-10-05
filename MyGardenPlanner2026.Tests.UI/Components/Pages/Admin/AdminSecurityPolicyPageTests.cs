namespace MyGardenPlanner2026.Tests.UI.Components.Pages.Admin;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Pages.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using Xunit;

public class AdminSecurityPolicyPageTests : BunitContext
{
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

        var auditLogExportJobService = Substitute.For<IAuditLogExportJobPolicyAdminService>();
        auditLogExportJobService.GetAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(new AuditLogExportJobPolicyDto(24, 3)));
        Services.AddSingleton(auditLogExportJobService);

        this.RegisterAdminStepUpFakes<AdminSecurityPolicyPage>();
    }

    [Fact]
    public void AdminSecurityPolicyPage_RendersSixTabs()
    {
        RegisterFakes();

        var cut = Render<AdminSecurityPolicyPage>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.FindAll(".context-tab").Should().HaveCount(6);
    }

    [Fact]
    public void AdminSecurityPolicyPage_DefaultTab_RendersJitElevationEditor()
    {
        RegisterFakes();

        var cut = Render<AdminSecurityPolicyPage>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.FindAll("#jit-min").Should().HaveCount(1);
    }

    [Fact]
    public void AdminSecurityPolicyPage_ClickingLoginRateLimitTab_RendersLoginRateLimitEditor()
    {
        RegisterFakes();
        var cut = Render<AdminSecurityPolicyPage>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.FindAll(".context-tab")[4].Click();

        cut.FindAll("#login-permit").Should().HaveCount(1);
    }

    [Fact]
    public void AdminSecurityPolicyPage_ClickingAuditLogExportJobTab_RendersAuditLogExportJobPolicyEditor()
    {
        RegisterFakes();
        var cut = Render<AdminSecurityPolicyPage>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.FindAll(".context-tab")[5].Click();

        cut.FindAll("#exportjob-retention").Should().HaveCount(1);
    }
}