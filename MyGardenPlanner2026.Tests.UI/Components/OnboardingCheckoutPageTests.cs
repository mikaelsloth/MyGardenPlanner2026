namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Pages;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public sealed class OnboardingCheckoutPageTests : BunitContext
{
    private readonly IOnboardingService onboardingService = Substitute.For<IOnboardingService>();
    private readonly IGardenAccessQueryService queryService = Substitute.For<IGardenAccessQueryService>();
    private readonly IPricingCalculatorService calculatorService = Substitute.For<IPricingCalculatorService>();
    private readonly ISubscriptionAddOnService addOnService = Substitute.For<ISubscriptionAddOnService>();

    public OnboardingCheckoutPageTests()
    {
        addOnService.GetAllAddOnsAsync(Arg.Any<CancellationToken>()).Returns([]);
        Services.AddSingleton(onboardingService);
        Services.AddSingleton(queryService);
        Services.AddSingleton(calculatorService);
        Services.AddSingleton(addOnService);
    }

    /// <summary>SetAuthorized(userId) alene sætter kun ClaimTypes.Name — CurrentUserIdResolver
    /// slår op på ClaimTypes.NameIdentifier, som skal sættes eksplicit.</summary>
    private void AuthorizeAs(string userId)
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized(userId);
        authContext.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    private static PricingCalculationResultDto CreateResult(decimal total = 100m) =>
        new(100m, 1m, 1.0m, 100m, [], 0m, total);

    private static CheckoutDraftDto CreateDraft(Guid id, string? userId = null) => new(
        id, userId, "Min gemte have", null, GardenAccessLevel.BedDesigner, AccessCategory.Editor,
        BillingCycle.Annual, new Dictionary<Guid, int>(), DateTimeOffset.UtcNow.AddMinutes(10));

    /// <summary>
    /// [SupplyParameterFromQuery]-parametre kan ikke sættes via ComponentParameter.Add i
    /// bUnit — de skal bindes via NavigationManager (bUnits fake), ved at navigere til
    /// URL'en med querystring FØR komponenten renderes.
    /// </summary>
    private IRenderedComponent<OnboardingCheckoutPage> RenderWithDraft(Guid draftId)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo($"/onboarding/checkout?draft={draftId}");
        return Render<OnboardingCheckoutPage>();
    }

    [Fact]
    public void PricingCalculator_HiddenUntilGardenNameEntered()
    {
        AddAuthorization().SetNotAuthorized();

        var cut = Render<OnboardingCheckoutPage>();

        cut.FindAll("#calc-level").Should().BeEmpty();

        cut.Find("#garden-name").Change("Min have");

        cut.FindAll("#calc-level").Should().HaveCount(1);
    }

    [Fact]
    public void NotAuthenticated_ClickingContinue_MovesToChooseAuthenticationStep()
    {
        AddAuthorization().SetNotAuthorized();
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        var cut = Render<OnboardingCheckoutPage>();
        cut.Find("#garden-name").Change("Min have");
        cut.Find(".btn-accent").Click();

        cut.Markup.Should().Contain("Opret konto eller log ind");
    }

    [Fact]
    public async Task NotAuthenticated_UsesFixedActiveGardensOfOneAndZeroArchived()
    {
        AddAuthorization().SetNotAuthorized();
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        var cut = Render<OnboardingCheckoutPage>();
        await cut.Find("#garden-name").ChangeAsync("Min have");
        await cut.Find(".btn-accent").ClickAsync();

        await calculatorService.Received(1).CalculateAsync(
            Arg.Is<PricingCalculationRequestDto>(r => r.ActiveGardens == 1 && r.ArchivedGardens == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Authenticated_ClickingContinue_MovesDirectlyToPaymentStep_UsingRealGardenCounts()
    {
        AuthorizeAs("user-1");
        queryService.GetOwnedGardenCountsAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(new OwnedGardenCountsDto(2, 1));
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(CreateResult(250m));

        var cut = Render<OnboardingCheckoutPage>();
        await cut.Find("#garden-name").ChangeAsync("Min have");
        await cut.Find(".btn-accent").ClickAsync();

        await calculatorService.Received(1).CalculateAsync(
            Arg.Is<PricingCalculationRequestDto>(r => r.ActiveGardens == 3 && r.ArchivedGardens == 1),
            Arg.Any<CancellationToken>());
        cut.Markup.Should().Contain("250,00");
    }

    [Fact]
    public async Task ChooseAuthenticationStep_ClickingRegister_SavesDraft_AndNavigatesToRegisterWithReturnUrl()
    {
        AddAuthorization().SetNotAuthorized();
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(CreateResult());
        onboardingService.SaveCheckoutDraftAsync(Arg.Any<SaveCheckoutDraftRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());

        var cut = Render<OnboardingCheckoutPage>();
        await cut.Find("#garden-name").ChangeAsync("Min have");
        await cut.Find(".btn-accent").ClickAsync();
        await cut.Find(".btn-primary").ClickAsync();

        await onboardingService.Received(1).SaveCheckoutDraftAsync(
            Arg.Is<SaveCheckoutDraftRequestDto>(r => r.UserId == null && r.GardenName == "Min have"),
            Arg.Any<CancellationToken>());

        Services.GetRequiredService<NavigationManager>().Uri.Should().Contain("/Account/Register");
    }

    [Fact]
    public void DraftQueryParam_UnknownDraft_ShowsExpiredEmptyState()
    {
        AuthorizeAs("user-1");
        queryService.GetOwnedGardenCountsAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(new OwnedGardenCountsDto(0, 0));
        onboardingService.GetCheckoutDraftAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((CheckoutDraftDto?)null);

        var cut = RenderWithDraft(Guid.NewGuid());

        cut.Markup.Should().Contain("udløbet");
    }

    [Fact]
    public void DraftQueryParam_NotAuthenticated_RedirectsToLoginAgain()
    {
        AddAuthorization().SetNotAuthorized();
        var draft = CreateDraft(Guid.NewGuid());
        onboardingService.GetCheckoutDraftAsync(draft.Id, Arg.Any<CancellationToken>()).Returns(draft);

        RenderWithDraft(draft.Id);

        Services.GetRequiredService<NavigationManager>().Uri.Should().Contain("/Account/Login");
    }

    [Fact]
    public void DraftQueryParam_ValidDraft_LoadsIntoPaymentStep()
    {
        AuthorizeAs("user-1");
        var draft = CreateDraft(Guid.NewGuid());

        onboardingService.GetCheckoutDraftAsync(draft.Id, Arg.Any<CancellationToken>()).Returns(draft);
        queryService.GetOwnedGardenCountsAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(new OwnedGardenCountsDto(0, 0));
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        var cut = RenderWithDraft(draft.Id);

        cut.Markup.Should().Contain("Min gemte have");
        cut.Markup.Should().Contain("Gennemfør betaling");
    }

    [Fact]
    public async Task PaymentStep_ConfirmingPayment_AuthenticatedFreshFlow_CallsProvisionPaidGardenAsync_AndNavigates()
    {
        AuthorizeAs("user-1");
        queryService.GetOwnedGardenCountsAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(new OwnedGardenCountsDto(0, 0));
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        var gardenId = Guid.NewGuid();
        onboardingService.ProvisionPaidGardenAsync(Arg.Any<PaidGardenProvisionRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(new PaidGardenProvisionResultDto(gardenId, Guid.NewGuid(), Guid.NewGuid()));

        var cut = Render<OnboardingCheckoutPage>();
        await cut.Find("#garden-name").ChangeAsync("Min have");
        await cut.Find(".btn-accent").ClickAsync();
        await cut.Find(".payment-mock-page .btn-primary").ClickAsync();

        await onboardingService.Received(1).ProvisionPaidGardenAsync(
            Arg.Is<PaidGardenProvisionRequestDto>(r => r.UserId == "user-1" && r.GardenName == "Min have"),
            Arg.Any<CancellationToken>());

        Services.GetRequiredService<NavigationManager>().Uri.Should().Contain($"/gardens/{gardenId}/access");
    }

    [Fact]
    public async Task PaymentStep_ResumedFromDraft_ConfirmingPayment_CallsProvisionPaidGardenFromDraftAsync()
    {
        AuthorizeAs("user-1");
        var draft = CreateDraft(Guid.NewGuid());

        onboardingService.GetCheckoutDraftAsync(draft.Id, Arg.Any<CancellationToken>()).Returns(draft);
        queryService.GetOwnedGardenCountsAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(new OwnedGardenCountsDto(0, 0));
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        var gardenId = Guid.NewGuid();
        onboardingService.ProvisionPaidGardenFromDraftAsync(draft.Id, "user-1", Arg.Any<CancellationToken>())
            .Returns(new PaidGardenProvisionResultDto(gardenId, Guid.NewGuid(), Guid.NewGuid()));

        var cut = RenderWithDraft(draft.Id);

        await cut.Find(".payment-mock-page .btn-primary").ClickAsync();

        await onboardingService.Received(1).ProvisionPaidGardenFromDraftAsync(draft.Id, "user-1", Arg.Any<CancellationToken>());
    }
}