namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Ændring af fejl-tracker-policyen er en følsom "core policy"-handling (§3.2) og
/// kræver step-up re-autentificering (se <see cref="PolicyEditorBase"/>).
/// </summary>
public partial class ReAuthFailureTrackerEditor
{
    [Inject]
    private IReAuthFailureTrackerPolicyAdminService AdminService { get; set; } = default!;

    [Inject]
    private ILogger<ReAuthFailureTrackerEditor> Logger { get; set; } = default!;

    private ReAuthFailureTrackerPolicyDto? settings;
    private int thresholdEdit;
    private int windowDaysEdit;

    protected override string SavedMessage => "Fejl-tracker-policyen er opdateret og trådt i kraft.";

    [LoggerMessage(EventId = 1075, Level = LogLevel.Information, Message = "Opdatering af fejl-tracker-policy afvist: {Reason}")]
    static partial void ReAuthFailureTrackerPolicySaveFailed(ILogger logger, string Reason);

    protected override void LogSaveRejected(string reason) => ReAuthFailureTrackerPolicySaveFailed(Logger, reason);

    protected override async Task LoadAsync()
    {
        settings = await AdminService.GetAsync();
        thresholdEdit = settings.Threshold;
        windowDaysEdit = settings.WindowDays;
    }

    protected override Task UpdateAsync(string userId) =>
        AdminService.UpdateAsync(new ReAuthFailureTrackerPolicyDto(thresholdEdit, windowDaysEdit), userId);
}