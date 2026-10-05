namespace MyGardenPlanner2026.Tests.UI.Components.Admin.SecurityPolicies;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using Xunit;

public class ReAuthenticationPolicyEditorTests : BunitContext
{
    private IReAuthenticationPolicyAdminService RegisterFakes(bool reAuthSucceeds, bool rateLimiterPermits = true)
    {
        var adminService = Substitute.For<IReAuthenticationPolicyAdminService>();
        adminService.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ReAuthenticationPolicyDto(15)));
        Services.AddSingleton(adminService);

        this.RegisterAdminStepUpFakes<ReAuthenticationPolicyEditorTests>(reAuthSucceeds, rateLimiterPermits);

        return adminService;
    }

    [Fact]
    public void ReAuthenticationPolicyEditor_RendersSeededValue()
    {
        RegisterFakes(reAuthSucceeds: true);

        var cut = Render<ReAuthenticationPolicyEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("#reauth-maxage").GetAttribute("value").Should().Be("15");
    }

    [Fact]
    public void ReAuthValid_ClickingGem_CallsUpdateAsyncWithEditedValue_WithoutOpeningModal()
    {
        var service = RegisterFakes(reAuthSucceeds: true);

        var cut = Render<ReAuthenticationPolicyEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));
        cut.Find("#reauth-maxage").Change("30");
        cut.Find("button.btn-primary").Click();

        _ = service.Received().UpdateAsync(
            Arg.Is<ReAuthenticationPolicyDto>(d => d.MaxAgeMinutes == 30),
            "user-1",
            Arg.Any<CancellationToken>());
        cut.ShouldNotShowStepUpModal();
    }

    [Fact]
    public void ReAuthValid_ClickingGem_InvokesOnStatusMessage()
    {
        RegisterFakes(reAuthSucceeds: true);
        string? receivedMessage = null;

        var cut = Render<ReAuthenticationPolicyEditor>(p => p
            .Add(x => x.OnStatusMessage, EventCallback.Factory.Create<string>(this, m => receivedMessage = m))
            .AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("button.btn-primary").Click();

        receivedMessage.Should().NotBeNull();
        receivedMessage.Should().Contain("opdateret");
    }

    [Fact]
    public void ReAuthExpired_ClickingGem_OpensStepUpModal_WithoutSaving()
    {
        var service = RegisterFakes(reAuthSucceeds: false);

        var cut = Render<ReAuthenticationPolicyEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();

        cut.ShouldShowStepUpModal();
        _ = service.DidNotReceive().UpdateAsync(Arg.Any<ReAuthenticationPolicyDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_CancellingStepUpModal_ClosesModal_WithoutSaving()
    {
        var service = RegisterFakes(reAuthSucceeds: false);

        var cut = Render<ReAuthenticationPolicyEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();
        cut.Find(".confirm-dialog button.btn-secondary").Click();

        cut.ShouldNotShowStepUpModal();
        _ = service.DidNotReceive().UpdateAsync(Arg.Any<ReAuthenticationPolicyDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void RateLimited_ClickingGem_DoesNotCallUpdateAsync_AndShowsErrorMessage()
    {
        var service = RegisterFakes(reAuthSucceeds: true, rateLimiterPermits: false);

        var cut = Render<ReAuthenticationPolicyEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();

        _ = service.DidNotReceive().UpdateAsync(Arg.Any<ReAuthenticationPolicyDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        cut.ShouldShowRateLimitMessage();
    }
}