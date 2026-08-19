namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using NSubstitute;
using Xunit;

public class VolumeDiscountEditorTests : BunitContext
{
    private static readonly GardenVolumeDiscountTierDto Tier1 = new(1, 1, 1, 1.00m);

    private IGardenVolumeDiscountAdminService RegisterFake()
    {
        var service = Substitute.For<IGardenVolumeDiscountAdminService>();
        service.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GardenVolumeDiscountTierDto>>([Tier1]));

        Services.AddSingleton(service);
        return service;
    }

    [Fact]
    public void VolumeDiscountEditor_RendersExistingTiersAndAddForm()
    {
        RegisterFake();

        var cut = Render<VolumeDiscountEditor>();

        cut.FindAll("tbody tr").Should().HaveCount(1);
        cut.Find("#new-min").Should().NotBeNull();
    }

    [Fact]
    public void VolumeDiscountEditor_AddingNewTier_CallsSaveAsyncWithNullId()
    {
        var service = RegisterFake();
        var cut = Render<VolumeDiscountEditor>();

        cut.Find("#new-min").Change("11");
        cut.Find("#new-mult").Change("0.70");
        cut.Find("#add-tier").Click();

        service.Received().SaveAsync(
            Arg.Is<GardenVolumeDiscountTierUpsertDto>(d => d.Id == null && d.MinGardens == 11 && d.PriceMultiplier == 0.70m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void VolumeDiscountEditor_ClickingSlet_CallsDeleteAsync()
    {
        var service = RegisterFake();
        var cut = Render<VolumeDiscountEditor>();

        cut.Find("button.btn-danger.btn-sm").Click();

        service.Received().DeleteAsync(1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void VolumeDiscountEditor_ConfirmingReset_CallsResetToDefaultAsync()
    {
        var service = RegisterFake();
        var cut = Render<VolumeDiscountEditor>();

        cut.Find(".danger-zone button.btn-danger").Click();
        cut.Find(".inline-confirm button.btn-danger").Click();

        service.Received().ResetToDefaultAsync(Arg.Any<CancellationToken>());
    }
}