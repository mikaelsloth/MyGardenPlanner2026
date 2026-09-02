namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Læser/opdaterer den runtime-styrede rate limit-policy for login-endpoints (§4.1).
/// Selve GlobalLimiter-forbruget er stadig hardkodet indtil PR3.
/// </summary>
public sealed class LoginRateLimitPolicyAdminService(
    IAdminDbContextFactory contextFactory,
    ISecurityPolicyChangeSignal changeSignal,
    ISecurityAlertService securityAlertService) : ILoginRateLimitPolicyAdminService
{
    public async Task<LoginRateLimitPolicyDto> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return ToDto(await LoadAsync(context, cancellationToken));
    }

    public async Task<LoginRateLimitPolicyDto> UpdateAsync(
        LoginRateLimitPolicyDto update, string updatedByUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);
        ArgumentException.ThrowIfNullOrWhiteSpace(updatedByUserId);

        if (update.PermitLimit < 1 || update.WindowSeconds < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(update), "PermitLimit og WindowSeconds skal være positive.");
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await LoadAsync(context, cancellationToken);

        settings.PermitLimit = update.PermitLimit;
        settings.WindowSeconds = update.WindowSeconds;
        await context.SaveChangesAsync(cancellationToken);

        changeSignal.TriggerChange<LoginRateLimitOptions>();
        await securityAlertService.AlertPolicyChangedAsync(updatedByUserId, "LoginRateLimitPolicy", cancellationToken);

        return ToDto(settings);
    }

    private static async Task<LoginRateLimitSettings> LoadAsync(
        PlannerDbContext context, CancellationToken cancellationToken) =>
        await context.LoginRateLimitSettings
            .SingleOrDefaultAsync(s => s.Id == LoginRateLimitSettings.SingletonId, cancellationToken)
            ?? throw new InvalidOperationException("Login rate limit-policy er ikke seedet.");

    private static LoginRateLimitPolicyDto ToDto(LoginRateLimitSettings settings) =>
        new(settings.PermitLimit, settings.WindowSeconds);
}