namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Globalization;

/// <summary>
/// Alle mutationer (Gem/Tilføj/Slet/Nulstil) er følsomme "core policy"-handlinger (§3.2)
/// og kræver derfor step-up re-autentificering — håndhævet via StepUpGuard (PR5).
/// </summary>
public partial class AddOnEditor
{
    [Inject]
    private ISubscriptionAddOnAdminService AdminService { get; set; } = default!;

    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = default!;

    [Inject]
    private IAdminActionRateLimiter RateLimiter { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

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
    private StepUpGuard stepUpGuard = default!;
    private AdminActionGuard adminActionGuard = default!;

    private AddOnType newType = AddOnType.BedforslagNiveau2;
    private string newName = string.Empty;
    private string newUnitDescription = string.Empty;
    private decimal newAnnualPrice;
    private decimal newMonthlyPrice;
    private decimal newPerpetualPrice;

    protected override async Task OnInitializedAsync()
    {
        stepUpGuard = new StepUpGuard(AuthorizationService, AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy);
        adminActionGuard = new AdminActionGuard(RateLimiter);
        await LoadAsync();
    }

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
        await adminActionGuard.RunAsync(AuthenticationStateTask, () =>
            stepUpGuard.RunAsync(AuthenticationStateTask, () => SaveExistingCoreAsync(addOnId)));

        if (adminActionGuard.IsRateLimited)
        {
            errorMessage = "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen.";
        }
    }

    private async Task SaveExistingCoreAsync(Guid addOnId)
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
        await adminActionGuard.RunAsync(AuthenticationStateTask, () =>
            stepUpGuard.RunAsync(AuthenticationStateTask, AddNewCoreAsync));

        if (adminActionGuard.IsRateLimited)
        {
            errorMessage = "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen.";
        }
    }

    private async Task AddNewCoreAsync()
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
        await adminActionGuard.RunAsync(AuthenticationStateTask, () =>
            stepUpGuard.RunAsync(AuthenticationStateTask, () => DeleteCoreAsync(id)));

        if (adminActionGuard.IsRateLimited)
        {
            errorMessage = "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen.";
        }
    }

    private async Task DeleteCoreAsync(Guid id)
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

        await adminActionGuard.RunAsync(AuthenticationStateTask, () =>
            stepUpGuard.RunAsync(AuthenticationStateTask, ConfirmResetCoreAsync));

        if (adminActionGuard.IsRateLimited)
        {
            errorMessage = "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen.";
        }
    }

    private async Task ConfirmResetCoreAsync()
    {
        await AdminService.ResetToDefaultAsync();
        await LoadAsync();
        await OnStatusMessage.InvokeAsync("Tilkøbsmodulerne er nulstillet til standardkataloget.");
    }

    private static decimal ParseDecimal(object? value) =>
        decimal.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0m;
}