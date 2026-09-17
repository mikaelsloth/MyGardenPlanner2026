namespace MyGardenPlanner2026.Components.Domain.Subscriptions;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
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

    /// <summary>Låser og skjuler "Aktive haver"-feltet til denne værdi (fx 1 for en ny, endnu ikke-provisioneret have). Null = brugeren indtaster selv.</summary>
    [Parameter] public int? FixedActiveGardens { get; set; }

    /// <summary>Låser og skjuler "Arkiverede haver"-feltet til denne værdi. Null = brugeren indtaster selv.</summary>
    [Parameter] public int? FixedArchivedGardens { get; set; }

    /// <summary>Begrænser de valgbare Lag-niveauer (fx til en invitations MaxAllowedLayer-loft). Null = alle niveauer.</summary>
    [Parameter] public IReadOnlyList<GardenAccessLevel>? AllowedLevels { get; set; }

    /// <summary>Begrænser de valgbare adgangskategorier. Null = alle kategorier.</summary>
    [Parameter] public IReadOnlyList<AccessCategory>? AllowedCategories { get; set; }

    [Parameter] public GardenAccessLevel? InitialLevel { get; set; }
    [Parameter] public AccessCategory? InitialCategory { get; set; }
    [Parameter] public BillingCycle? InitialBillingCycle { get; set; }

    /// <summary>Når sat, vises en ekstra "Fortsæt"-knap, der beregner prisen og kalder OnContinue med den valgte konfiguration.</summary>
    [Parameter] public string? ContinueButtonLabel { get; set; }

    [Parameter] public EventCallback<PricingSelectionDto> OnContinue { get; set; }

    private IReadOnlyList<SubscriptionAddOnDto> addOns = [];
    private readonly Dictionary<Guid, int> addOnQuantities = [];

    private GardenAccessLevel selectedLevel;
    private AccessCategory selectedCategory;
    private BillingCycle selectedCycle;
    private PricingCalculationResultDto? result;
    private string? errorMessage;

    private IReadOnlyList<GardenAccessLevel> Levels => AllowedLevels ?? [.. Enum.GetValues<GardenAccessLevel>()];
    private IReadOnlyList<AccessCategory> Categories => AllowedCategories ?? [.. Enum.GetValues<AccessCategory>()];

    private int EffectiveActiveGardens { get => FixedActiveGardens ?? field; set; } = 1;
    private int EffectiveArchivedGardens { get => FixedArchivedGardens ?? field; set; }

    protected override async Task OnInitializedAsync()
    {
        selectedLevel = InitialLevel ?? GardenAccessLevel.HaveArkitekt;
        selectedCategory = InitialCategory ?? AccessCategory.Editor;
        selectedCycle = InitialBillingCycle ?? BillingCycle.Annual;

        if (AllowedLevels is { Count: > 0 } && !AllowedLevels.Contains(selectedLevel))
        {
            selectedLevel = AllowedLevels[0];
        }

        if (AllowedCategories is { Count: > 0 } && !AllowedCategories.Contains(selectedCategory))
        {
            selectedCategory = AllowedCategories[0];
        }

        addOns = await AddOnService.GetAllAddOnsAsync();

        foreach (var addOn in addOns)
        {
            addOnQuantities[addOn.Id] = 0;
        }
    }

    private int GetQuantity(Guid addOnId) => addOnQuantities.GetValueOrDefault(addOnId);

    private void SetQuantity(Guid addOnId, int quantity) =>
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
                EffectiveActiveGardens,
                EffectiveArchivedGardens,
                addOnQuantities);

            result = await CalculatorService.CalculateAsync(request);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task ContinueAsync()
    {
        await CalculateAsync();

        if (result is not null)
        {
            await OnContinue.InvokeAsync(new PricingSelectionDto(
                selectedLevel, selectedCategory, selectedCycle,
                new Dictionary<Guid, int>(addOnQuantities), result));
        }
    }
}