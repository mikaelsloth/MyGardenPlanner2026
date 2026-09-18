namespace MyGardenPlanner2026.Components.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using System.Globalization;

/// <summary>
/// Trin "vælg opgradering" (PricingCalculator) foregår inden for samme kredsløb. Login/
/// Register kræver derimod en rigtig HTTP-navigation (static SSR, jf. OnboardingCheckoutPage) —
/// en evt. igangværende opgraderingskonfiguration overlever IKKE denne navigation, kun
/// selve invitations-tokenet (stateløst, bæres videre via query-strengen). Brugeren skal
/// derfor konfigurere en opgradering igen efter login/registrering, hvis den ønskes.
/// </summary>
public partial class InviteAcceptancePage
{
    private enum InviteStep { Details, ChooseAuthentication, Payment }

    private static readonly CultureInfo DanishCulture = new("da-DK");

    [Inject] private IOnboardingService OnboardingService { get; set; } = default!;
    [Inject] private IGardenAccessQueryService QueryService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    /// <summary>Public (samme fravigelse som DraftId i OnboardingCheckoutPage) så bUnit kan navigere direkte til URL'en med denne query-parameter.</summary>
    [SupplyParameterFromQuery(Name = "token")]
    public string? Token { get; set; }

    private bool isLoading = true;
    private bool isAuthenticated;
    private string? currentUserId;

    private bool isTokenValid;
    private GardenInvitationDto invitation = default!;
    private string gardenName = string.Empty;

    private InviteStep currentStep = InviteStep.Details;

    private IReadOnlyList<GardenAccessLevel> allowedLevels = [];
    private IReadOnlyList<AccessCategory> allowedCategories = [];

    private PricingSelectionDto? pendingUpgradeSelection;
    private bool isAccepting;
    private string? acceptErrorMessage;

    protected override async Task OnInitializedAsync()
    {
        var authState = AuthenticationStateTask is null ? null : await AuthenticationStateTask;
        isAuthenticated = authState?.User.Identity?.IsAuthenticated == true;
        currentUserId = isAuthenticated ? await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask) : null;

        var token = Token;
        if (string.IsNullOrWhiteSpace(token))
        {
            isTokenValid = false;
            isLoading = false;
            return;
        }

        var validation = await OnboardingService.ValidateInvitationTokenAsync(token);
        isTokenValid = validation.IsValid;

        if (isTokenValid)
        {
            invitation = validation.Invitation!;

            var garden = await QueryService.GetGardenSummaryAsync(invitation.GardenId);
            gardenName = garden?.Name ?? string.Empty;

            allowedLevels = [.. Enum.GetValues<GardenAccessLevel>()
                .Where(l => (int)l >= (int)invitation.MaxAllowedLayer && (int)l <= (int)invitation.TargetLayer)];
            allowedCategories = [.. Enum.GetValues<AccessCategory>()
                .Where(c => (int)c >= (int)invitation.TargetCategory && (int)c <= (int)invitation.MaxAllowedCategory)];
        }

        isLoading = false;
    }

    private Task AcceptBaselineAsync()
    {
        acceptErrorMessage = null;

        if (!isAuthenticated || currentUserId is null)
        {
            currentStep = InviteStep.ChooseAuthentication;
            return Task.CompletedTask;
        }

        return AcceptAsync(invitation.TargetLayer, invitation.TargetCategory, upgradeBillingCycle: null, new Dictionary<Guid, int>());
    }

    private Task HandleUpgradeContinueAsync(PricingSelectionDto selection)
    {
        pendingUpgradeSelection = selection;
        currentStep = isAuthenticated ? InviteStep.Payment : InviteStep.ChooseAuthentication;
        return Task.CompletedTask;
    }

    private void BackToDetails()
    {
        currentStep = InviteStep.Details;
        acceptErrorMessage = null;
    }

    private Task ContinueToRegisterAsync() => RedirectToAuthenticationAsync("/Account/Register");
    private Task ContinueToLoginAsync() => RedirectToAuthenticationAsync("/Account/Login");

    private Task RedirectToAuthenticationAsync(string authPath)
    {
        var token = Token ?? string.Empty;

        var returnUrl = QueryHelpers.AddQueryString(
            NavigationManager.ToAbsoluteUri("/invite").AbsoluteUri, "token", token);

        var target = QueryHelpers.AddQueryString(
            NavigationManager.ToAbsoluteUri(authPath).AbsoluteUri, "ReturnUrl", returnUrl);

        NavigationManager.NavigateTo(target, forceLoad: true);
        return Task.CompletedTask;
    }

    private Task ConfirmUpgradePaymentAsync()
    {
        return pendingUpgradeSelection is null
            ? Task.CompletedTask
            : AcceptAsync(
            pendingUpgradeSelection.Level, pendingUpgradeSelection.Category,
            pendingUpgradeSelection.BillingCycle, pendingUpgradeSelection.AddOnQuantities);
    }

    private async Task AcceptAsync(
        GardenAccessLevel grantedLayer, AccessCategory grantedCategory,
        BillingCycle? upgradeBillingCycle, IReadOnlyDictionary<Guid, int> addOnQuantities)
    {
        var token = Token;
        if (currentUserId is null || token is null)
        {
            return;
        }

        acceptErrorMessage = null;
        isAccepting = true;

        try
        {
            var request = new AcceptInvitationRequestDto(
                token, currentUserId, grantedLayer, grantedCategory, upgradeBillingCycle, addOnQuantities);

            var result = await OnboardingService.AcceptInvitationAsync(request);

            // OBS: /dashboard findes endnu ikke (kommer i Prompt 4 / AppShell) — navigerer
            // midlertidigt til havens adgangsside.
            NavigationManager.NavigateTo($"/gardens/{result.GardenId}/access");
        }
        catch (InvalidOperationException ex)
        {
            acceptErrorMessage = ex.Message;
        }
        finally
        {
            isAccepting = false;
        }
    }
}