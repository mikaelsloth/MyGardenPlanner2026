namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using System.Globalization;

/// <summary>
/// Alle mutationer (Gem/Tilføj/Slet/Nulstil) er følsomme "core policy"-handlinger (§3.2)
/// og kræver derfor step-up re-autentificering — håndhævet via StepUpGuard (PR5).
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
    private StepUpGuard stepUpGuard = default!;

    private int newMinGardens = 1;
    private int? newMaxGardens;
    private decimal newPriceMultiplier = 1.00m;

    protected override async Task OnInitializedAsync()
    {
        stepUpGuard = new StepUpGuard(AuthorizationService, AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy);
        await LoadAsync();
    }

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

    private Task SaveExistingAsync(Guid tierId) =>
        stepUpGuard.RunAsync(AuthenticationStateTask, () => SaveExistingCoreAsync(tierId));

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

    private Task AddNewAsync() =>
        stepUpGuard.RunAsync(AuthenticationStateTask, AddNewCoreAsync);

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

    private Task DeleteAsync(Guid id) =>
        stepUpGuard.RunAsync(AuthenticationStateTask, () => DeleteCoreAsync(id));

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

    private Task ConfirmResetAsync()
    {
        showResetConfirm = false;
        return stepUpGuard.RunAsync(AuthenticationStateTask, ConfirmResetCoreAsync);
    }

    private async Task ConfirmResetCoreAsync()
    {
        await AdminService.ResetToDefaultAsync();
        await LoadAsync();
        await OnStatusMessage.InvokeAsync("Volumenrabat-trapperne er nulstillet til standardkataloget.");
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