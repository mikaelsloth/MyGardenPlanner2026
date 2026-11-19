namespace MyGardenPlanner2026.Components.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Trin 1 (konfiguration) og trin 3 (betaling) foregår inden for samme kredsløb/side —
/// ingen navigation, kun et lokalt trin-skift, så tilstanden bevares uden videre. Trin 2
/// (Opret konto/Log ind) kræver derimod en rigtig HTTP-navigation, da Identity's
/// Register/Login-sider er static SSR (kan sætte auth-cookien), hvilket river dette
/// kredsløb ned — konfigurationen persisteres derfor til en CheckoutDraft først, og
/// genindlæses via ?draft=-parameteren, når brugeren vender tilbage.
/// R10.1e: er brugeren logget ind og har gemte (slettede) haver, tilbydes gendannelse i
/// betalingstrinnet i stedet for at oprette en ny have; købet skal dække havens tidligere niveau.
/// </summary>
public partial class OnboardingCheckoutPage
{
    private enum CheckoutStep { Configuration, ChooseAuthentication, Payment }

    [Inject] private IOnboardingService OnboardingService { get; set; } = default!;
    [Inject] private IGardenAccessQueryService QueryService { get; set; } = default!;
    [Inject] private IGardenRestoreService RestoreService { get; set; } = default!;
    [Inject] private IPricingCalculatorService CalculatorService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    /// <summary>Public (fraviger ellers privat konvention for SupplyParameterFromQuery) så bUnit kan sætte den direkte uden router-integration.</summary>
    [SupplyParameterFromQuery(Name = "draft")]
    public Guid? DraftId { get; set; }

    /// <summary>Bruges til at deep-linke fra fx landingssidens prisknapper direkte ind i en forudkonfigureret checkout.</summary>
    [SupplyParameterFromQuery(Name = "level")]
    public string? PreselectedLevel { get; set; }

    [SupplyParameterFromQuery(Name = "category")]
    public string? PreselectedCategory { get; set; }

    [SupplyParameterFromQuery(Name = "cycle")]
    public string? PreselectedCycle { get; set; }

    // Valg fra en indlæst kladde har forrang for query-parametrene (bruges, når brugeren vender
    // tilbage til konfigurationen fra betalingstrinnet).

    private GardenAccessLevel? InitialLevel
    {
        get =>
        field ?? (Enum.TryParse<GardenAccessLevel>(PreselectedLevel, out var level) ? level : default(GardenAccessLevel?)); set;
    }

    private AccessCategory? InitialCategory
    {
        get =>
        field ?? (Enum.TryParse<AccessCategory>(PreselectedCategory, out var category) ? category : default(AccessCategory?)); set;
    }

    private BillingCycle? InitialCycle
    {
        get =>
        field ?? (Enum.TryParse<BillingCycle>(PreselectedCycle, out var cycle) ? cycle : default(BillingCycle?)); set;
    }

    private bool isLoading = true;
    private bool isAuthenticated;
    private string? currentUserId;
    private string? currentUserEmail;
    private bool draftNotFound;
    private string? loadErrorMessage;
    private Guid? loadedDraftId;

    private CheckoutStep currentStep = CheckoutStep.Configuration;

    private string gardenName = string.Empty;
    private string? description;
    private string? configurationErrorMessage;

    private int fixedActiveGardens = 1;
    private int fixedArchivedGardens;

    private IReadOnlyList<RestorableGardenDto> restorableGardens = [];
    private Guid? selectedRestoreId;

    private PricingSelectionDto? pendingSelection;
    private bool isSubmittingPayment;
    private string? paymentErrorMessage;

    private RestorableGardenDto? SelectedRestore =>
        selectedRestoreId is { } id ? restorableGardens.FirstOrDefault(g => g.PendingRestoreId == id) : null;

    private string PaymentGardenName => SelectedRestore?.Name ?? gardenName;

    private string ConfirmButtonLabel =>
        SelectedRestore is null ? "Gennemfør betaling og opret have" : "Gennemfør betaling og gendan have";

    protected override async Task OnInitializedAsync()
    {
        var authState = AuthenticationStateTask is null ? null : await AuthenticationStateTask;
        isAuthenticated = authState?.User.Identity?.IsAuthenticated == true;
        currentUserId = isAuthenticated ? await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask) : null;

        if (isAuthenticated && currentUserId is not null)
        {
            var ownedCounts = await QueryService.GetOwnedGardenCountsAsync(currentUserId);
            fixedActiveGardens = ownedCounts.ActiveCount + 1;
            fixedArchivedGardens = ownedCounts.ArchivedCount;

            currentUserEmail = await CurrentUserEmailResolver.ResolveAsync(AuthenticationStateTask);
            await RefreshRestorableGardensAsync();
        }

        if (DraftId is { } draftId)
        {
            await LoadDraftAsync(draftId);
        }

