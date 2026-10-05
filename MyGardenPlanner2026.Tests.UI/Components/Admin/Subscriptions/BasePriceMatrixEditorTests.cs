namespace MyGardenPlanner2026.Tests.UI.Components.Admin.Subscriptions;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using Xunit;

public class BasePriceMatrixEditorTests : BunitContext
{
    private static readonly Guid TierId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private static SubscriptionTierAdminDto CreateDto(Guid id) => new(
        id, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, "Have Arkitekt · Administrator",
        AnnualPrice: 336m, MonthlyPrice: 28m, PerpetualPrice: 840m);

    private ISubscriptionTierAdminService RegisterFakes(bool reAuthSucceeds, bool rateLimiterPermits = true)
    {
        var adminService = Substitute.For<ISubscriptionTierAdminService>();
        adminService.GetAllTiersAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SubscriptionTierAdminDto>>([CreateDto(TierId)]));
        Services.AddSingleton(adminService);

        this.RegisterAdminStepUpFakes<BasePriceMatrixEditorTests>(reAuthSucceeds, rateLimiterPermits);

        return adminService;
    }

    [Fact]
    public void BasePriceMatrixEditor_RendersOneRowPerTier()
    {
        RegisterFakes(reAuthSucceeds: true);

        var cut = Render<BasePriceMatrixEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.FindAll("tbody tr").Should().HaveCount(1);
    }

    [Fact]
    public void ReAuthValid_ClickingGem_CallsUpdateTierAsyncImmediately_WithoutOpeningModal()
    {
        var service = RegisterFakes(reAuthSucceeds: true);

        var cut = Render<BasePriceMatrixEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));
        cut.Find($"#annual-{TierId}").Change("350");
        cut.Find("button.btn-primary").Click();

        _ = service.Received().UpdateTierAsync(
            Arg.Is<SubscriptionTierUpdateDto>(u => u.Id == TierId && u.AnnualPrice == 350m),
            Arg.Any<CancellationToken>());
        cut.ShouldNotShowStepUpModal();
    }

    [Fact]
    public void ReAuthValid_ClickingGem_InvokesOnStatusMessage()
    {
        RegisterFakes(reAuthSucceeds: true);
        string? receivedMessage = null;

        var cut = Render<BasePriceMatrixEditor>(p => p
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

        var cut = Render<BasePriceMatrixEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();

        cut.ShouldShowStepUpModal();
        _ = service.DidNotReceive().UpdateTierAsync(Arg.Any<SubscriptionTierUpdateDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_CancellingStepUpModal_ClosesModal_WithoutSaving()
    {
        var service = RegisterFakes(reAuthSucceeds: false);

        var cut = Render<BasePriceMatrixEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();

        cut.Find(".confirm-dialog button.btn-secondary").Click();

        cut.ShouldNotShowStepUpModal();
        _ = service.DidNotReceive().UpdateTierAsync(Arg.Any<SubscriptionTierUpdateDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void RateLimited_ClickingGem_DoesNotCallUpdateTierAsync_AndShowsErrorMessage()
    {
        var service = RegisterFakes(reAuthSucceeds: true, rateLimiterPermits: false);

        var cut = Render<BasePriceMatrixEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();

        _ = service.DidNotReceive().UpdateTierAsync(Arg.Any<SubscriptionTierUpdateDto>(), Arg.Any<CancellationToken>());
        cut.ShouldShowRateLimitMessage();
    }
}