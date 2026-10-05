namespace MyGardenPlanner2026.Tests.UI.Components.Pages.Admin;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Pages.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using Xunit;

public class AdminJitRequestsPageTests : BunitContext
{
    private void RegisterFakes()
    {
        var jitService = Substitute.For<IJitElevationService>();
        jitService.GetRequestsForUserAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RoleElevationRequestDto>>([]));
        jitService.GetPendingRequestsForApprovalAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RoleElevationRequestDto>>([]));
        Services.AddSingleton(jitService);

        this.RegisterAdminStepUpFakes<AdminJitRequestsPage>();
    }

    [Fact]
    public void AdminJitRequestsPage_RendersTwoTabs()
    {
        RegisterFakes();

        var cut = Render<AdminJitRequestsPage>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.FindAll(".context-tab").Should().HaveCount(2);
    }

    [Fact]
    public void AdminJitRequestsPage_DefaultTab_RendersRequestForm()
    {
        RegisterFakes();

        var cut = Render<AdminJitRequestsPage>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.FindAll("#jit-request-role").Should().HaveCount(1);
    }

    [Fact]
    public void AdminJitRequestsPage_ClickingApproveTab_RendersApprovalQueue()
    {
        RegisterFakes();
        var cut = Render<AdminJitRequestsPage>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.FindAll(".context-tab")[1].Click();

        cut.Markup.Should().Contain("Anmodninger til godkendelse");
    }
}