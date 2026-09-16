namespace MyGardenPlanner2026.Components.Domain.Gardens;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Onboarding;

/// <summary>
/// Grupperer en haves invitationer i Afventende/Accepteret/Udløbet-Tilbagekaldt og
/// håndterer tilbagekaldelse via en delt ConfirmDialog (jf. Adgangsrettigheder.md,
/// HaveInvitation: "Alle" må CRUDA egne invitationer, kun Have Arkitekt\Administrator
/// må CRUDA ALLE — se CanManageAll).
/// </summary>
public partial class GardenInvitationList
{
    [Parameter, EditorRequired] public IReadOnlyList<GardenInvitationDto> Invitations { get; set; } = [];
    [Parameter] public bool CanManageAll { get; set; }
    [Parameter, EditorRequired] public string CurrentUserId { get; set; } = default!;
    [Parameter] public EventCallback<Guid> OnRevoke { get; set; }

    private Guid? pendingRevokeId;

    private IReadOnlyList<GardenInvitationDto> VisibleInvitations =>
        CanManageAll ? Invitations : [.. Invitations.Where(i => i.InvitedByUserId == CurrentUserId)];

    private IReadOnlyList<GardenInvitationDto> Pending =>
        [.. VisibleInvitations.Where(i => !i.IsRevoked && !i.IsAccepted && i.ExpiresUtc >= DateTimeOffset.UtcNow)];

    private IReadOnlyList<GardenInvitationDto> Accepted =>
        [.. VisibleInvitations.Where(i => i.IsAccepted)];

    private IReadOnlyList<GardenInvitationDto> ExpiredOrRevoked =>
        [.. VisibleInvitations.Where(i => !i.IsAccepted && (i.IsRevoked || i.ExpiresUtc < DateTimeOffset.UtcNow))];

    private void RequestRevoke(Guid invitationId) => pendingRevokeId = invitationId;

    private void CancelRevoke() => pendingRevokeId = null;

    private async Task ConfirmRevokeAsync()
    {
        if (pendingRevokeId is { } id)
        {
            pendingRevokeId = null;
            await OnRevoke.InvokeAsync(id);
        }
    }
}