namespace MyGardenPlanner2026.Components.Account.Shared;

using Microsoft.AspNetCore.Components;

public partial class ShowRecoveryCodes
{
    [Parameter]
    public string[] RecoveryCodes { get; set; } = [];

    [Parameter]
    public string? StatusMessage { get; set; }
}