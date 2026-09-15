namespace MyGardenPlanner2026.Components.Layout;

using Microsoft.AspNetCore.Components;

/// <summary>Visuel opdeling af en formularsektion (jf. Appendiks A, FormSection).</summary>
public partial class FormSection
{
    [Parameter] public string? Title { get; set; }
    [Parameter] public string? Intro { get; set; }
    [Parameter] public bool HasError { get; set; }

    /// <summary>Antal kolonner i form-grid'et under sektionen: 1 (default), 2, 3 eller 4.</summary>
    [Parameter] public int Columns { get; set; } = 1;

    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string GridClass => Columns switch
    {
        2 => "form-grid-2",
        3 => "form-grid-3",
        4 => "form-grid-4",
        _ => string.Empty
    };
}