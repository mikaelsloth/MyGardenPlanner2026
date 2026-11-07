namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MyGardenPlanner2026.Components.Foundation;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Text.Json;

/// <summary>
/// Viser Old/New JSON-værdier for én AuditLog-række. To-kolonne layout ≥ 940px,
/// sekventiel visning (Old øverst, New nedenunder) under 940px — se
/// AuditLogDetailModal.razor.css. Ugyldig/manglende JSON vises som rå tekst i
/// stedet for at fejle.
/// </summary>
public partial class AuditLogDetailModal : IAsyncDisposable
{
    private static readonly JsonSerializerOptions PrettyPrintOptions = new() { WriteIndented = true };

    private ElementReference dialogElement;
    private FocusTrapScope? focusTrapScope;

    private FocusTrapScope FocusTrap => focusTrapScope ??= new FocusTrapScope(FocusTrapService);

    [Inject]
    private IFocusTrapService FocusTrapService { get; set; } = default!;

    [Parameter]
    public AuditLogEntryDto? Entry { get; set; }

    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    private string FormattedOldValues => FormatJson(Entry?.OldValues);
    private string FormattedNewValues => FormatJson(Entry?.NewValues);

    private Task HandleCloseAsync() => OnClose.InvokeAsync();

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

    // Dialogen renderes kun, når begge er sat (jf. @if i markup).
    private bool IsRendered => IsOpen && Entry is not null;

    protected override Task OnAfterRenderAsync(bool firstRender) =>
        FocusTrap.SyncAsync(IsRendered, dialogElement);

    public async ValueTask DisposeAsync()
    {
        if (focusTrapScope is not null)
        {
            await focusTrapScope.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            await HandleCloseAsync();
        }
    }
}