namespace MyGardenPlanner2026.Components.Domain.Subscriptions;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Layer1;
using System.Globalization;

/// <summary>
/// Simuleret betalingsside — ingen rigtig betalingsudbyder er integreret endnu. Viser en
/// opsummering af den beregnede pris og lader den kaldende komponent afgøre, hvad
/// "betaling gennemført" faktisk udløser (fx ProvisionPaidGardenAsync). Genanvendes i
/// OnboardingCheckoutPage (trin 3) og senere i invitations-accept med selv-opgradering.
/// </summary>
public partial class PaymentMockPage
{
    private static readonly CultureInfo DanishCulture = new("da-DK");

    [Parameter, EditorRequired] public string GardenName { get; set; } = default!;
    [Parameter, EditorRequired] public PricingCalculationResultDto PriceResult { get; set; } = default!;
    [Parameter, EditorRequired] public BillingCycle BillingCycle { get; set; }
    [Parameter] public string ConfirmButtonLabel { get; set; } = "Gennemfør betaling og opret have";
    [Parameter] public bool IsProcessing { get; set; }
    [Parameter] public string? ErrorMessage { get; set; }
    [Parameter] public EventCallback OnConfirmPayment { get; set; }

    private string CycleLabel => BillingCycle switch
    {
        BillingCycle.Annual => "Årligt",
        BillingCycle.Monthly => "Månedligt",
        BillingCycle.Perpetual => "Engangsbeløb",
        _ => BillingCycle.ToString()
    };
}