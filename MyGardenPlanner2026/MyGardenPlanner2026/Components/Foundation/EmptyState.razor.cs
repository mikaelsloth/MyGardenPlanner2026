namespace MyGardenPlanner2026.Components.Foundation;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Generisk tom-tilstand til sider, sektioner og søgeresultater uden data — svarer på
/// "hvad er tomt, hvorfor, og hvad er næste skridt" (jf. Appendiks A, EmptyState).
/// </summary>
public partial class EmptyState
{
    public enum EmptyStateVariant { FirstUse, Context, Filtered, Search, Restricted, Processing, Error }

    [Parameter] public EmptyStateVariant Variant { get; set; } = EmptyStateVariant.FirstUse;
    [Parameter, EditorRequired] public string IconClass { get; set; } = default!;
    [Parameter, EditorRequired] public string Title { get; set; } = default!;
    [Parameter] public string? Description { get; set; }
    [Parameter] public bool Inline { get; set; }

    /// <summary>Overskriftsniveau (1-3). Default 2, da komponenten typisk indsættes i en side der allerede har sin egen h1.</summary>
    [Parameter] public int HeadingLevel { get; set; } = 2;

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? Actions { get; set; }

    private string VariantClass => Variant switch
    {
        EmptyStateVariant.Filtered => "empty-filtered",
        EmptyStateVariant.Search => "empty-search",
        EmptyStateVariant.Error => "empty-error",
        EmptyStateVariant.Restricted => "empty-restricted",
        EmptyStateVariant.Processing => "empty-processing",
        _ => string.Empty
    };
}