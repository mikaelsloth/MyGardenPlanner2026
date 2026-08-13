namespace MyGardenPlanner2026.Components.Domain.Marketing;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Entities.Common;

public partial class FeatureCard
{
    [Parameter, EditorRequired]
    public GardenAccessLevel AccessLevel { get; set; }

    [Parameter, EditorRequired]
    public string Title { get; set; } = default!;

    [Parameter, EditorRequired]
    public string Description { get; set; } = default!;
}