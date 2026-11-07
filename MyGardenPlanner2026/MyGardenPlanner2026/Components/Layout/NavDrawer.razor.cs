namespace MyGardenPlanner2026.Components.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MyGardenPlanner2026.Components.Foundation;

public partial class NavDrawer : IAsyncDisposable
{
    private ElementReference drawerElement;
    private FocusTrapScope? focusTrapScope;

    [Inject]
    private IFocusTrapService FocusTrapService { get; set; } = default!;

    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    private FocusTrapScope FocusTrap => focusTrapScope ??= new FocusTrapScope(FocusTrapService);

    protected override Task OnAfterRenderAsync(bool firstRender) =>
        FocusTrap.SyncAsync(IsOpen, drawerElement);

    private Task HandleCloseAsync() => OnClose.InvokeAsync();

    private async Task HandleKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            await HandleCloseAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (focusTrapScope is not null)
        {
            await focusTrapScope.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}