namespace MyGardenPlanner2026.Tests.UI.Components.Admin.Subscriptions;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using Xunit;

public class AddOnEditorTests : BunitContext
{
    private static readonly Guid AddOn1Id = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static readonly SubscriptionAddOnDto AddOn1 = new(
        AddOn1Id, AddOnType.BedforslagNiveau2, "Bedforslag (Niveau 2)", "Pakke med 2 bedforslag", 180m, 15m, 450m);

    private ISubscriptionAddOnAdminService RegisterFake(bool reAuthSucceeds = true, bool rateLimiterPermits = true)
    {
        var service = Substitute.For<ISubscriptionAddOnAdminService>();
        service.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SubscriptionAddOnDto>>([AddOn1]));
        Services.AddSingleton(service);

        this.RegisterAdminStepUpFakes<AddOnEditorTests>(reAuthSucceeds, rateLimiterPermits);

        return service;
    }

    [Fact]
    public void AddOnEditor_RendersExistingAddOns()
    {
        RegisterFake();

        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.FindAll("tbody tr").Should().HaveCount(1);
        cut.Markup.Should().Contain("Bedforslag (Niveau 2)");
    }

    [Fact]
    public void ReAuthValid_ClickingGem_CallsSaveAsyncWithSameTypeAndEditedName()
    {
        var service = RegisterFake(reAuthSucceeds: true);
        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find($"#name-{AddOn1Id}").Change("Bedforslag (Niveau 2) - opdateret");
        cut.Find("button.btn-primary.btn-sm").Click();

        _ = service.Received().SaveAsync(
            Arg.Is<SubscriptionAddOnUpsertDto>(d =>
                d.Id == AddOn1Id && d.Type == AddOnType.BedforslagNiveau2 && d.Name == "Bedforslag (Niveau 2) - opdateret"),
            Arg.Any<CancellationToken>());
        cut.ShouldNotShowStepUpModal();
    }

    [Fact]
    public void ReAuthValid_AddingNewAddOn_CallsSaveAsyncWithNullId()
    {
        var service = RegisterFake(reAuthSucceeds: true);
        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("#new-name").Change("Artefaktpakke C");
        cut.Find("button.btn-primary:not(.btn-sm)").Click();

        _ = service.Received().SaveAsync(
            Arg.Is<SubscriptionAddOnUpsertDto>(d => d.Id == null && d.Name == "Artefaktpakke C"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_ClickingGem_OpensStepUpModal_WithoutSaving()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("button.btn-primary.btn-sm").Click();

        cut.ShouldShowStepUpModal();
        _ = service.DidNotReceive().SaveAsync(Arg.Any<SubscriptionAddOnUpsertDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_AddingNewAddOn_OpensStepUpModal_WithoutSaving()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("#new-name").Change("Artefaktpakke C");
        cut.Find("button.btn-primary:not(.btn-sm)").Click();

        cut.ShouldShowStepUpModal();
        _ = service.DidNotReceive().SaveAsync(Arg.Any<SubscriptionAddOnUpsertDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_ClickingSlet_OpensStepUpModal_WithoutDeleting()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("button.btn-danger.btn-sm").Click();

        cut.ShouldShowStepUpModal();
        _ = service.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_ConfirmingReset_OpensStepUpModal_WithoutResetting()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find(".danger-zone button.btn-danger").Click();
        cut.Find(".inline-confirm button.btn-danger").Click();

        cut.ShouldShowStepUpModal();
        _ = service.DidNotReceive().ResetToDefaultAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_CancellingStepUpModal_ClosesModal_WithoutSaving()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

        cut.Find("button.btn-danger.btn-sm").Click();
        cut.Find(".confirm-dialog button.btn-secondary").Click();

        cut.ShouldNotShowStepUpModal();
        _ = service.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void RateLimited_ClickingGem_DoesNotCallSaveAsync_AndShowsErrorMessage()
    {
        var service = RegisterFake(reAuthSucceeds: true, rateLimiterPermits: false);

        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();

        _ = service.DidNotReceive().SaveAsync(Arg.Any<SubscriptionAddOnUpsertDto>(), Arg.Any<CancellationToken>());
        cut.ShouldShowRateLimitMessage();
    }
}