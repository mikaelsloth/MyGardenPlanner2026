namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using NSubstitute;
using Xunit;

public class AddOnEditorTests : BunitContext
{
    private static readonly Guid AddOn1Id = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static readonly SubscriptionAddOnDto AddOn1 = new(
        AddOn1Id, AddOnType.BedforslagNiveau2, "Bedforslag (Niveau 2)", "Pakke med 2 bedforslag", 180m, 15m, 450m);

    private ISubscriptionAddOnAdminService RegisterFake()
    {
        var service = Substitute.For<ISubscriptionAddOnAdminService>();
        service.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SubscriptionAddOnDto>>([AddOn1]));

        Services.AddSingleton(service);
        return service;
    }

    [Fact]
    public void AddOnEditor_RendersExistingAddOns()
    {
        RegisterFake();

        var cut = Render<AddOnEditor>();

        cut.FindAll("tbody tr").Should().HaveCount(1);
        cut.Markup.Should().Contain("Bedforslag (Niveau 2)");
    }

    [Fact]
    public void AddOnEditor_ClickingGem_CallsSaveAsyncWithSameTypeAndEditedName()
    {
        var service = RegisterFake();
        var cut = Render<AddOnEditor>();

        cut.Find($"#name-{AddOn1Id}").Change("Bedforslag (Niveau 2) - opdateret");
        cut.Find("button.btn-primary.btn-sm").Click();

        service.Received().SaveAsync(
            Arg.Is<SubscriptionAddOnUpsertDto>(d =>
                d.Id == AddOn1Id && d.Type == AddOnType.BedforslagNiveau2 && d.Name == "Bedforslag (Niveau 2) - opdateret"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void AddOnEditor_AddingNewAddOn_CallsSaveAsyncWithNullId()
    {
        var service = RegisterFake();
        var cut = Render<AddOnEditor>();

        cut.Find("#new-name").Change("Artefaktpakke C");
        cut.Find("button.btn-primary:not(.btn-sm)").Click();

        service.Received().SaveAsync(
            Arg.Is<SubscriptionAddOnUpsertDto>(d => d.Id == null && d.Name == "Artefaktpakke C"),
            Arg.Any<CancellationToken>());
    }
}