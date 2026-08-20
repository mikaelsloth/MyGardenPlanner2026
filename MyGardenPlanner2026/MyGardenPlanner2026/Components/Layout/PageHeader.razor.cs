namespace MyGardenPlanner2026.Components.Layout;

using Microsoft.AspNetCore.Components;

public partial class PageHeader
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = default!;

    [Parameter]
    public string? Intro { get; set; }

    [Parameter]
    public string? LastUpdated { get; set; }
}