namespace MyGardenPlanner2026.Components.Domain.Gardens;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Globalization;

/// <summary>Rækkevisning af én haveinvitation med status og evt. tilbagekaldelses-handling (jf. Appendiks A, InvitationStatusCard).</summary>
public partial class InvitationStatusCard
{
    private static readonly CultureInfo DanishCulture = new("da-DK");

    [Parameter, EditorRequired] public GardenInvitationDto Invitation { get; set; } = default!;

    /// <summary>True hvis kalderen tillader tilbagekaldelse generelt (kombineres internt med pending-status).</summary>
    [Parameter] public bool AllowRevoke { get; set; }

    [Parameter] public EventCallback<Guid> OnRevoke { get; set; }

    private string RoleLabel => $"{Invitation.TargetLayer.ToDisplayName()} · {Invitation.TargetCategory.ToDisplayName()}";

    private bool IsPending => !Invitation.IsRevoked && !Invitation.IsAccepted && Invitation.ExpiresUtc >= DateTimeOffset.UtcNow;

    private bool CanRevoke => AllowRevoke && IsPending;

    private string StatusLabel => Invitation switch
    {
        { IsRevoked: true } => "Tilbagekaldt",
        { IsAccepted: true } => "Accepteret",
        _ when Invitation.ExpiresUtc < DateTimeOffset.UtcNow => "Udløbet",
        _ => "Afventer"
    };

    private string StatusBadgeClass => Invitation switch
    {
        { IsRevoked: true } => "badge-danger-soft",
        { IsAccepted: true } => "badge-primary",
        _ when Invitation.ExpiresUtc < DateTimeOffset.UtcNow => "badge-archived",
        _ => "badge-accent"
    };

    private async Task HandleRevokeAsync() => await OnRevoke.InvokeAsync(Invitation.Id);
}