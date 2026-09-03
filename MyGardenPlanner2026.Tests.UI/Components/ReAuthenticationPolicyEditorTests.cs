namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class ReAuthenticationPolicyEditorTests : BunitContext
{
    private static Task<AuthenticationState> CreateAuthStateAsync()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")], authenticationType: "Test");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    private IReAuthenticationPolicyAdminService RegisterFakes(bool reAuthSucceeds)
    {
        var adminService = Substitute.For<IReAuthenticationPolicyAdminService>();
        adminService.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ReAuthenticationPolicyDto(15)));
        Services.AddSingleton(adminService);

        var authorizationService = Substitute.For<IAuthorizationService>();
        authorizationService.AuthorizeAsync(
                Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Is(AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy))
            .Returns(Task.FromResult(reAuthSucceeds ? AuthorizationResult.Success() : AuthorizationResult.Failed()));
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

        return adminService;
    }

    [Fact]
    public void ReAuthenticationPolicyEditor_RendersSeededValue()
    {
        RegisterFakes(reAuthSucceeds: true);

        var cut = Render<ReAuthenticationPolicyEditor>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.Find("#reauth-maxage").GetAttribute("value").Should().Be("15");
    }

    [Fact]
    public void ReAuthValid_ClickingGem_CallsUpdateAsyncWithEditedValue_WithoutOpeningModal()
    {
        var service = RegisterFakes(reAuthSucceeds: true);

        var cut = Render<ReAuthenticationPolicyEditor>(p => p.AddCascadingValue(CreateAuthStateAsync()));
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
            .AddCascadingValue(CreateAuthStateAsync()));

        cut.Find("button.btn-primary").Click();

        receivedMessage.Should().NotBeNull();
        receivedMessage.Should().Contain("opdateret");
    }

    [Fact]
    public void ReAuthExpired_ClickingGem_OpensStepUpModal_WithoutSaving()
    {
        var service = RegisterFakes(reAuthSucceeds: false);

        var cut = Render<ReAuthenticationPolicyEditor>(p => p.AddCascadingValue(CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();

        cut.FindAll(".confirm-dialog").Should().HaveCount(1);
        _ = service.DidNotReceive().UpdateAsync(Arg.Any<ReAuthenticationPolicyDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_CancellingStepUpModal_ClosesModal_WithoutSaving()
    {
        var service = RegisterFakes(reAuthSucceeds: false);

        var cut = Render<ReAuthenticationPolicyEditor>(p => p.AddCascadingValue(CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();
        cut.Find(".confirm-dialog button.btn-secondary").Click();

        cut.FindAll(".confirm-dialog").Should().BeEmpty();
        _ = service.DidNotReceive().UpdateAsync(Arg.Any<ReAuthenticationPolicyDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void RateLimited_ClickingGem_DoesNotCallUpdateAsync_AndShowsErrorMessage()
    {
        var service = RegisterFakes(reAuthSucceeds: true);
        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        rateLimiter.TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        Services.AddSingleton(rateLimiter);

        var cut = Render<ReAuthenticationPolicyEditor>(p => p.AddCascadingValue(CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();

        _ = service.DidNotReceive().UpdateAsync(Arg.Any<ReAuthenticationPolicyDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        cut.Markup.Should().Contain("For mange handlinger");
    }
}