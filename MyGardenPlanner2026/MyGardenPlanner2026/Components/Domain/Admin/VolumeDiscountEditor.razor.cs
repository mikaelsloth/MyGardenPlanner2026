namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using System.Globalization;

/// <summary>
/// Alle mutationer (Gem/Tilføj/Slet/Nulstil) er følsomme "core policy"-handlinger (§3.2)
/// og kræver derfor step-up re-autentificering, håndhævet direkte i backend-event-
/// handlerne — samme mønster som BasePriceMatrixEditor (PR3).
/// </summary>
public partial class VolumeDiscountEditor
{
    [Inject]
    private IGardenVolumeDiscountAdminService AdminService { get; set; } = default!;

    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    [Parameter]
    public EventCallback<string> OnStatusMessage { get; set; }

    private IReadOnlyList<GardenVolumeDiscountTierDto> tiers = [];
    private readonly Dictionary<Guid, int> minEdits = [];
    private readonly Dictionary<Guid, int?> maxEdits = [];
    private readonly Dictionary<Guid, decimal> multiplierEdits = [];
    private string? errorMessage;
    private bool showResetConfirm;
    private bool showStepUpModal;
    private Func<Task>? pendingAction;

    private int newMinGardens = 1;
    private int? newMaxGardens;
    private decimal newPriceMultiplier = 1.00m;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        tiers = await AdminService.GetAllAsync();

        minEdits.Clear();
        maxEdits.Clear();
        multiplierEdits.Clear();

        foreach (var tier in tiers)
        {
            minEdits[tier.Id] = tier.MinGardens;
            maxEdits[tier.Id] = tier.MaxGardens;
            multiplierEdits[tier.Id] = tier.PriceMultiplier;
        }
    }

    private async Task SaveExistingAsync(Guid tierId) =>
        await RunWithStepUpAsync(() => SaveExistingCoreAsync(tierId));

    private async Task SaveExistingCoreAsync(Guid tierId)
    {
        errorMessage = null;
        try
        {
            await AdminService.SaveAsync(new GardenVolumeDiscountTierUpsertDto(
                tierId, minEdits[tierId], maxEdits[tierId], multiplierEdits[tierId]));

            var minGardens = minEdits[tierId];
            await LoadAsync();
            await OnStatusMessage.InvokeAsync($"Volumenrabat-trappe fra {minGardens} haver er opdateret.");
        }
        catch (InvalidOperationException ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task AddNewAsync() =>
        await RunWithStepUpAsync(AddNewCoreAsync);

    private async Task AddNewCoreAsync()
    {
        errorMessage = null;
        try
        {
            await AdminService.SaveAsync(new GardenVolumeDiscountTierUpsertDto(
                null, newMinGardens, newMaxGardens, newPriceMultiplier));

            var minGardens = newMinGardens;
            await LoadAsync();
            await OnStatusMessage.InvokeAsync($"Ny volumenrabat-trappe fra {minGardens} haver er oprettet.");

            newMinGardens = 1;
            newMaxGardens = null;
            newPriceMultiplier = 1.00m;
        }
        catch (InvalidOperationException ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task DeleteAsync(Guid id) =>
        await RunWithStepUpAsync(() => DeleteCoreAsync(id));

    private async Task DeleteCoreAsync(Guid id)
    {
        errorMessage = null;
        try
        {
            await AdminService.DeleteAsync(id);
            await LoadAsync();
            await OnStatusMessage.InvokeAsync("Volumenrabat-trappen er slettet.");
        }
        catch (InvalidOperationException ex)
        {
            errorMessage = ex.Message;
        }
    }

    private void RequestReset() => showResetConfirm = true;
    private void CancelReset() => showResetConfirm = false;

    private async Task ConfirmResetAsync()
    {
        // Lukker inline-confirm popoveren med det samme; selve nulstillingen ventes
        // stadig af step-up-guarden nedenfor.
        showResetConfirm = false;
        await RunWithStepUpAsync(ConfirmResetCoreAsync);
    }

    private async Task ConfirmResetCoreAsync()
    {
        await AdminService.ResetToDefaultAsync();
        await LoadAsync();
        await OnStatusMessage.InvokeAsync("Volumenrabat-trapperne er nulstillet til standardkataloget.");
    }

    private async Task RunWithStepUpAsync(Func<Task> action)
    {
        if (await HasRecentAuthenticationAsync())
        {
            await action();
            return;
        }

        pendingAction = action;
        showStepUpModal = true;
    }

    private async Task<bool> HasRecentAuthenticationAsync()
    {
        if (AuthenticationStateTask is null)
        {
            return false;
        }

        var authState = await AuthenticationStateTask;
        var result = await AuthorizationService.AuthorizeAsync(
            authState.User, resource: null, AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy);

        return result.Succeeded;
    }

    private async Task ExecutePendingActionAsync()
    {
        showStepUpModal = false;

        if (pendingAction is not null)
        {
            var action = pendingAction;
            pendingAction = null;
            await action();
        }
    }

    private void CancelStepUp()
    {
        showStepUpModal = false;
        pendingAction = null;
    }

    private static decimal ParseDecimal(object? value) =>
        decimal.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0m;

    private static int ParseInt(object? value) =>
        int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out var result) ? result : 0;

    private static int? ParseNullableInt(object? value)
    {
        var text = Convert.ToString(value, CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(text) ? null : int.Parse(text, CultureInfo.InvariantCulture);
    }
}