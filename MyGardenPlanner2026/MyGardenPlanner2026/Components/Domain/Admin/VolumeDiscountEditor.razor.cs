namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using System.Globalization;

public partial class VolumeDiscountEditor
{
    [Inject]
    private IGardenVolumeDiscountAdminService AdminService { get; set; } = default!;

    [Parameter]
    public EventCallback<string> OnStatusMessage { get; set; }

    private IReadOnlyList<GardenVolumeDiscountTierDto> tiers = [];
    private readonly Dictionary<int, int> minEdits = [];
    private readonly Dictionary<int, int?> maxEdits = [];
    private readonly Dictionary<int, decimal> multiplierEdits = [];
    private string? errorMessage;
    private bool showResetConfirm;

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

    private async Task SaveExistingAsync(int tierId)
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

    private async Task AddNewAsync()
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

    private async Task DeleteAsync(int id)
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
        showResetConfirm = false;
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