namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Rent søgekriterie-input. Emitterer et fuldt, klar-til-brug AuditLogFilterDto
/// (PageNumber nulstillet til 1) ved klik på "Søg" — siden er ansvarlig for at
/// gemme resultatet som ny gældende filter, udføre søgningen og persistere
/// præferencen. EntityName-dropdown populeres dynamisk fra EntityNameOptions
/// (hentet af siden via IAuditLogQueryService.GetDistinctEntityNamesAsync).
/// </summary>
public partial class AuditLogFilterBar
{
    private static readonly AuditAction[] ActionOptions = Enum.GetValues<AuditAction>();
    private static readonly int[] PageSizeOptions = [10, 25, 50, 100];

    [Parameter, EditorRequired]
    public AuditLogFilterDto InitialFilter { get; set; } = default!;

    [Parameter, EditorRequired]
    public IReadOnlyList<string> EntityNameOptions { get; set; } = [];

    [Parameter]
    public EventCallback<AuditLogFilterDto> OnSearch { get; set; }

    private bool hasSyncedInitialFilter;
    private string entityName = string.Empty;
    private string entityId = string.Empty;
    private string userId = string.Empty;
    private string userEmail = string.Empty;
    private AuditAction? action;
    private DateTime? fromDate;
    private DateTime? toDate;
    private int pageSize = 25;

    protected override void OnParametersSet()
    {
        // Synkroniseres kun ÉN gang fra InitialFilter (typisk brugerens gemte
        // præference ved sideindlæsning) — efterfølgende parent-rerenders må
        // ikke overskrive brugerens igangværende redigering af felterne.
        if (hasSyncedInitialFilter)
        {
            return;
        }

        entityName = InitialFilter.EntityName ?? string.Empty;
        entityId = InitialFilter.EntityId ?? string.Empty;
        userId = InitialFilter.UserId ?? string.Empty;
        userEmail = InitialFilter.UserEmail ?? string.Empty;
        action = InitialFilter.Action;
        fromDate = InitialFilter.FromUtc?.UtcDateTime;
        toDate = InitialFilter.ToUtc?.UtcDateTime;
        pageSize = InitialFilter.PageSize;
        hasSyncedInitialFilter = true;
    }

    private Task HandleSearchAsync()
    {
        var filter = new AuditLogFilterDto(
            NullIfWhiteSpace(entityName),
            NullIfWhiteSpace(entityId),
            NullIfWhiteSpace(userId),
            NullIfWhiteSpace(userEmail),
            action,
            ToUtcStartOfDay(fromDate),
            ToUtcEndOfDay(toDate),
            PageNumber: 1,
            PageSize: pageSize,
            SortDescending: true);

        return OnSearch.InvokeAsync(filter);
    }

    /// <summary>Valgt dato som 00:00:00 UTC (Fra er inklusiv).</summary>
    private static DateTimeOffset? ToUtcStartOfDay(DateTime? date) =>
        date is { } d
            ? new DateTimeOffset(DateTime.SpecifyKind(d.Date, DateTimeKind.Utc))
            : null;

    /// <summary>Valgt dato som 23:59:59,9999999 UTC, så hele Til-dagen er med (filteret bruger &lt;=).</summary>
    private static DateTimeOffset? ToUtcEndOfDay(DateTime? date) =>
        ToUtcStartOfDay(date)?.AddDays(1).AddTicks(-1);

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private static string ActionLabel(AuditAction value) => value switch
    {
        AuditAction.Create => "Oprettet",
        AuditAction.Update => "Opdateret",
        AuditAction.Delete => "Slettet",
        _ => value.ToString()
    };
}