namespace MyGardenPlanner2026.Components.Domain.Subscriptions;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using System.Globalization;

public partial class PricingCalculator
{
    private static readonly CultureInfo DanishCulture = new("da-DK");

    [Inject]
    private ISubscriptionAddOnService AddOnService { get; set; } = default!;

    [Inject]
    private IPricingCalculatorService CalculatorService { get; set; } = default!;

    private IReadOnlyList<SubscriptionAddOnDto> addOns = [];
    private readonly Dictionary<int, int> addOnQuantities = [];

    private GardenAccessLevel selectedLevel = GardenAccessLevel.HaveArkitekt;
    private AccessCategory selectedCategory = AccessCategory.Editor;
    private BillingCycle selectedCycle = BillingCycle.Annual;
    private int activeGardens = 1;
    private int archivedGardens;

    private PricingCalculationResultDto? result;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        addOns = await AddOnService.GetAllAddOnsAsync();

        foreach (var addOn in addOns)
        {
            addOnQuantities[addOn.Id] = 0;
        }
    }

    private int GetQuantity(int addOnId) => addOnQuantities.GetValueOrDefault(addOnId);

    private void SetQuantity(int addOnId, int quantity) =>
        addOnQuantities[addOnId] = Math.Max(0, quantity);

    private async Task CalculateAsync()
    {
        errorMessage = null;
        result = null;

        try
        {
            var request = new PricingCalculationRequestDto(
                selectedLevel,
                selectedCategory,
                selectedCycle,
                activeGardens,
                archivedGardens,
                addOnQuantities);

            result = await CalculatorService.CalculateAsync(request);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            errorMessage = ex.Message;
        }
    }
}