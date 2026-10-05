namespace MyGardenPlanner2026.Tests.UI.Components.Admin.Subscriptions;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using Xunit;

public class VolumeDiscountEditorTests : BunitContext
{
    private static readonly Guid Tier1Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly GardenVolumeDiscountTierDto Tier1 = new(Tier1Id, 1, 1, 1.00m);

    private IGardenVolumeDiscountAdminService RegisterFake(bool reAuthSucceeds = true, bool rateLimiterPermits = true)
    {
        var service = Substitute.For<IGardenVolumeDiscountAdminService>();
        service.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GardenVolumeDiscountTierDto>>([Tier1]));
        Services.AddSingleton(service);

        this.RegisterAdminStepUpFakes<VolumeDiscountEditorTests>(reAuthSucceeds, rateLimiterPermits);

        return service;
    }

    [Fact]
    public void VolumeDiscountEditor_RendersExistingTiersAndAddForm()
    {
        RegisterFake();

        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.FindAll("tbody tr").Should().HaveCount(1);
        cut.Find("#new-min").Should().NotBeNull();
    }

    [Fact]
    public void ReAuthValid_AddingNewTier_CallsSaveAsyncWithNullId()
    {
        var service = RegisterFake(reAuthSucceeds: true);
        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("#new-min").Change("11");
        cut.Find("#new-mult").Change("0.70");
        cut.Find("#add-tier").Click();

        _ = service.Received().SaveAsync(
            Arg.Is<GardenVolumeDiscountTierUpsertDto>(d => d.Id == null && d.MinGardens == 11 && d.PriceMultiplier == 0.70m),
            Arg.Any<CancellationToken>());
        cut.ShouldNotShowStepUpModal();
    }

    [Fact]
    public void ReAuthValid_ClickingSlet_CallsDeleteAsync()
    {
        var service = RegisterFake(reAuthSucceeds: true);
        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("button.btn-danger.btn-sm").Click();

        _ = service.Received().DeleteAsync(Tier1Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthValid_ConfirmingReset_CallsResetToDefaultAsync()
    {
        var service = RegisterFake(reAuthSucceeds: true);
        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find(".danger-zone button.btn-danger").Click();
        cut.Find(".inline-confirm button.btn-danger").Click();

        _ = service.Received().ResetToDefaultAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_AddingNewTier_OpensStepUpModal_WithoutSaving()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("#new-min").Change("11");
        cut.Find("#new-mult").Change("0.70");
        cut.Find("#add-tier").Click();

        cut.ShouldShowStepUpModal();
        _ = service.DidNotReceive().SaveAsync(Arg.Any<GardenVolumeDiscountTierUpsertDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_ClickingSlet_OpensStepUpModal_WithoutDeleting()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("button.btn-danger.btn-sm").Click();

        cut.ShouldShowStepUpModal();
        _ = service.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_ConfirmingReset_OpensStepUpModal_WithoutResetting()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find(".danger-zone button.btn-danger").Click();
        cut.Find(".inline-confirm button.btn-danger").Click();

        cut.ShouldShowStepUpModal();
        _ = service.DidNotReceive().ResetToDefaultAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_CancellingStepUpModal_ClosesModal_WithoutSaving()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("#add-tier").Click();
        cut.Find(".confirm-dialog button.btn-secondary").Click();

        cut.ShouldNotShowStepUpModal();
        _ = service.DidNotReceive().SaveAsync(Arg.Any<GardenVolumeDiscountTierUpsertDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void RateLimited_ClickingGem_DoesNotCallSaveAsync_AndShowsErrorMessage()
    {
        var service = RegisterFake(reAuthSucceeds: true, rateLimiterPermits: false);

        var cut = Render<VolumeDiscountEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();

        _ = service.DidNotReceive().SaveAsync(Arg.Any<GardenVolumeDiscountTierUpsertDto>(), Arg.Any<CancellationToken>());
        cut.ShouldShowRateLimitMessage();
    }
}