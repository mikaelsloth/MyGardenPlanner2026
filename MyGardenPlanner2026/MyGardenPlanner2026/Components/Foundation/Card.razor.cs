namespace MyGardenPlanner2026.Components.Foundation;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Generisk container til browsing og overblik over entiteter (jf. Appendiks A, Card).
/// Understøtter varianterne Default/Entity/Attention/Archived — Action/Compact/Restricted
/// kræver en anden intern grid-struktur og er bevidst udeladt indtil et konkret behov opstår.
/// </summary>
public partial class Card
{
    public enum CardVariant { Default, Entity, Attention, Archived }

    [Parameter] public CardVariant Variant { get; set; } = CardVariant.Default;
    [Parameter] public string? Title { get; set; }
    [Parameter] public string? Description { get; set; }
    [Parameter] public string DataDensity { get; set; } = "default";
    [Parameter] public RenderFragment? HeaderExtra { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? Actions { get; set; }

    private string VariantClass => Variant switch
    {
        CardVariant.Entity => "card-entity",
        CardVariant.Attention => "card-entity card-attention",
        CardVariant.Archived => "card-entity card-archived",
        _ => string.Empty
    };
}