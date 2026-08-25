namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using System.Globalization;

public partial class BasePriceMatrixEditor
{
    [Inject]
    private ISubscriptionTierAdminService AdminService { get; set; } = default!;

    [Parameter]
    public EventCallback<string> OnStatusMessage { get; set; }

    private IReadOnlyList<SubscriptionTierAdminDto> tiers = [];
    private readonly Dictionary<Guid, decimal> annualEdits = [];
    private readonly Dictionary<Guid, decimal> monthlyEdits = [];
    private readonly Dictionary<Guid, decimal> perpetualEdits = [];
    private string? errorMessage;

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

    private static decimal ParseDecimal(object? value) =>
        decimal.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0m;
}