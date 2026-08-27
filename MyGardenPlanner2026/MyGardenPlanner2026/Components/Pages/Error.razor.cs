namespace MyGardenPlanner2026.Components.Pages;

using Microsoft.AspNetCore.Components;
using System.Diagnostics;

public partial class Error
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    private string? RequestId { get; set; }

    private bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    protected override void OnInitialized() =>
            RequestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
}
