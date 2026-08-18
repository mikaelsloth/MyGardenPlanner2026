namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Subscriptions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using Xunit;

public class GardenVolumeDiscountTableTests : BunitContext
{
    [Fact]
    public void GardenVolumeDiscountTable_RendersAllSevenTiersAndInfoBox()
    {
        Services.AddSingleton<IGardenVolumeDiscountCatalog>(new DefaultGardenVolumeDiscountCatalog());

        var cut = Render<GardenVolumeDiscountTable>();

        cut.FindAll("tbody tr").Should().HaveCount(7);
        cut.FindAll(".status-info").Should().HaveCount(1);
        cut.Markup.Should().Contain("25%");
    }
}