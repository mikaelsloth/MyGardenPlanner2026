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
using NSubstitute.ExceptionExtensions;
using System.Security.Claims;
using Xunit;

public sealed class InviteAcceptancePageTests : BunitContext
{
    private readonly IOnboardingService onboardingService = Substitute.For<IOnboardingService>();
    private readonly IGardenAccessQueryService queryService = Substitute.For<IGardenAccessQueryService>();
    private readonly IPricingCalculatorService calculatorService = Substitute.For<IPricingCalculatorService>();
    private readonly ISubscriptionAddOnService addOnService = Substitute.For<ISubscriptionAddOnService>();

    public InviteAcceptancePageTests()
    {
        addOnService.GetAllAddOnsAsync(Arg.Any<CancellationToken>()).Returns([]);
        Services.AddSingleton(onboardingService);
        Services.AddSingleton(queryService);
        Services.AddSingleton(calculatorService);
        Services.AddSingleton(addOnService);
    }

    private void AuthorizeAs(string userId)
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized(userId);
        authContext.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    private static GardenInvitationDto CreateInvitationDto(
        Guid gardenId, bool allowSelfUpgrade = false,
        GardenAccessLevel targetLayer = GardenAccessLevel.Planlaegger, AccessCategory targetCategory = AccessCategory.Viewer,
        GardenAccessLevel maxLayer = GardenAccessLevel.Planlaegger, AccessCategory maxCategory = AccessCategory.Viewer) =>
        new(Guid.NewGuid(), gardenId, "owner", "invited@example.com", targetLayer, targetCategory,
            false, allowSelfUpgrade, DateTimeOffset.UtcNow.AddDays(7), false, false, DateTimeOffset.UtcNow,
            maxLayer, maxCategory);

    private static PricingCalculationResultDto CreateResult(decimal total = 150m) =>
        new(100m, 1m, 1.0m, 100m, [], 50m, total);

