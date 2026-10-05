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

public class LoginRateLimitEditorTests : BunitContext
{
    private ILoginRateLimitPolicyAdminService RegisterFakes(bool reAuthSucceeds, bool rateLimiterPermits = true)
    {
        var adminService = Substitute.For<ILoginRateLimitPolicyAdminService>();
        adminService.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new LoginRateLimitPolicyDto(5, 60)));
        Services.AddSingleton(adminService);

        this.RegisterAdminStepUpFakes<LoginRateLimitEditorTests>(reAuthSucceeds, rateLimiterPermits);

        return adminService;
    }

    [Fact]
    public void LoginRateLimitEditor_RendersSeededValues()
    {
        RegisterFakes(reAuthSucceeds: true);

        var cut = Render<LoginRateLimitEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("#login-permit").GetAttribute("value").Should().Be("5");
        cut.Find("#login-window").GetAttribute("value").Should().Be("60");
    }

    [Fact]
    public void ReAuthValid_ClickingGem_CallsUpdateAsyncWithEditedValues_WithoutOpeningModal()
    {
        var service = RegisterFakes(reAuthSucceeds: true);

        var cut = Render<LoginRateLimitEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));
        cut.Find("#login-permit").Change("10");
        cut.Find("button.btn-primary").Click();

        _ = service.Received().UpdateAsync(
            Arg.Is<LoginRateLimitPolicyDto>(d => d.PermitLimit == 10 && d.WindowSeconds == 60),
            "user-1",
            Arg.Any<CancellationToken>());
        cut.ShouldNotShowStepUpModal();
    }

    [Fact]
    public void ReAuthValid_ClickingGem_InvokesOnStatusMessage()
    {
        RegisterFakes(reAuthSucceeds: true);
        string? receivedMessage = null;

        var cut = Render<LoginRateLimitEditor>(p => p
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

        var cut = Render<LoginRateLimitEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();

        cut.ShouldShowStepUpModal();
        _ = service.DidNotReceive().UpdateAsync(Arg.Any<LoginRateLimitPolicyDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_CancellingStepUpModal_ClosesModal_WithoutSaving()
    {
        var service = RegisterFakes(reAuthSucceeds: false);

        var cut = Render<LoginRateLimitEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();
        cut.Find(".confirm-dialog button.btn-secondary").Click();

        cut.ShouldNotShowStepUpModal();
        _ = service.DidNotReceive().UpdateAsync(Arg.Any<LoginRateLimitPolicyDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void RateLimited_ClickingGem_DoesNotCallUpdateAsync_AndShowsErrorMessage()
    {
        var service = RegisterFakes(reAuthSucceeds: true, rateLimiterPermits: false);

        var cut = Render<LoginRateLimitEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();

        _ = service.DidNotReceive().UpdateAsync(Arg.Any<LoginRateLimitPolicyDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        cut.ShouldShowRateLimitMessage();
    }
}