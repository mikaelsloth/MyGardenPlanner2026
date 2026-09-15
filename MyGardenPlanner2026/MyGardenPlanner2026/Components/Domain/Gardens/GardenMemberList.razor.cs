namespace MyGardenPlanner2026.Components.Domain.Gardens;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using System.Globalization;

/// <summary>OBS: viser UserId rå — e-mail-opslag mod Identity er ikke koblet på endnu (se antagelse #12).</summary>
public partial class GardenMemberList
{
    private static readonly CultureInfo DanishCulture = new("da-DK");

    [Parameter, EditorRequired] public IReadOnlyList<GardenMembershipDto> Members { get; set; } = [];
}