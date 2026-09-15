namespace MyGardenPlanner2026.Components.Interaction;

using Microsoft.AspNetCore.Components;

/// <summary>Modal bekræftelsesdialog til destruktive/kritiske handlinger (jf. Appendiks A, ConfirmDialog).</summary>
public partial class ConfirmDialog
{
    private readonly string titleId = $"confirm-dialog-title-{Guid.NewGuid():N}";

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
}