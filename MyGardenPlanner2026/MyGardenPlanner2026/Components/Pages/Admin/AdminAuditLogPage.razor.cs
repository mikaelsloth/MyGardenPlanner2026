namespace MyGardenPlanner2026.Components.Pages.Admin;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Core.Contracts.Admin;

public partial class AdminAuditLogPage
{
    [Inject]
    private IAuditLogQueryService QueryService { get; set; } = default!;

    [Inject]
    private IAuditLogViewerPreferenceService PreferenceService { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private AuditLogFilterDto currentFilter = new(null, null, null, null, null, null, null);
    private IReadOnlyList<string> entityNameOptions = [];
    private AuditLogQueryResultDto? result;
    private AuditLogEntryDto? selectedEntryForDetail;
    private bool showDetailModal;

    protected override async Task OnInitializedAsync()
    {
        entityNameOptions = await QueryService.GetDistinctEntityNamesAsync();

        var userId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);
        if (userId is not null)
        {
            var preference = await PreferenceService.GetAsync(userId);
            currentFilter = (preference.LastFilter ?? currentFilter) with
            {
                PageNumber = 1,
                PageSize = preference.PageSize
            };
        }

        await RunSearchAsync();
    }

    private async Task RunSearchAsync() =>
        result = await QueryService.SearchAsync(currentFilter);

    private async Task HandleSearchAsync(AuditLogFilterDto filter)
    {
        currentFilter = filter;
        await RunSearchAsync();
        await SavePreferenceAsync();
    }

    private async Task HandlePageChangedAsync(int pageNumber)
    {
        currentFilter = currentFilter with { PageNumber = pageNumber };
        await RunSearchAsync();
    }

    private async Task SavePreferenceAsync()
    {
        var userId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);
        if (userId is not null)
        {
            await PreferenceService.SaveAsync(
                userId, new AuditLogViewerPreferenceDto(currentFilter.PageSize, currentFilter));
        }
    }

    private void HandleViewDetails(AuditLogEntryDto entry)
    {
        selectedEntryForDetail = entry;
        showDetailModal = true;
    }

    private void HandleCloseDetail() => showDetailModal = false;
}