    private IRenderedComponent<InviteAcceptancePage> RenderWithToken(string token)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo($"/invite?token={token}");
        return Render<InviteAcceptancePage>();
    }

    [Fact]
    public void NoToken_ShowsInvalidEmptyState()
    {
        AddAuthorization().SetNotAuthorized();

        var cut = Render<InviteAcceptancePage>();

        cut.Markup.Should().Contain("ugyldig eller udløbet");
    }

    [Fact]
    public void InvalidToken_ShowsInvalidEmptyState()
    {
        AddAuthorization().SetNotAuthorized();
        onboardingService.ValidateInvitationTokenAsync("bad-token", Arg.Any<CancellationToken>())
            .Returns(new InvitationValidationResultDto(false, null, "Invitationen findes ikke eller er ugyldig."));

        var cut = RenderWithToken("bad-token");

        cut.Markup.Should().Contain("ugyldig eller udløbet");
    }

    [Fact]
    public void ValidToken_NoSelfUpgrade_ShowsSingleAcceptButton_NoPricingCalculator()
    {
        AddAuthorization().SetNotAuthorized();
        var gardenId = Guid.NewGuid();
        var invitation = CreateInvitationDto(gardenId, allowSelfUpgrade: false);

        onboardingService.ValidateInvitationTokenAsync("good-token", Arg.Any<CancellationToken>())
            .Returns(new InvitationValidationResultDto(true, invitation, null));
        queryService.GetGardenSummaryAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns(new GardenSummaryDto(gardenId, "Testhave", false));

        var cut = RenderWithToken("good-token");

        cut.FindAll("#calc-level").Should().BeEmpty();
        cut.Find(".form-actions .btn-primary").TextContent.Should().Be("Accepter invitation og fortsæt");
    }

    [Fact]
    public void ValidToken_AllowSelfUpgrade_ShowsPricingCalculatorAndBaselineButton()
    {
        AddAuthorization().SetNotAuthorized();
        var gardenId = Guid.NewGuid();
        var invitation = CreateInvitationDto(gardenId, allowSelfUpgrade: true,
            targetLayer: GardenAccessLevel.Planlaegger, maxLayer: GardenAccessLevel.BedDesigner);

        onboardingService.ValidateInvitationTokenAsync("good-token", Arg.Any<CancellationToken>())
            .Returns(new InvitationValidationResultDto(true, invitation, null));
        queryService.GetGardenSummaryAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns(new GardenSummaryDto(gardenId, "Testhave", false));

        var cut = RenderWithToken("good-token");

        cut.FindAll("#calc-level").Should().HaveCount(1);
        cut.Find(".form-actions .btn-primary").TextContent.Should().Be("Accepter invitation uden opgradering");
    }

    [Fact]
    public void AllowSelfUpgrade_PricingCalculator_OnlyOffersLevelsWithinCeiling()
    {
        AddAuthorization().SetNotAuthorized();
        var gardenId = Guid.NewGuid();
        var invitation = CreateInvitationDto(gardenId, allowSelfUpgrade: true,
            targetLayer: GardenAccessLevel.Planlaegger, maxLayer: GardenAccessLevel.BedDesigner);

        onboardingService.ValidateInvitationTokenAsync("good-token", Arg.Any<CancellationToken>())
            .Returns(new InvitationValidationResultDto(true, invitation, null));
        queryService.GetGardenSummaryAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns(new GardenSummaryDto(gardenId, "Testhave", false));

        var cut = RenderWithToken("good-token");

        var options = cut.FindAll("#calc-level option").Select(o => o.GetAttribute("value"));
        options.Should().BeEquivalentTo(nameof(GardenAccessLevel.BedDesigner), nameof(GardenAccessLevel.Planlaegger));
    }

    [Fact]
    public async Task Authenticated_ClickingBaselineAccept_CallsAcceptInvitationAsync_WithTargetLevelAndNullUpgrade_AndNavigates()
    {
        AuthorizeAs("user-1");
        var gardenId = Guid.NewGuid();
        var invitation = CreateInvitationDto(gardenId, allowSelfUpgrade: false,
            targetLayer: GardenAccessLevel.Planlaegger, targetCategory: AccessCategory.Viewer);

        onboardingService.ValidateInvitationTokenAsync("good-token", Arg.Any<CancellationToken>())
            .Returns(new InvitationValidationResultDto(true, invitation, null));
        queryService.GetGardenSummaryAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns(new GardenSummaryDto(gardenId, "Testhave", false));
        onboardingService.AcceptInvitationAsync(Arg.Any<AcceptInvitationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(new AcceptInvitationResultDto(gardenId, Guid.NewGuid(), null));

        var cut = RenderWithToken("good-token");
        await cut.Find(".form-actions .btn-primary").ClickAsync();

        await onboardingService.Received(1).AcceptInvitationAsync(
            Arg.Is<AcceptInvitationRequestDto>(r =>
                r.RawToken == "good-token" && r.UserId == "user-1" &&
                r.GrantedLayer == GardenAccessLevel.Planlaegger && r.GrantedCategory == AccessCategory.Viewer &&
                r.UpgradeBillingCycle == null),
            Arg.Any<CancellationToken>());

        Services.GetRequiredService<NavigationManager>().Uri.Should().Contain($"/gardens/{gardenId}/access");
    }

    [Fact]
    public async Task NotAuthenticated_ClickingBaselineAccept_ShowsChooseAuthenticationStep()
    {
        AddAuthorization().SetNotAuthorized();
        var gardenId = Guid.NewGuid();
        var invitation = CreateInvitationDto(gardenId);

        onboardingService.ValidateInvitationTokenAsync("good-token", Arg.Any<CancellationToken>())
            .Returns(new InvitationValidationResultDto(true, invitation, null));
        queryService.GetGardenSummaryAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns(new GardenSummaryDto(gardenId, "Testhave", false));

        var cut = RenderWithToken("good-token");
        await cut.Find(".form-actions .btn-primary").ClickAsync();

        cut.Markup.Should().Contain("Opret konto eller log ind");
        await onboardingService.DidNotReceive().AcceptInvitationAsync(Arg.Any<AcceptInvitationRequestDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ChooseAuthenticationStep_ClickingRegister_NavigatesToRegister_WithTokenInReturnUrl()
    {
        AddAuthorization().SetNotAuthorized();
        var gardenId = Guid.NewGuid();
        var invitation = CreateInvitationDto(gardenId);

        onboardingService.ValidateInvitationTokenAsync("good-token", Arg.Any<CancellationToken>())
            .Returns(new InvitationValidationResultDto(true, invitation, null));
        queryService.GetGardenSummaryAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns(new GardenSummaryDto(gardenId, "Testhave", false));

        var cut = RenderWithToken("good-token");
        cut.Find(".form-actions .btn-primary").Click();
        cut.Find(".btn-row .btn-primary").Click();

        var uri = Services.GetRequiredService<NavigationManager>().Uri;
        uri.Should().Contain("/Account/Register");
        var containsToken =
            uri.Contains("token%3Dgood-token") ||
            uri.Contains("token=good-token");

        containsToken.Should().BeTrue();
    }

    [Fact]
    public void AllowSelfUpgrade_Authenticated_ConfiguringUpgrade_MovesToPaymentStep()
    {
        AuthorizeAs("user-1");
        var gardenId = Guid.NewGuid();
        var invitation = CreateInvitationDto(gardenId, allowSelfUpgrade: true,
            targetLayer: GardenAccessLevel.Planlaegger, maxLayer: GardenAccessLevel.BedDesigner);

        onboardingService.ValidateInvitationTokenAsync("good-token", Arg.Any<CancellationToken>())
            .Returns(new InvitationValidationResultDto(true, invitation, null));
        queryService.GetGardenSummaryAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns(new GardenSummaryDto(gardenId, "Testhave", false));
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        var cut = RenderWithToken("good-token");
        cut.Find(".btn-accent").Click();

        cut.Markup.Should().Contain("Gennemfør betaling");
    }

    [Fact]
    public async Task PaymentStep_ConfirmingPayment_CallsAcceptInvitationAsync_WithUpgradeSelection_AndNavigates()
    {
        AuthorizeAs("user-1");
        var gardenId = Guid.NewGuid();
        var invitation = CreateInvitationDto(gardenId, allowSelfUpgrade: true,
            targetLayer: GardenAccessLevel.Planlaegger, maxLayer: GardenAccessLevel.BedDesigner);

        onboardingService.ValidateInvitationTokenAsync("good-token", Arg.Any<CancellationToken>())
            .Returns(new InvitationValidationResultDto(true, invitation, null));
        queryService.GetGardenSummaryAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns(new GardenSummaryDto(gardenId, "Testhave", false));
        calculatorService.CalculateAsync(Arg.Any<PricingCalculationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(CreateResult());
        onboardingService.AcceptInvitationAsync(Arg.Any<AcceptInvitationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(new AcceptInvitationResultDto(gardenId, Guid.NewGuid(), Guid.NewGuid()));

        var cut = RenderWithToken("good-token");
        await cut.Find(".btn-accent").ClickAsync();
        await cut.Find(".payment-mock-page .btn-primary").ClickAsync();

        await onboardingService.Received(1).AcceptInvitationAsync(
            Arg.Is<AcceptInvitationRequestDto>(r => r.UserId == "user-1" && r.UpgradeBillingCycle == BillingCycle.Annual),
            Arg.Any<CancellationToken>());

        Services.GetRequiredService<NavigationManager>().Uri.Should().Contain($"/gardens/{gardenId}/access");
    }

    [Fact]
    public void AcceptInvitationAsync_Throws_ShowsErrorMessage_WithoutNavigating()
    {
        AuthorizeAs("user-1");
        var gardenId = Guid.NewGuid();
        var invitation = CreateInvitationDto(gardenId, allowSelfUpgrade: false);

        onboardingService.ValidateInvitationTokenAsync("good-token", Arg.Any<CancellationToken>())
            .Returns(new InvitationValidationResultDto(true, invitation, null));
        queryService.GetGardenSummaryAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns(new GardenSummaryDto(gardenId, "Testhave", false));
        onboardingService.AcceptInvitationAsync(Arg.Any<AcceptInvitationRequestDto>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Du er allerede medlem af denne have."));

        var cut = RenderWithToken("good-token");
        var originalUri = Services.GetRequiredService<NavigationManager>().Uri;
        cut.Find(".form-actions .btn-primary").Click();

        cut.Markup.Should().Contain("Du er allerede medlem af denne have.");
        Services.GetRequiredService<NavigationManager>().Uri.Should().Be(originalUri);
    }
}