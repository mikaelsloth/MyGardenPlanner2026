namespace MyGardenPlanner2026.Tests.UI.Components.Domain.Subscriptions;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Components.Domain.Subscriptions;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public sealed class RestoreGardenChoiceTests : BunitContext
{
    private static readonly Guid GardenId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private static RestorableGardenDto Garden(GardenAccessLevel layer, AccessCategory category) =>
        new(GardenId, Guid.NewGuid(), "Min gemte have",
            new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 12, 20, 12, 0, 0, TimeSpan.Zero),
            layer, category);

    private IRenderedComponent<RestoreGardenChoice> RenderChoice(
        RestorableGardenDto garden,
        GardenAccessLevel purchasedLayer = GardenAccessLevel.BedDesigner,
        AccessCategory purchasedCategory = AccessCategory.Editor,
        Guid? selected = null,
        Action<Guid?>? onChanged = null,
        Action? onChangeSubscription = null)
    {
        IReadOnlyList<RestorableGardenDto> gardens = [garden];

        return Render<RestoreGardenChoice>(p => p
            .Add(x => x.Gardens, gardens)
            .Add(x => x.SelectedLayer, purchasedLayer)
            .Add(x => x.SelectedCategory, purchasedCategory)
            .Add(x => x.SelectedPendingRestoreId, selected)
            .Add(x => x.SelectedPendingRestoreIdChanged, EventCallback.Factory.Create<Guid?>(this, onChanged ?? (_ => { })))
            .Add(x => x.OnChangeSubscription, EventCallback.Factory.Create(this, onChangeSubscription ?? (() => { }))));
    }

    [Fact]
    public void NothingSelected_NewGardenOptionIsChecked()
    {
        var cut = RenderChoice(Garden(GardenAccessLevel.BedDesigner, AccessCategory.Editor));

        cut.Find("#restore-garden-none").HasAttribute("checked").Should().BeTrue();
        cut.Find($"#restore-garden-{GardenId}").HasAttribute("checked").Should().BeFalse();
    }

    [Fact]
    public void GardenSelected_GardenOptionIsChecked()
    {
        var cut = RenderChoice(Garden(GardenAccessLevel.BedDesigner, AccessCategory.Editor), selected: GardenId);

        cut.Find($"#restore-garden-{GardenId}").HasAttribute("checked").Should().BeTrue();
        cut.Find("#restore-garden-none").HasAttribute("checked").Should().BeFalse();
    }

    [Fact]
    public void CoveredGarden_IsEnabledWithNameAndExpiry_AndNoUpgradeHint()
    {
        var cut = RenderChoice(Garden(GardenAccessLevel.BedDesigner, AccessCategory.Editor));

        cut.Find($"#restore-garden-{GardenId}").HasAttribute("disabled").Should().BeFalse();
        cut.Find($"label[for='restore-garden-{GardenId}']").TextContent.Should().Contain("Min gemte have");
        cut.Find(".restore-garden-choice-hint").TextContent.Should().Contain("20.12.2026 (UTC)");
        cut.FindAll(".restore-garden-choice-upgrade").Should().BeEmpty();
    }

    [Fact]
    public void GardenNotCoveredByPurchase_IsDisabledAndShowsRequiredLevel()
    {
        var cut = RenderChoice(Garden(GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator));

        cut.Find($"#restore-garden-{GardenId}").HasAttribute("disabled").Should().BeTrue();
        cut.Find(".restore-garden-choice-upgrade").TextContent.Should().Contain("Kræver mindst Have Arkitekt / Administrator");
    }

    [Fact]
    public async Task SelectingCoveredGarden_RaisesChangedWithPendingId()
    {
        Guid? raised = null;
        var cut = RenderChoice(Garden(GardenAccessLevel.BedDesigner, AccessCategory.Editor), onChanged: id => raised = id);

        await cut.Find($"#restore-garden-{GardenId}").ChangeAsync("on");

        raised.Should().Be(GardenId);
    }

    [Fact]
    public async Task SelectingNewGarden_RaisesChangedWithNull()
    {
        Guid? raised = GardenId;
        var cut = RenderChoice(
            Garden(GardenAccessLevel.BedDesigner, AccessCategory.Editor), selected: GardenId, onChanged: id => raised = id);

        await cut.Find("#restore-garden-none").ChangeAsync("on");

        raised.Should().BeNull();
    }

    [Fact]
    public async Task ChangeSubscriptionButton_RaisesCallback()
    {
        var raised = false;
        var cut = RenderChoice(
            Garden(GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator), onChangeSubscription: () => raised = true);

        await cut.Find(".restore-garden-choice-hint button").ClickAsync();

        raised.Should().BeTrue();
    }
}