namespace MyGardenPlanner2026.Tests.UI.Internal_Tests;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Interaction;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities;
using Xunit;

public sealed class AdminTestSupportTests : BunitContext
{
    [Fact]
    public async Task RegisterAdminStepUpFakes_ReAuthSucceeds_RecentAuthenticationPolicySucceeds()
    {
        var fakes = this.RegisterAdminStepUpFakes<AdminTestSupportTests>(reAuthSucceeds: true);

        var result = await fakes.AuthorizationService.AuthorizeAsync(
            TestPrincipals.Create(), null, AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAdminStepUpFakes_ReAuthFails_RecentAuthenticationPolicyFails()
    {
        var fakes = this.RegisterAdminStepUpFakes<AdminTestSupportTests>(reAuthSucceeds: false);

        var result = await fakes.AuthorizationService.AuthorizeAsync(
            TestPrincipals.Create(), null, AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy);

        result.Succeeded.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RegisterAdminStepUpFakes_RateLimiterReturnsConfiguredPermit(bool permits)
    {
        var fakes = this.RegisterAdminStepUpFakes<AdminTestSupportTests>(rateLimiterPermits: permits);

        var result = await fakes.RateLimiter.TryAcquireAsync("user-1", Xunit.TestContext.Current.CancellationToken);

        result.Should().Be(permits);
    }

    [Fact]
    public void RegisterAdminStepUpFakes_RegistersFakesInServices()
    {
        var fakes = this.RegisterAdminStepUpFakes<AdminTestSupportTests>();

        Services.GetRequiredService<IAdminActionRateLimiter>().Should().BeSameAs(fakes.RateLimiter);
        Services.GetRequiredService<UserManager<ApplicationUser>>().Should().NotBeNull();
        Services.GetRequiredService<IReAuthenticationService>().Should().NotBeNull();
        Services.GetRequiredService<IReAuthFailureTracker>().Should().NotBeNull();
    }

    [Fact]
    public void ShouldShowStepUpModal_OpenDialog_Passes()
    {
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked"));

        cut.ShouldShowStepUpModal();
    }

    [Fact]
    public void ShouldShowStepUpModal_ClosedDialog_Fails()
    {
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, false)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked"));

        var act = () => cut.ShouldShowStepUpModal();

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldNotShowStepUpModal_ClosedDialog_Passes()
    {
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, false)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, "Besked"));

        cut.ShouldNotShowStepUpModal();
    }

    [Fact]
    public void ShouldShowRateLimitMessage_MessagePresent_Passes()
    {
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsOpen, true)
            .Add(d => d.Title, "Titel")
            .Add(d => d.Message, StepUpAssertions.RateLimitMessage));

        cut.ShouldShowRateLimitMessage();
    }
}