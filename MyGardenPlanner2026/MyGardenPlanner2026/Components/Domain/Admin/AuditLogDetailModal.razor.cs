namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Globalization;
using System.Text.Json;

/// <summary>
/// Viser Old/New JSON-værdier for én AuditLog-række. To-kolonne layout ≥ 940px,
/// sekventiel visning (Old øverst, New nedenunder) under 940px — se
/// AuditLogDetailModal.razor.css. Ugyldig/manglende JSON vises som rå tekst i
/// stedet for at fejle.
/// </summary>
public partial class AuditLogDetailModal
{
    private static readonly CultureInfo DanishCulture = new("da-DK");
    private static readonly JsonSerializerOptions PrettyPrintOptions = new() { WriteIndented = true };

    [Parameter]
    public AuditLogEntryDto? Entry { get; set; }

    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    private string FormattedOldValues => FormatJson(Entry?.OldValues);
    private string FormattedNewValues => FormatJson(Entry?.NewValues);

    private async Task HandleCloseAsync() => await OnClose.InvokeAsync();

    private static string FormatJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return "(ingen data)";
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document, PrettyPrintOptions);
        }
        catch (JsonException)
        {
            return json;
        }
    }
}