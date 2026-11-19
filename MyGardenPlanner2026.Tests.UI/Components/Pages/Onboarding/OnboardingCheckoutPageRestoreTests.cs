namespace MyGardenPlanner2026.Tests.UI.Components.Pages.Onboarding;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Pages;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using NSubstitute;
using Xunit;

/// <summary>Dækker gendannelse i checkout (R10.1e). Egen fil for ikke at kollidere med de øvrige checkout-tests.</summary>
public sealed class OnboardingCheckoutPageRestoreTests : OnboardingPricingTestContext
{
    private const string Email = "bruger@example.dk";
    private const string BedDesignerEditorUri = "/onboarding/checkout?level=BedDesigner&category=Editor";

    private static readonly Guid PendingId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static RestorableGardenDto Restorable(
        GardenAccessLevel layer = GardenAccessLevel.BedDesigner,
        AccessCategory category = AccessCategory.Editor,
        string name = "Min gemte have") =>
        new(PendingId, Guid.NewGuid(), name,
            new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.Zero),
            layer, category);

    private void ArrangeAuthenticated(params RestorableGardenDto[] restorable)
    {
        this.AuthorizeAs("user-1", email: Email);
        queryService.GetOwnedGardenCountsAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(new OwnedGardenCountsDto(0, 0));
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(SubscriptionTestData.PricingResult());
        restoreService.GetRestorableGardensAsync(Email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RestorableGardenDto>>(restorable));
    }

    private async Task<IRenderedComponent<OnboardingCheckoutPage>> ReachPaymentStepAsync()
    {
        var cut = this.RenderAt<OnboardingCheckoutPage>(BedDesignerEditorUri);
        await cut.Find("#garden-name").ChangeAsync("Min have");
        await cut.Find(".btn-accent").ClickAsync();
        return cut;
    }

    private static PaidGardenProvisionResultDto Result(Guid gardenId) => new(gardenId, Guid.NewGuid(), Guid.NewGuid());

    // ---------- Visning ----------

    [Fact]
    public async Task NoRestorableGardens_PaymentStepShowsNoRestoreChoice()
    {
        ArrangeAuthenticated();

        var cut = await ReachPaymentStepAsync();

        cut.FindAll(".restore-garden-choice").Should().BeEmpty();
        cut.Markup.Should().Contain("Gennemfør betaling og opret have");
    }

    [Fact]
    public void RestorableGardens_ConfigurationStepShowsHintBanner()
    {
        ArrangeAuthenticated(Restorable());

        var cut = this.RenderAt<OnboardingCheckoutPage>(BedDesignerEditorUri);

        cut.Find(".status-banner").TextContent.Should().Contain("gemte haver");
    }

    [Fact]
    public async Task RestorableGardens_PaymentStepShowsChoice_WithNewGardenSelectedByDefault()
    {
        ArrangeAuthenticated(Restorable());

        var cut = await ReachPaymentStepAsync();

        cut.FindAll(".restore-garden-choice").Should().HaveCount(1);
        cut.Find("#restore-garden-none").HasAttribute("checked").Should().BeTrue();
        cut.Find($"#restore-garden-{PendingId}").HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void NoEmailClaim_DoesNotLookUpRestorableGardens()
    {
        this.AuthorizeAs("user-1");
        queryService.GetOwnedGardenCountsAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(new OwnedGardenCountsDto(0, 0));

        this.RenderAt<OnboardingCheckoutPage>(BedDesignerEditorUri);

        _ = restoreService.DidNotReceive().GetRestorableGardensAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void NotAuthenticated_DoesNotLookUpRestorableGardens()
    {
        AddAuthorization().SetNotAuthorized();

        this.RenderAt<OnboardingCheckoutPage>(BedDesignerEditorUri);

        _ = restoreService.DidNotReceive().GetRestorableGardensAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChoosingRestore_ShowsRestoredGardenNameAndRestoreButtonLabel()
    {
        ArrangeAuthenticated(Restorable(name: "Min gemte have"));

        var cut = await ReachPaymentStepAsync();
        await cut.Find($"#restore-garden-{PendingId}").ChangeAsync("on");

        cut.FindAll(".payment-mock-page .read-only-value")[0].TextContent.Should().Be("Min gemte have");
        cut.Find(".payment-mock-page .btn-primary").TextContent.Should().Contain("Gennemfør betaling og gendan have");
    }

    // ---------- Dækning ----------

    [Fact]
    public async Task GardenNotCoveredByChoice_IsDisabledWithUpgradeHint()
    {
        ArrangeAuthenticated(Restorable(GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator));

        var cut = await ReachPaymentStepAsync();

        cut.Find($"#restore-garden-{PendingId}").HasAttribute("disabled").Should().BeTrue();
        cut.Find(".restore-garden-choice-upgrade").TextContent.Should().Contain("Have Arkitekt / Administrator");
    }

    [Fact]
    public async Task ClickingChangeSubscription_ReturnsToConfigurationWithoutRestoreChoice()
    {
        ArrangeAuthenticated(Restorable(GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator));

        var cut = await ReachPaymentStepAsync();
        await cut.Find(".restore-garden-choice-hint button").ClickAsync();

        cut.FindAll("#calc-level").Should().HaveCount(1);
        cut.FindAll(".restore-garden-choice").Should().BeEmpty();
    }

    // ---------- Gennemfør betaling ----------

    [Fact]
    public async Task ChoosingRestore_ConfirmingPayment_CallsProvisionRestoredGardenAsync_AndNavigates()
    {
        ArrangeAuthenticated(Restorable());
        var gardenId = Guid.NewGuid();
        onboardingService.ProvisionRestoredGardenAsync(Arg.Any<PaidGardenRestoreRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(Result(gardenId));

        var cut = await ReachPaymentStepAsync();
        await cut.Find($"#restore-garden-{PendingId}").ChangeAsync("on");
        await cut.Find(".payment-mock-page .btn-primary").ClickAsync();

        await onboardingService.Received(1).ProvisionRestoredGardenAsync(
            Arg.Is<PaidGardenRestoreRequestDto>(r =>
                r.UserId == "user-1"
                && r.Email == Email
                && r.RestorePendingId == PendingId
                && r.Layer == GardenAccessLevel.BedDesigner
                && r.Category == AccessCategory.Editor
                && r.BillingCycle == BillingCycle.Annual),
            Arg.Any<CancellationToken>());
        await onboardingService.DidNotReceive().ProvisionPaidGardenAsync(
            Arg.Any<PaidGardenProvisionRequestDto>(), Arg.Any<CancellationToken>());
        this.ShouldHaveNavigatedTo($"/gardens/{gardenId}/access");
    }

    [Fact]
    public async Task NewGardenChosen_ConfirmingPayment_ProvisionsNewGardenAndDoesNotRestore()
    {
        ArrangeAuthenticated(Restorable());
        onboardingService.ProvisionPaidGardenAsync(Arg.Any<PaidGardenProvisionRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(Result(Guid.NewGuid()));

        var cut = await ReachPaymentStepAsync();
        await cut.Find(".payment-mock-page .btn-primary").ClickAsync();

        await onboardingService.Received(1).ProvisionPaidGardenAsync(
            Arg.Is<PaidGardenProvisionRequestDto>(r => r.UserId == "user-1" && r.GardenName == "Min have"),
            Arg.Any<CancellationToken>());
        await onboardingService.DidNotReceive().ProvisionRestoredGardenAsync(
            Arg.Any<PaidGardenRestoreRequestDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChoosingRestore_ResumedFromDraft_CallsProvisionRestoredGardenFromDraftAsync()
    {
        ArrangeAuthenticated(Restorable());
        var draftId = Guid.NewGuid();
        var draft = new CheckoutDraftDto(
            draftId, null, "Min gemte have", null, GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            BillingCycle.Annual, new Dictionary<Guid, int>(), DateTimeOffset.UtcNow.AddMinutes(10));
        onboardingService.GetCheckoutDraftAsync(draftId, Arg.Any<CancellationToken>()).Returns(draft);
        var gardenId = Guid.NewGuid();
        onboardingService.ProvisionRestoredGardenFromDraftAsync(draftId, "user-1", Email, PendingId, Arg.Any<CancellationToken>())
            .Returns(Result(gardenId));

        var cut = this.RenderAt<OnboardingCheckoutPage>($"/onboarding/checkout?draft={draftId}");
        await cut.Find($"#restore-garden-{PendingId}").ChangeAsync("on");
        await cut.Find(".payment-mock-page .btn-primary").ClickAsync();

        await onboardingService.Received(1).ProvisionRestoredGardenFromDraftAsync(
            draftId, "user-1", Email, PendingId, Arg.Any<CancellationToken>());
        await onboardingService.DidNotReceive().ProvisionPaidGardenFromDraftAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        this.ShouldHaveNavigatedTo($"/gardens/{gardenId}/access");
    }

    [Fact]
    public async Task ChangeSubscriptionFromDraft_PrefillsCalculatorFromDraft_AndDoesNotUseOldDraftForPayment()
    {
        ArrangeAuthenticated(Restorable(GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator));
        var draftId = Guid.NewGuid();
        var draft = new CheckoutDraftDto(
            draftId, null, "Min gemte have", null, GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            BillingCycle.Annual, new Dictionary<Guid, int>(), DateTimeOffset.UtcNow.AddMinutes(10));
        onboardingService.GetCheckoutDraftAsync(draftId, Arg.Any<CancellationToken>()).Returns(draft);
        onboardingService.ProvisionPaidGardenAsync(Arg.Any<PaidGardenProvisionRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(Result(Guid.NewGuid()));

        var cut = this.RenderAt<OnboardingCheckoutPage>($"/onboarding/checkout?draft={draftId}");
        await cut.Find(".restore-garden-choice-hint button").ClickAsync();
        await cut.Find(".btn-accent").ClickAsync();
        await cut.Find(".payment-mock-page .btn-primary").ClickAsync();

        // Én beregning ved kladde-indlæsning og én efter "Fortsæt", begge med kladdens niveau (kalkulatoren er forudfyldt).
        await calculatorService.Received(2).CalculateAsync(
            Arg.Is<PricingCalculationRequestDto>(r => r.Level == GardenAccessLevel.BedDesigner),
            Arg.Any<CancellationToken>());
        await onboardingService.Received(1).ProvisionPaidGardenAsync(
            Arg.Any<PaidGardenProvisionRequestDto>(), Arg.Any<CancellationToken>());
        await onboardingService.DidNotReceive().ProvisionPaidGardenFromDraftAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ServiceRejectsRestore_ShowsErrorAndReloadsRestorableGardens()
    {
        ArrangeAuthenticated(Restorable());
        restoreService.GetRestorableGardensAsync(Email, Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyList<RestorableGardenDto>>([Restorable()]),
                Task.FromResult<IReadOnlyList<RestorableGardenDto>>([]));
        onboardingService.ProvisionRestoredGardenAsync(Arg.Any<PaidGardenRestoreRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<PaidGardenProvisionResultDto>(
                new InvalidOperationException("Gendannelsesperioden er udløbet.")));

        var cut = await ReachPaymentStepAsync();
        await cut.Find($"#restore-garden-{PendingId}").ChangeAsync("on");
        await cut.Find(".payment-mock-page .btn-primary").ClickAsync();

        cut.Find(".payment-mock-page .status-danger p").TextContent.Should().Be("Gendannelsesperioden er udløbet.");
        cut.FindAll(".restore-garden-choice").Should().BeEmpty();
    }
}