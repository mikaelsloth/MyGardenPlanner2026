namespace MyGardenPlanner2026.Components.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MyGardenPlanner2026.Components.Foundation;

public partial class NavDrawer : IAsyncDisposable
{
    private ElementReference drawerElement;
    private bool wasOpen;
    private IAsyncDisposable? focusTrap;

    [Inject]
    private IFocusTrapService FocusTrapService { get; set; } = default!;

    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen == wasOpen)
        {
            return;
        }

        wasOpen = IsOpen;

        if (IsOpen)
        {
            var trap = await FocusTrapService.ActivateAsync(drawerElement);

            // Lukket igen, mens aktiveringen afventede.
            if (IsOpen)
            {
                focusTrap = trap;
            }
            else
            {
                await trap.DisposeAsync();
            }
        }
        else
        {
            await ReleaseFocusTrapAsync();
        }
    }

    private async Task ReleaseFocusTrapAsync()
    {
        var trap = focusTrap;
        focusTrap = null;

        if (trap is not null)
        {
            await trap.DisposeAsync();
        }
    }

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
        await ReleaseFocusTrapAsync();
        GC.SuppressFinalize(this);
    }
}