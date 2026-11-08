namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Ændring af admin-API rate limit-policyen er en følsom "core policy"-handling (§3.2)
/// og kræver step-up re-autentificering (se <see cref="PolicyEditorBase"/>).
/// </summary>
public partial class AdminApiRateLimitEditor
{
    [Inject]
    private IAdminApiRateLimitPolicyAdminService AdminService { get; set; } = default!;

    [Inject]
    private ILogger<AdminApiRateLimitEditor> Logger { get; set; } = default!;

    private AdminApiRateLimitPolicyDto? settings;
    private int permitEdit;
    private int windowEdit;
    private int segmentsEdit;

    protected override string SavedMessage => "Admin-API rate limit-policyen er opdateret og trådt i kraft.";

    [LoggerMessage(EventId = 1076, Level = LogLevel.Information, Message = "Opdatering af admin-API rate limit-policy afvist: {Reason}")]
    static partial void AdminApiRateLimitPolicySaveFailed(ILogger logger, string Reason);

    protected override void LogSaveRejected(string reason) => AdminApiRateLimitPolicySaveFailed(Logger, reason);

    protected override async Task LoadAsync()
    {
        settings = await AdminService.GetAsync();
        permitEdit = settings.PermitLimit;
        windowEdit = settings.WindowSeconds;
        segmentsEdit = settings.SegmentsPerWindow;
    }

    protected override Task UpdateAsync(string userId) =>
        AdminService.UpdateAsync(new AdminApiRateLimitPolicyDto(permitEdit, windowEdit, segmentsEdit), userId);
}