namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Globalization;

public partial class AddOnEditor
{
    [Inject]
    private ISubscriptionAddOnAdminService AdminService { get; set; } = default!;

    [Parameter]
    public EventCallback<string> OnStatusMessage { get; set; }

    private IReadOnlyList<SubscriptionAddOnDto> addOns = [];
    private readonly Dictionary<Guid, string> nameEdits = [];
    private readonly Dictionary<Guid, string> unitEdits = [];
    private readonly Dictionary<Guid, decimal> annualEdits = [];
    private readonly Dictionary<Guid, decimal> monthlyEdits = [];
    private readonly Dictionary<Guid, decimal> perpetualEdits = [];
    private string? errorMessage;
    private bool showResetConfirm;

    private AddOnType newType = AddOnType.BedforslagNiveau2;
    private string newName = string.Empty;
    private string newUnitDescription = string.Empty;
    private decimal newAnnualPrice;
    private decimal newMonthlyPrice;
    private decimal newPerpetualPrice;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        addOns = await AdminService.GetAllAsync();

        nameEdits.Clear();
        unitEdits.Clear();
        annualEdits.Clear();
        monthlyEdits.Clear();
        perpetualEdits.Clear();

        foreach (var addOn in addOns)
        {
            nameEdits[addOn.Id] = addOn.Name;
            unitEdits[addOn.Id] = addOn.UnitDescription;
            annualEdits[addOn.Id] = addOn.AnnualPrice;
            monthlyEdits[addOn.Id] = addOn.MonthlyPrice;
            perpetualEdits[addOn.Id] = addOn.PerpetualPrice;
        }
    }

    private async Task SaveExistingAsync(Guid addOnId)
    {
        errorMessage = null;
        try
        {
            var currentType = addOns.Single(a => a.Id == addOnId).Type;

            await AdminService.SaveAsync(new SubscriptionAddOnUpsertDto(
                addOnId,
                currentType,
                nameEdits[addOnId],
                unitEdits[addOnId],
                annualEdits[addOnId],
                monthlyEdits[addOnId],
                perpetualEdits[addOnId]));

            var savedName = nameEdits[addOnId];
            await LoadAsync();
            await OnStatusMessage.InvokeAsync($"Tilkøb '{savedName}' er opdateret.");
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
            await AdminService.SaveAsync(new SubscriptionAddOnUpsertDto(
                null, newType, newName, newUnitDescription, newAnnualPrice, newMonthlyPrice, newPerpetualPrice));

            var savedName = newName;
            await LoadAsync();
            await OnStatusMessage.InvokeAsync($"Nyt tilkøb '{savedName}' er oprettet.");

            newName = string.Empty;
            newUnitDescription = string.Empty;
            newAnnualPrice = 0m;
            newMonthlyPrice = 0m;
            newPerpetualPrice = 0m;
        }
        catch (InvalidOperationException ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task DeleteAsync(Guid id)
    {
        errorMessage = null;
        try
        {
            await AdminService.DeleteAsync(id);
            await LoadAsync();
            await OnStatusMessage.InvokeAsync("Tilkøbet er slettet.");
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
        await OnStatusMessage.InvokeAsync("Tilkøbsmodulerne er nulstillet til standardkataloget.");
    }

    private static decimal ParseDecimal(object? value) =>
        decimal.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0m;
}