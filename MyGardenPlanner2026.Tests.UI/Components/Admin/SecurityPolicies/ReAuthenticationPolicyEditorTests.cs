namespace MyGardenPlanner2026.Tests.UI.Components.Admin.SecurityPolicies;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using Xunit;

public class ReAuthenticationPolicyEditorTests : BunitContext
{
    private IReAuthenticationPolicyAdminService RegisterFakes(bool reAuthSucceeds, bool rateLimitAllowed = true)
    {
        var adminService = Substitute.For<IReAuthenticationPolicyAdminService>();
        adminService.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ReAuthenticationPolicyDto(15)));
        Services.AddSingleton(adminService);

        this.AddAdminSecurityServices(reAuthSucceeds, rateLimitAllowed);
        Services.AddSingleton(Substitute.For<ILogger<ReAuthenticationPolicyEditor>>());

        return adminService;
    }

    [Fact]
    public void ReAuthenticationPolicyEditor_RendersSeededValues()
    {
        RegisterFakes(reAuthSucceeds: true);
        var cut = Render<ReAuthenticationPolicyEditor>(p => p.AddCascadingValue(TestAuthHelper.CreateAuthStateAsync()));

        cut.Find("#reauth-maxage").GetAttribute("value").Should().Be("15");
    }

    [Fact]
    public void ReAuthValid_ClickingGem_CallsUpdateAsyncWithEditedValues_WithoutOpeningModal()
    {
        var service = RegisterFakes(reAuthSucceeds: true);
        var cut = Render<ReAuthenticationPolicyEditor>(p => p.AddCascadingValue(TestAuthHelper.CreateAuthStateAsync()));

        cut.Find("#reauth-maxage").Change("30");
        cut.Find("button.btn-primary").Click();

        _ = service.Received().UpdateAsync(
            Arg.Is<ReAuthenticationPolicyDto>(d => d.MaxAgeMinutes == 30),
            "user-1",
            Arg.Any<CancellationToken>());
        cut.FindAll(".confirm-dialog").Should().BeEmpty();
    }

    [Fact]
    public void ReAuthValid_ClickingGem_InvokesOnStatusMessage()
    {
        RegisterFakes(reAuthSucceeds: true);
        string? receivedMessage = null;
        var cut = Render<ReAuthenticationPolicyEditor>(p => p
            .Add(x => x.OnStatusMessage, EventCallback.Factory.Create<string>(this, m => receivedMessage = m))
            .AddCascadingValue(TestAuthHelper.CreateAuthStateAsync()));

        cut.Find("button.btn-primary").Click();

        receivedMessage.Should().NotBeNull();
        receivedMessage.Should().Contain("opdateret");
    }

    [Fact]
    public void ReAuthExpired_ClickingGem_OpensStepUpModal_WithoutSaving()
    {
        var service = RegisterFakes(reAuthSucceeds: false);
        var cut = Render<ReAuthenticationPolicyEditor>(p => p.AddCascadingValue(TestAuthHelper.CreateAuthStateAsync()));

        cut.Find("button.btn-primary").Click();

        cut.FindAll(".confirm-dialog").Should().HaveCount(1);
        _ = service.DidNotReceive().UpdateAsync(Arg.Any<ReAuthenticationPolicyDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_CancellingStepUpModal_ClosesModal_WithoutSaving()
    {
        var service = RegisterFakes(reAuthSucceeds: false);
        var cut = Render<ReAuthenticationPolicyEditor>(p => p.AddCascadingValue(TestAuthHelper.CreateAuthStateAsync()));

        cut.Find("button.btn-primary").Click();
        cut.Find(".confirm-dialog button.btn-secondary").Click();

        cut.FindAll(".confirm-dialog").Should().BeEmpty();
        _ = service.DidNotReceive().UpdateAsync(Arg.Any<ReAuthenticationPolicyDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void RateLimited_ClickingGem_DoesNotCallUpdateAsync_AndShowsErrorMessage()
    {
        var service = RegisterFakes(reAuthSucceeds: true, rateLimitAllowed: false);
        var cut = Render<ReAuthenticationPolicyEditor>(p => p.AddCascadingValue(TestAuthHelper.CreateAuthStateAsync()));

        cut.Find("button.btn-primary").Click();

        _ = service.DidNotReceive().UpdateAsync(Arg.Any<ReAuthenticationPolicyDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        cut.Markup.Should().Contain("For mange handlinger");
    }
}