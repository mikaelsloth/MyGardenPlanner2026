namespace MyGardenPlanner2026.Components.Domain.Gardens;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MyGardenPlanner2026.Core.Contracts.Onboarding;

/// <summary>
/// Viser invitationslinket til manuel kopiering (IdentityNoOpEmailSender er stadig en
/// no-op) samt en printbar version med QR-kode. QR-rendering sker klientside via
/// CDN-biblioteket "qrcode" (se App.razor) — samme mønster som Bootstrap/Bootstrap Icons.
/// </summary>
public partial class InvitationLinkCard : IAsyncDisposable
{
    private const string ModulePath = "./Components/Domain/Gardens/InvitationLinkCard.razor.js";

    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter, EditorRequired] public CreateInvitationResultDto Result { get; set; } = default!;
    [Parameter, EditorRequired] public string GardenName { get; set; } = default!;
    [Parameter] public string? InvitedEmail { get; set; }

    private ElementReference qrCanvasElement;
    private IJSObjectReference? module;
    private bool copied;

    private string InviteUrl => NavigationManager.ToAbsoluteUri($"/invite?token={Result.RawToken}").ToString();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            module = await JS.InvokeAsync<IJSObjectReference>("import", ModulePath);
        }

        if (module is not null)
        {
            await module.InvokeVoidAsync("renderQrCode", qrCanvasElement, InviteUrl);
        }
    }

    private async Task CopyLinkAsync()
    {
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", InviteUrl);
        copied = true;
    }

    private async Task PrintAsync()
    {
        if (module is not null)
        {
            await module.InvokeVoidAsync("printSection");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            try
            {
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit allerede afbrudt.
            }
        }

        GC.SuppressFinalize(this);
    }
}