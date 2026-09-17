namespace MyGardenPlanner2026.Components.Foundation;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Tværgående, vedvarende advarsels-/informationslinje øverst på siden eller under
/// hoved-headeren (jf. Appendiks A, StatusBanner/GlobalBanner) — modsat StatusMessage,
/// der er kontekstuel feedback inde i en sektion/formular. Parent-styret: komponenten
/// skjuler ikke sig selv ved Dismiss, den rejser blot OnDismiss.
/// </summary>
public partial class StatusBanner
{
    public enum StatusBannerVariant { Info, Warning, Danger }

    [Parameter] public StatusBannerVariant Variant { get; set; } = StatusBannerVariant.Info;
    [Parameter] public string? IconClass { get; set; }
    [Parameter] public bool Dismissible { get; set; }
    [Parameter] public EventCallback OnDismiss { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string VariantClass => Variant.ToString().ToLowerInvariant();

    private string Role => Variant == StatusBannerVariant.Danger ? "alert" : "status";

    private async Task HandleDismissAsync() => await OnDismiss.InvokeAsync();
}