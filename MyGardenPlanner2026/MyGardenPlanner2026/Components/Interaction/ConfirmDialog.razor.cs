namespace MyGardenPlanner2026.Components.Interaction;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MyGardenPlanner2026.Components.Foundation;

/// <summary>Modal bekræftelsesdialog til destruktive/kritiske handlinger (jf. Appendiks A, ConfirmDialog).</summary>
public partial class ConfirmDialog : IAsyncDisposable
{
    private readonly string titleId = $"confirm-dialog-title-{Guid.NewGuid():N}";

    private ElementReference dialogElement;
    private bool wasOpen;
    private IAsyncDisposable? focusTrap;

    [Inject]
    private IFocusTrapService FocusTrapService { get; set; } = default!;
    [Parameter, EditorRequired] public bool IsOpen { get; set; }
    [Parameter, EditorRequired] public string Title { get; set; } = default!;
    [Parameter, EditorRequired] public string Message { get; set; } = default!;
    [Parameter] public string? ImpactNote { get; set; }
    [Parameter] public string ConfirmLabel { get; set; } = "Bekræft";
    [Parameter] public string CancelLabel { get; set; } = "Annullér";
    [Parameter] public bool IsDangerous { get; set; } = true;
    [Parameter] public bool IsSubmitting { get; set; }
    [Parameter] public bool CloseOnBackdropClick { get; set; } = true;
    [Parameter] public EventCallback OnConfirm { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    private async Task HandleConfirmAsync()
    {
        if (!IsSubmitting)
        {
            await OnConfirm.InvokeAsync();
        }
    }

    private async Task HandleCancelAsync()
    {
        if (!IsSubmitting)
        {
            await OnCancel.InvokeAsync();
        }
    }

    private async Task HandleBackdropClickAsync()
    {
        if (CloseOnBackdropClick && !IsSubmitting)
        {
            await OnCancel.InvokeAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen == wasOpen)
        {
            return;
        }

        wasOpen = IsOpen;

        if (IsOpen)
        {
            var trap = await FocusTrapService.ActivateAsync(dialogElement);

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

    private async Task HandleKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            await HandleCancelAsync();
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

    public async ValueTask DisposeAsync()
    {
        await ReleaseFocusTrapAsync();
        GC.SuppressFinalize(this);
    }
}