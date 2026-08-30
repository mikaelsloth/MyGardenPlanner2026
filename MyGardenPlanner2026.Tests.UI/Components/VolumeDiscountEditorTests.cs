namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class VolumeDiscountEditorTests : BunitContext
{
    private static readonly Guid Tier1Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly GardenVolumeDiscountTierDto Tier1 = new(Tier1Id, 1, 1, 1.00m);

    private static Task<AuthenticationState> CreateAuthState()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")], authenticationType: "Test");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    private IGardenVolumeDiscountAdminService RegisterFake(bool reAuthSucceeds = true)
    {
        var service = Substitute.For<IGardenVolumeDiscountAdminService>();
        service.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GardenVolumeDiscountTierDto>>([Tier1]));
        Services.AddSingleton(service);

        var authorizationService = Substitute.For<IAuthorizationService>();
        authorizationService.AuthorizeAsync(
                Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Is(AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy))
            .Returns(Task.FromResult(reAuthSucceeds ? AuthorizationResult.Success() : AuthorizationResult.Failed()));
        Services.AddSingleton(authorizationService);

        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(null));
        Services.AddSingleton(userManager);

        Services.AddSingleton(Substitute.For<IReAuthenticationService>());

        return service;
    }

    [Fact]
    public void VolumeDiscountEditor_RendersExistingTiersAndAddForm()
    {
        RegisterFake();

        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(CreateAuthState()));

        cut.FindAll("tbody tr").Should().HaveCount(1);
        cut.Find("#new-min").Should().NotBeNull();
    }

    [Fact]
    public void ReAuthValid_AddingNewTier_CallsSaveAsyncWithNullId()
    {
        var service = RegisterFake(reAuthSucceeds: true);
        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(CreateAuthState()));

        cut.Find("#new-min").Change("11");
        cut.Find("#new-mult").Change("0.70");
        cut.Find("#add-tier").Click();

        service.Received().SaveAsync(
            Arg.Is<GardenVolumeDiscountTierUpsertDto>(d => d.Id == null && d.MinGardens == 11 && d.PriceMultiplier == 0.70m),
            Arg.Any<CancellationToken>());
        cut.FindAll(".confirm-dialog").Should().BeEmpty();
    }

    [Fact]
    public void ReAuthValid_ClickingSlet_CallsDeleteAsync()
    {
        var service = RegisterFake(reAuthSucceeds: true);
        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(CreateAuthState()));

        cut.Find("button.btn-danger.btn-sm").Click();

        service.Received().DeleteAsync(Tier1Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthValid_ConfirmingReset_CallsResetToDefaultAsync()
    {
        var service = RegisterFake(reAuthSucceeds: true);
        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(CreateAuthState()));

        cut.Find(".danger-zone button.btn-danger").Click();
        cut.Find(".inline-confirm button.btn-danger").Click();

        service.Received().ResetToDefaultAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_AddingNewTier_OpensStepUpModal_WithoutSaving()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(CreateAuthState()));

        cut.Find("#new-min").Change("11");
        cut.Find("#new-mult").Change("0.70");
        cut.Find("#add-tier").Click();

        cut.FindAll(".confirm-dialog").Should().HaveCount(1);
        service.DidNotReceive().SaveAsync(Arg.Any<GardenVolumeDiscountTierUpsertDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_ClickingSlet_OpensStepUpModal_WithoutDeleting()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(CreateAuthState()));

        cut.Find("button.btn-danger.btn-sm").Click();

        cut.FindAll(".confirm-dialog").Should().HaveCount(1);
        service.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_ConfirmingReset_OpensStepUpModal_WithoutResetting()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(CreateAuthState()));

        cut.Find(".danger-zone button.btn-danger").Click();
        cut.Find(".inline-confirm button.btn-danger").Click();

        cut.FindAll(".confirm-dialog").Should().HaveCount(1);
        service.DidNotReceive().ResetToDefaultAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_CancellingStepUpModal_ClosesModal_WithoutSaving()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(CreateAuthState()));

        cut.Find("#add-tier").Click();
        cut.Find(".confirm-dialog button.btn-secondary").Click();

        cut.FindAll(".confirm-dialog").Should().BeEmpty();
        service.DidNotReceive().SaveAsync(Arg.Any<GardenVolumeDiscountTierUpsertDto>(), Arg.Any<CancellationToken>());
    }
}