namespace MyGardenPlanner2026.Components.Domain.Gardens;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>Viser en haves medlemsrolle som dansk klartekst (jf. Appendiks A, RoleBadge).</summary>
public partial class RoleBadge
{
    [Parameter] public AccessCategory Category { get; set; }
    [Parameter] public bool IsOwner { get; set; }

    private string Label => IsOwner ? "Ejer" : Category.ToDisplayName();

    private string VariantClass => IsOwner || Category == AccessCategory.Administrator
        ? "badge-primary"
        : Category == AccessCategory.Editor
            ? "badge-accent"
            : string.Empty;
}