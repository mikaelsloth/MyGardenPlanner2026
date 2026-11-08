namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Ændring af login rate limit-policyen er en følsom "core policy"-handling (§3.2) og
/// kræver step-up re-autentificering (se <see cref="PolicyEditorBase"/>). Selve forbruget
/// (ReloadableLoginRateLimiter) reagerer på ændringer via IOptionsMonitor.
/// </summary>
public partial class LoginRateLimitEditor
{
    [Inject]
    private ILoginRateLimitPolicyAdminService AdminService { get; set; } = default!;

    [Inject]
    private ILogger<LoginRateLimitEditor> Logger { get; set; } = default!;

    private LoginRateLimitPolicyDto? settings;
    private int permitEdit;
    private int windowEdit;

    protected override string SavedMessage => "Login rate limit-policyen er opdateret og trådt i kraft.";

    [LoggerMessage(EventId = 1077, Level = LogLevel.Information, Message = "Opdatering af login rate limit-policy afvist: {Reason}")]
    static partial void LoginRateLimitPolicySaveFailed(ILogger logger, string Reason);

    protected override void LogSaveRejected(string reason) => LoginRateLimitPolicySaveFailed(Logger, reason);

    protected override async Task LoadAsync()
    {
        settings = await AdminService.GetAsync();
        permitEdit = settings.PermitLimit;
        windowEdit = settings.WindowSeconds;
    }

    protected override Task UpdateAsync(string userId) =>
        AdminService.UpdateAsync(new LoginRateLimitPolicyDto(permitEdit, windowEdit), userId);
}