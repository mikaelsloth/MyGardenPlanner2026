namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Globalization;

public partial class AuditLogResultsTable
{
    private static readonly CultureInfo DanishCulture = new("da-DK");

    [Parameter]
    public AuditLogQueryResultDto? Result { get; set; }

    [Parameter]
    public EventCallback<int> OnPageChanged { get; set; }

    [Parameter]
    public EventCallback<AuditLogEntryDto> OnViewDetails { get; set; }

    private int TotalPages => Result is null || Result.PageSize <= 0
        ? 1
        : Math.Max(1, (int)Math.Ceiling(Result.TotalCount / (double)Result.PageSize));

    private bool HasPreviousPage => Result is not null && Result.PageNumber > 1;
    private bool HasNextPage => Result is not null && Result.PageNumber < TotalPages;

    private async Task GoToPreviousPageAsync()
    {
        if (HasPreviousPage)
        {
            await OnPageChanged.InvokeAsync(Result!.PageNumber - 1);
        }
    }

    private async Task GoToNextPageAsync()
    {
        if (HasNextPage)
        {
            await OnPageChanged.InvokeAsync(Result!.PageNumber + 1);
        }
    }

    private static string ActionBadgeClass(AuditAction action) => action switch
    {
        AuditAction.Create => "badge-primary",
        AuditAction.Update => "badge-accent",
        AuditAction.Delete => "badge-danger-soft",
        _ => string.Empty
    };

    private static string ActionLabel(AuditAction action) => action switch
    {
        AuditAction.Create => "Oprettet",
        AuditAction.Update => "Opdateret",
        AuditAction.Delete => "Slettet",
        _ => action.ToString()
    };
}