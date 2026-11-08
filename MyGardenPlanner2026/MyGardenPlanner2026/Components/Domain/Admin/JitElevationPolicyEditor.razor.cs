namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Ændring af JIT-eskaleringspolicyen er en følsom "core policy"-handling (§3.2) og
/// kræver step-up re-autentificering (se <see cref="PolicyEditorBase"/>).
/// </summary>
public partial class JitElevationPolicyEditor
{
    [Inject]
    private IJitElevationPolicyAdminService AdminService { get; set; } = default!;

    [Inject]
    private ILogger<JitElevationPolicyEditor> Logger { get; set; } = default!;

    private JitElevationPolicyDto? settings;
    private int minEdit;
    private int maxEdit;
    private int sweepEdit;
    private int pendingExpiryEdit;

    protected override string SavedMessage => "JIT-eskaleringspolicyen er opdateret og trådt i kraft.";

    [LoggerMessage(EventId = 1073, Level = LogLevel.Information, Message = "Opdatering af JIT-eskaleringspolicy afvist: {Reason}")]
    static partial void JitPolicySaveFailed(ILogger logger, string Reason);

    protected override void LogSaveRejected(string reason) => JitPolicySaveFailed(Logger, reason);

    protected override async Task LoadAsync()
    {
        settings = await AdminService.GetAsync();
        minEdit = settings.MinRequestedMinutes;
        maxEdit = settings.MaxRequestedMinutes;
        sweepEdit = settings.SweepIntervalMinutes;
        pendingExpiryEdit = settings.PendingExpiryHours;
    }

    protected override Task UpdateAsync(string userId) =>
        AdminService.UpdateAsync(new JitElevationPolicyDto(minEdit, maxEdit, sweepEdit, pendingExpiryEdit), userId);
}