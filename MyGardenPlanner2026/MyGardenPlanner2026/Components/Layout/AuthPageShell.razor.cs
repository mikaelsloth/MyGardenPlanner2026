namespace MyGardenPlanner2026.Components.Layout;

using Microsoft.AspNetCore.Components;

public partial class AuthPageShell
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = default!;

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;
}