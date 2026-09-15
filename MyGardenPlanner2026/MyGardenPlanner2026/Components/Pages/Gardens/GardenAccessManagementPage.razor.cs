namespace MyGardenPlanner2026.Components.Pages.Gardens;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;

public partial class GardenAccessManagementPage
{
    [Parameter] public Guid GardenId { get; set; }

    [Inject] private IGardenAccessQueryService QueryService { get; set; } = default!;
    [Inject] private IOnboardingService OnboardingService { get; set; } = default!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private bool isLoading = true;
    private bool isAuthorized;
    private string? currentUserId;
    private GardenMembershipDto? currentMembership;
    private GardenSummaryDto? garden;
    private IReadOnlyList<GardenMembershipDto> members = [];
    private IReadOnlyList<GardenInvitationDto> invitations = [];
    private FreeInvitationQuotaDto freeQuota = new(0, 0, 0);
    private string? statusMessage;
    private CreateInvitationResultDto? lastResult;
    private string? lastInvitedEmail;

    private bool CanManageAllInvitations =>
        currentMembership is { Layer: GardenAccessLevel.HaveArkitekt, Category: AccessCategory.Administrator };

    protected override async Task OnInitializedAsync()
    {
        currentUserId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);

        if (currentUserId is null || AuthenticationStateTask is null)
        {
            isAuthorized = false;
            isLoading = false;
            return;
        }

        var authState = await AuthenticationStateTask;
        var authResult = await AuthorizationService.AuthorizeAsync(
            authState.User, GardenId, AuthorizationServicesExtensions.RequireGardenMemberPolicy);

        isAuthorized = authResult.Succeeded;

        if (!isAuthorized)
        {
            isLoading = false;
            return;
        }

        garden = await QueryService.GetGardenSummaryAsync(GardenId);
        currentMembership = await QueryService.GetMembershipAsync(GardenId, currentUserId);
        await ReloadDataAsync();

        isLoading = false;
    }

    private async Task ReloadDataAsync()
    {
        members = await QueryService.GetMembersAsync(GardenId);
        invitations = await QueryService.GetInvitationsAsync(GardenId);
        freeQuota = await OnboardingService.GetFreeInvitationQuotaAsync(GardenId, currentUserId!);
    }

    private async Task HandleInvitationCreatedAsync((CreateInvitationResultDto Result, string Email) created)
    {
        lastResult = created.Result;
        lastInvitedEmail = created.Email;
        statusMessage = $"Invitation sendt til {created.Email}.";
        await ReloadDataAsync();
    }

    private async Task HandleRevokeAsync(Guid invitationId)
    {
        await OnboardingService.RevokeInvitationAsync(invitationId, currentUserId!);
        statusMessage = "Invitationen er tilbagekaldt.";
        await ReloadDataAsync();
    }
}