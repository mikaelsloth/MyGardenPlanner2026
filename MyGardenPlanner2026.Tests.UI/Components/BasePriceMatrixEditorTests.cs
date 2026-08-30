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
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class BasePriceMatrixEditorTests : BunitContext
{
    private static readonly Guid TierId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private static SubscriptionTierAdminDto CreateDto(Guid id) => new(
        id, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, "Have Arkitekt · Administrator",
        AnnualPrice: 336m, MonthlyPrice: 28m, PerpetualPrice: 840m);

    private static Task<AuthenticationState> CreateAuthState()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")], authenticationType: "Test");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    private ISubscriptionTierAdminService RegisterFakes(bool reAuthSucceeds)
    {
        var adminService = Substitute.For<ISubscriptionTierAdminService>();
        adminService.GetAllTiersAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SubscriptionTierAdminDto>>([CreateDto(TierId)]));
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

        return adminService;
    }

    [Fact]
    public void BasePriceMatrixEditor_RendersOneRowPerTier()
    {
        RegisterFakes(reAuthSucceeds: true);

        var cut = Render<BasePriceMatrixEditor>(p => p.AddCascadingValue(CreateAuthState()));

        cut.FindAll("tbody tr").Should().HaveCount(1);
    }

    [Fact]
    public void ReAuthValid_ClickingGem_CallsUpdateTierAsyncImmediately_WithoutOpeningModal()
    {
        var service = RegisterFakes(reAuthSucceeds: true);

        var cut = Render<BasePriceMatrixEditor>(p => p.AddCascadingValue(CreateAuthState()));
        cut.Find($"#annual-{TierId}").Change("350");
        cut.Find("button.btn-primary").Click();

        service.Received().UpdateTierAsync(
            Arg.Is<SubscriptionTierUpdateDto>(u => u.Id == TierId && u.AnnualPrice == 350m),
            Arg.Any<CancellationToken>());
        cut.FindAll(".confirm-dialog").Should().BeEmpty();
    }

    [Fact]
    public void ReAuthValid_ClickingGem_InvokesOnStatusMessage()
    {
        RegisterFakes(reAuthSucceeds: true);
        string? receivedMessage = null;

        var cut = Render<BasePriceMatrixEditor>(p => p
            .Add(x => x.OnStatusMessage, EventCallback.Factory.Create<string>(this, m => receivedMessage = m))
            .AddCascadingValue(CreateAuthState()));

        cut.Find("button.btn-primary").Click();

        receivedMessage.Should().NotBeNull();
        receivedMessage.Should().Contain("opdateret");
    }

    [Fact]
    public void ReAuthExpired_ClickingGem_OpensStepUpModal_WithoutSaving()
    {
        var service = RegisterFakes(reAuthSucceeds: false);

        var cut = Render<BasePriceMatrixEditor>(p => p.AddCascadingValue(CreateAuthState()));
        cut.Find("button.btn-primary").Click();

        cut.FindAll(".confirm-dialog").Should().HaveCount(1);
        service.DidNotReceive().UpdateTierAsync(Arg.Any<SubscriptionTierUpdateDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_CancellingStepUpModal_ClosesModal_WithoutSaving()
    {
        var service = RegisterFakes(reAuthSucceeds: false);

        var cut = Render<BasePriceMatrixEditor>(p => p.AddCascadingValue(CreateAuthState()));
        cut.Find("button.btn-primary").Click();

        cut.Find(".confirm-dialog button.btn-secondary").Click();

        cut.FindAll(".confirm-dialog").Should().BeEmpty();
        service.DidNotReceive().UpdateTierAsync(Arg.Any<SubscriptionTierUpdateDto>(), Arg.Any<CancellationToken>());
    }
}