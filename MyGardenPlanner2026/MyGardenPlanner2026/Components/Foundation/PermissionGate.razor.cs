namespace MyGardenPlanner2026.Components.Foundation;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Logisk wrapper-komponent til betinget adgang/visning (jf. Appendiks A, PermissionGate).
/// Disable-tilstand kan ikke sætte disabled-attributten på det indpakkede, opake indhold —
/// i stedet ombrydes det visuelt (pointer-events/opacity), samme effekt som .btn:disabled.
/// ReadOnly-tilstand (jf. Appendiks A) er ikke implementeret her — intet aktuelt behov.
/// </summary>
public partial class PermissionGate
{
    public enum PermissionGateMode { Hide, Disable }

    [Parameter, EditorRequired] public bool IsAuthorized { get; set; }
    [Parameter] public PermissionGateMode Mode { get; set; } = PermissionGateMode.Disable;
    [Parameter] public string? HintText { get; set; }
    [Parameter, EditorRequired] public RenderFragment ChildContent { get; set; } = default!;
}