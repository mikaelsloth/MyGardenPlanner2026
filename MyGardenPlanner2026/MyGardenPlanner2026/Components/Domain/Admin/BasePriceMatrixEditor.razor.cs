namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using System.Globalization;

/// <summary>
/// Gemning af basispriser er en følsom "core policy"-handling (§3.2) og kræver derfor
/// step-up re-autentificering (RequireRecentAuthentication-policy). Håndhæves direkte
/// i backend-event-handleren — ikke kun via UI-skjulning — da manipulerede WebSocket-
/// beskeder ellers kunne kalde SaveTierAsync direkte uden om et skjult knap.
/// </summary>
public partial class BasePriceMatrixEditor
{
    [Inject]
    private ISubscriptionTierAdminService AdminService { get; set; } = default!;

    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    [Parameter]
    public EventCallback<string> OnStatusMessage { get; set; }

    private IReadOnlyList<SubscriptionTierAdminDto> tiers = [];
    private readonly Dictionary<Guid, decimal> annualEdits = [];
    private readonly Dictionary<Guid, decimal> monthlyEdits = [];
    private readonly Dictionary<Guid, decimal> perpetualEdits = [];
    private string? errorMessage;
    private bool showStepUpModal;
    private Func<Task>? pendingAction;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        tiers = await AdminService.GetAllTiersAsync();

        annualEdits.Clear();
        monthlyEdits.Clear();
        perpetualEdits.Clear();

        foreach (var tier in tiers)
        {
            annualEdits[tier.Id] = tier.AnnualPrice;
            monthlyEdits[tier.Id] = tier.MonthlyPrice;
            perpetualEdits[tier.Id] = tier.PerpetualPrice;
        }
    }

    private async Task SaveTierAsync(Guid tierId)
    {
        if (!await HasRecentAuthenticationAsync())
        {
            pendingAction = () => SaveTierCoreAsync(tierId);
            showStepUpModal = true;
            return;
        }

        await SaveTierCoreAsync(tierId);
    }

    private async Task SaveTierCoreAsync(Guid tierId)
    {
        errorMessage = null;

        try
        {
            var update = new SubscriptionTierUpdateDto(
                tierId, annualEdits[tierId], monthlyEdits[tierId], perpetualEdits[tierId]);

            await AdminService.UpdateTierAsync(update);

            var savedName = tiers.Single(t => t.Id == tierId).Name;
            await LoadAsync();
            await OnStatusMessage.InvokeAsync($"Basispris for '{savedName}' er opdateret.");
        }
        catch (InvalidOperationException ex)
        {
            errorMessage = ex.Message;
        }
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
}