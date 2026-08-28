namespace MyGardenPlanner2026.Components.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

public partial class NavDrawer : IAsyncDisposable
{
    private const string ModulePath = "./Components/Layout/NavDrawer.razor.js";

    private ElementReference drawerElement;
    private bool wasOpen;
    private IJSObjectReference? module;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            module = await JS.InvokeAsync<IJSObjectReference>("import", ModulePath);
        }

        if (IsOpen && !wasOpen)
        {
            await ActivateFocusTrapAsync();
        }
        else if (!IsOpen && wasOpen)
        {
            await DeactivateFocusTrapAsync();
        }

        wasOpen = IsOpen;
    }

    private async Task ActivateFocusTrapAsync()
    {
        if (module is not null)
        {
            await module.InvokeVoidAsync("activate", drawerElement);
        }
    }

    private async Task DeactivateFocusTrapAsync()
    {
        if (module is not null)
        {
            await module.InvokeVoidAsync("deactivate");
        }
    }

    private async Task HandleClose() => await OnClose.InvokeAsync();

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            await HandleClose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (module is null)
        {
            return;
        }

        try
        {
            if (IsOpen)
            {
                await module.InvokeVoidAsync("deactivate");
            }

            await module.DisposeAsync();
            GC.SuppressFinalize(this);
        }
        catch (JSDisconnectedException)
        {
            // Circuit already disconnected — intet at rydde op på klientsiden.
        }
    }
}