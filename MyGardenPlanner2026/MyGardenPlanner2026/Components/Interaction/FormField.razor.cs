namespace MyGardenPlanner2026.Components.Interaction;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Tre-lags feltwrapper: label (+ required/optional-indikator), control-slot og
/// reserveret plads til fejl-/hjælpetekst (jf. Appendiks A, FormField).
/// </summary>
public partial class FormField
{
    [Parameter, EditorRequired] public string For { get; set; } = default!;
    [Parameter, EditorRequired] public string Label { get; set; } = default!;
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool ShowOptionalLabel { get; set; }
    [Parameter] public string? HelpText { get; set; }
    [Parameter] public string? ErrorMessage { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    private string ErrorId => $"{For}-error";
    private string HelpId => $"{For}-help";
}