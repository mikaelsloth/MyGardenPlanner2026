namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Globalization;

/// <summary>
/// Viser den aktuelle brugers egne AuditLog-baggrundseksportjobs (oprettes via
/// AuditLogExportPanel). Downloadlinket peger direkte på det ejerskabsvaliderede
/// download-endpoint (se AuditLogEndpointsExtension) — filen hentes IKKE gennem denne
/// komponent. "Ryd notifikation" kalder derimod IAuditLogExportJobService direkte via DI,
/// da Blazor Server kører i samme proces og ikke behøver en HTTP-roundtrip for det.
/// </summary>
public partial class AuditLogExportJobsPanel
{
    private static readonly CultureInfo DanishCulture = new("da-DK");

    [Inject]
    private IAuditLogExportJobService JobService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private IReadOnlyList<AuditLogExportJobDto>? jobs;
    private string? currentUserId;

    protected override async Task OnInitializedAsync()
    {
        currentUserId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);
        await LoadAsync();
    }

    /// <summary>Kaldes af parent-siden, når et nyt job er oprettet, eller brugeren klikker "Opdater".</summary>
    public async Task RefreshAsync()
    {
        await LoadAsync();
        StateHasChanged();
    }

    private async Task LoadAsync()
    {
        jobs = currentUserId is null
            ? []
            : await JobService.GetJobsForUserAsync(currentUserId);
    }

    private async Task ClearNotificationAsync(Guid jobId)
    {
        if (currentUserId is null)
        {
            return;
        }

        await JobService.MarkNotificationSeenAsync(jobId, currentUserId);
        await LoadAsync();
    }

    private string DownloadUrl(AuditLogExportJobDto job) =>
        NavigationManager.ToAbsoluteUri($"/admin/audit-log/export-jobs/{job.Id}/download").ToString();

    private static bool CanDownload(AuditLogExportJobDto job) =>
        job.Status == AuditLogExportJobStatus.Completed
        && (job.ExpiresAtUtc is null || job.ExpiresAtUtc > DateTimeOffset.UtcNow);

    private static string StatusBadgeClass(AuditLogExportJobDto job) => job.Status switch
    {
        AuditLogExportJobStatus.Pending => "badge-accent",
        AuditLogExportJobStatus.Running => "badge-accent",
        AuditLogExportJobStatus.Completed => "badge-primary",
        AuditLogExportJobStatus.Failed => "badge-danger-soft",
        _ => string.Empty
    };

    private static string StatusLabel(AuditLogExportJobDto job) => job.Status switch
    {
        AuditLogExportJobStatus.Pending => "Afventer",
        AuditLogExportJobStatus.Running => "Kører",
        AuditLogExportJobStatus.Completed => "Klar",
        AuditLogExportJobStatus.Failed => "Fejlet",
        _ => job.Status.ToString()
    };
}