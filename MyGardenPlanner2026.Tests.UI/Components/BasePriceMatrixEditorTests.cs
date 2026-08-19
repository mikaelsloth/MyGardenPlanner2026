namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using NSubstitute;
using Xunit;

public class BasePriceMatrixEditorTests : BunitContext
{
    private static SubscriptionTierAdminDto CreateDto(int id) => new(
        id, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, "Have Arkitekt · Administrator",
        AnnualPrice: 336m, MonthlyPrice: 28m, PerpetualPrice: 840m);

    private ISubscriptionTierAdminService RegisterFake()
    {
        var service = Substitute.For<ISubscriptionTierAdminService>();
        service.GetAllTiersAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SubscriptionTierAdminDto>>([CreateDto(1)]));

        Services.AddSingleton(service);
        return service;
    }

    [Fact]
    public void BasePriceMatrixEditor_RendersOneRowPerTier()
    {
        RegisterFake();

        var cut = Render<BasePriceMatrixEditor>();

        cut.FindAll("tbody tr").Should().HaveCount(1);
    }

    [Fact]
    public void BasePriceMatrixEditor_ClickingGem_CallsUpdateTierAsyncWithEditedValues()
    {
        var service = RegisterFake();

        var cut = Render<BasePriceMatrixEditor>();

        cut.Find("#annual-1").Change("350");
        cut.Find("button.btn-primary").Click();

        service.Received().UpdateTierAsync(
            Arg.Is<SubscriptionTierUpdateDto>(u => u.Id == 1 && u.AnnualPrice == 350m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void BasePriceMatrixEditor_ClickingGem_InvokesOnStatusMessage()
    {
        RegisterFake();
        string? receivedMessage = null;

        var cut = Render<BasePriceMatrixEditor>(p => p
            .Add(x => x.OnStatusMessage, EventCallback.Factory.Create<string>(this, m => receivedMessage = m)));

        cut.Find("button.btn-primary").Click();

        receivedMessage.Should().NotBeNull();
        receivedMessage.Should().Contain("opdateret");
    }
}