        isLoading = false;
    }

    private async Task LoadDraftAsync(Guid draftId)
    {
        var draft = await OnboardingService.GetCheckoutDraftAsync(draftId);
        if (draft is null)
        {
            draftNotFound = true;
            return;
        }

        gardenName = draft.GardenName;
        description = draft.Description;

        if (!isAuthenticated || currentUserId is null)
        {
            // Bør ikke normalt ske (ReturnUrl sender kun hertil efter gennemført login),
            // men send brugeren til login igen for en sikkerheds skyld, med samme draft.
            RedirectToAuthentication("/Account/Login", draftId);
            return;
        }

        try
        {
            var priceRequest = new PricingCalculationRequestDto(
                draft.Layer, draft.Category, draft.BillingCycle,
                fixedActiveGardens, fixedArchivedGardens, draft.AddOnQuantities);
            var priceResult = await CalculatorService.CalculateAsync(priceRequest);

            pendingSelection = new PricingSelectionDto(
                draft.Layer, draft.Category, draft.BillingCycle, draft.AddOnQuantities, priceResult);
            loadedDraftId = draftId;
            InitialLevel = draft.Layer;
            InitialCategory = draft.Category;
            InitialCycle = draft.BillingCycle;
            currentStep = CheckoutStep.Payment;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            loadErrorMessage = "Kunne ikke genberegne prisen for din konfiguration. Kontakt venligst support.";
        }
    }

    private Task HandleConfigurationContinueAsync(PricingSelectionDto selection)
    {
        configurationErrorMessage = null;

        if (string.IsNullOrWhiteSpace(gardenName))
        {
            configurationErrorMessage = "Angiv venligst et navn til din have.";
            return Task.CompletedTask;
        }

        pendingSelection = selection;

        // Et nyt valg erstatter en evt. indlæst kladde og et tidligere gendannelsesvalg (som skal dækkes af det nye køb).
        loadedDraftId = null;
        selectedRestoreId = null;

        currentStep = isAuthenticated ? CheckoutStep.Payment : CheckoutStep.ChooseAuthentication;

        return Task.CompletedTask;
    }

    private void BackToConfiguration()
    {
        currentStep = CheckoutStep.Configuration;
        configurationErrorMessage = null;
        paymentErrorMessage = null;

        // Betalingen skal bruge det nye valg, ikke den tidligere indlæste kladde.
        loadedDraftId = null;
        selectedRestoreId = null;
    }

    private Task ContinueToRegisterAsync() => SaveDraftAndRedirectAsync("/Account/Register");
    private Task ContinueToLoginAsync() => SaveDraftAndRedirectAsync("/Account/Login");

    private async Task SaveDraftAndRedirectAsync(string authPath)
    {
        if (pendingSelection is null)
        {
            return;
        }

        var request = new SaveCheckoutDraftRequestDto(
            null, gardenName, description, pendingSelection.Level, pendingSelection.Category,
            pendingSelection.BillingCycle, pendingSelection.AddOnQuantities);

        var draftId = await OnboardingService.SaveCheckoutDraftAsync(request);

        RedirectToAuthentication(authPath, draftId);
    }

    private void RedirectToAuthentication(string authPath, Guid draftId)
    {
        var returnUrl = NavigationManager.GetUriWithQueryParameters(
            NavigationManager.ToAbsoluteUri("/onboarding/checkout").AbsoluteUri,
            new Dictionary<string, object?> { ["draft"] = draftId.ToString() });

        var target = NavigationManager.GetUriWithQueryParameters(
            NavigationManager.ToAbsoluteUri(authPath).AbsoluteUri,
            new Dictionary<string, object?> { ["ReturnUrl"] = returnUrl });

        NavigationManager.NavigateTo(target, forceLoad: true);
    }

    /// <summary>
    /// Indlæser brugerens gendannelige haver (kræver bekræftet e-mail fra login) og fjerner et
    /// valg, der ikke længere findes (fx udløbet).
    /// </summary>
    private async Task RefreshRestorableGardensAsync()
    {
        if (string.IsNullOrWhiteSpace(currentUserEmail))
        {
            return;
        }

        restorableGardens = await RestoreService.GetRestorableGardensAsync(currentUserEmail);

        if (selectedRestoreId is { } id && restorableGardens.All(g => g.PendingRestoreId != id))
        {
            selectedRestoreId = null;
        }
    }

    private async Task ConfirmPaymentAsync()
    {
        if (pendingSelection is null || currentUserId is null)
        {
            return;
        }

        paymentErrorMessage = null;
        isSubmittingPayment = true;

        var restore = currentUserEmail is null ? null : SelectedRestore;

        try
        {
            PaidGardenProvisionResultDto result = restore is not null && currentUserEmail is not null
                ? loadedDraftId is { } restoreDraftId
                    ? await OnboardingService.ProvisionRestoredGardenFromDraftAsync(
                        restoreDraftId, currentUserId, currentUserEmail, restore.PendingRestoreId)
                    : await OnboardingService.ProvisionRestoredGardenAsync(new PaidGardenRestoreRequestDto(
                        currentUserId, currentUserEmail, restore.PendingRestoreId, pendingSelection.Level,
                        pendingSelection.Category, pendingSelection.BillingCycle, pendingSelection.AddOnQuantities))
                : loadedDraftId is { } draftId
                    ? await OnboardingService.ProvisionPaidGardenFromDraftAsync(draftId, currentUserId)
                    : await OnboardingService.ProvisionPaidGardenAsync(new PaidGardenProvisionRequestDto(
                        currentUserId, gardenName, description, pendingSelection.Level, pendingSelection.Category,
                        pendingSelection.BillingCycle, pendingSelection.AddOnQuantities));

            // OBS: /dashboard findes endnu ikke (kommer i Prompt 4 / AppShell) — navigerer
            // midlertidigt til havens adgangsside, som er den eneste fungerende destination.
            NavigationManager.NavigateTo($"/gardens/{result.GardenId}/access");
        }
        catch (InvalidOperationException ex)
        {
            paymentErrorMessage = ex.Message;

            if (restore is not null)
            {
                // Fx udløbet gendannelse: opdater listen, så brugeren ikke fastholdes i et ugyldigt valg.
                await RefreshRestorableGardensAsync();
            }
        }
        finally
        {
            isSubmittingPayment = false;
        }
    }
}