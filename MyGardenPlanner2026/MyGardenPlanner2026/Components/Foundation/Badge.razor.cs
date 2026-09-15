namespace MyGardenPlanner2026.Components.Foundation;

using Microsoft.AspNetCore.Components;

public partial class Badge
{
    public enum BadgeVariant { Neutral, Primary, Accent, DangerSoft }

    [Parameter] public BadgeVariant Variant { get; set; } = BadgeVariant.Neutral;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string VariantClass => Variant switch
    {
        BadgeVariant.Primary => "badge-primary",
        BadgeVariant.Accent => "badge-accent",
        BadgeVariant.DangerSoft => "badge-danger-soft",
        _ => string.Empty
    };
}