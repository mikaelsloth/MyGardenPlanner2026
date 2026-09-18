namespace MyGardenPlanner2026.Components.Foundation;

using Microsoft.AspNetCore.Components;

/// <summary>Kompakt forklarende tekst under deaktiverede knapper/felter (jf. Appendiks A, PermissionHint).</summary>
public partial class PermissionHint
{
    [Parameter, EditorRequired] public RenderFragment ChildContent { get; set; } = default!;
}