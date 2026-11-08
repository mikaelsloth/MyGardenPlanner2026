namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Ændring af step-up re-autentificeringspolicyen er en følsom "core policy"-handling
/// (§3.2) og kræver step-up re-autentificering (se <see cref="PolicyEditorBase"/>).
/// </summary>
public partial class ReAuthenticationPolicyEditor
{
    private const int MaxAgeMinutes = IReAuthenticationPolicyAdminService.MaxAllowedAgeMinutes;

    [Inject]
    private IReAuthenticationPolicyAdminService AdminService { get; set; } = default!;

    [Inject]
    private ILogger<ReAuthenticationPolicyEditor> Logger { get; set; } = default!;

    private ReAuthenticationPolicyDto? settings;
    private int maxAgeEdit;

    protected override string SavedMessage => "Step-up re-auth-policyen er opdateret og trådt i kraft.";

    [LoggerMessage(EventId = 1074, Level = LogLevel.Information, Message = "Opdatering af step-up re-auth-policy afvist: {Reason}")]
    static partial void ReAuthPolicySaveFailed(ILogger logger, string Reason);

    protected override void LogSaveRejected(string reason) => ReAuthPolicySaveFailed(Logger, reason);

    protected override async Task LoadAsync()
    {
        settings = await AdminService.GetAsync();
        maxAgeEdit = settings.MaxAgeMinutes;
    }

    protected override Task UpdateAsync(string userId) =>
        AdminService.UpdateAsync(new ReAuthenticationPolicyDto(maxAgeEdit), userId);
}