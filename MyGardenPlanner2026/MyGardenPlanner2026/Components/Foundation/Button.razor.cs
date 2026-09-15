namespace MyGardenPlanner2026.Components.Foundation;

using Microsoft.AspNetCore.Components;

public partial class Button
{
    public enum ButtonVariant { Primary, Secondary, Accent, Danger, Ghost, Icon }
    public enum ButtonSize { Default, Small, Large }

    [Parameter] public ButtonVariant Variant { get; set; } = ButtonVariant.Primary;
    [Parameter] public ButtonSize Size { get; set; } = ButtonSize.Default;
    [Parameter] public string Type { get; set; } = "button";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string VariantClass => $"btn-{Variant.ToString().ToLowerInvariant()}";

    private string SizeClass => Size switch
    {
        ButtonSize.Small => "btn-sm",
        ButtonSize.Large => "btn-lg",
        _ => string.Empty
    };

    private async Task HandleClickAsync()
    {
        if (!Disabled && !Loading)
        {
            await OnClick.InvokeAsync();
        }
    }
}