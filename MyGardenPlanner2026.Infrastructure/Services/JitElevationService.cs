namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// JIT-eskaleringsmotor. Håndhæver:
/// - RoleName skal eksistere i Identity (RoleManager.RoleExistsAsync).
/// - RequestedMinutes skal ligge inden for policyens Min/MaxRequestedMinutes
///   (konfigurerbar via JitElevationPolicyOptions, sektion "JitElevationPolicy").
/// - Peer approval / dual-custody: godkender/afviser må ikke være ansøgeren selv.
/// Skriver via IAdminDbContextFactory, da RoleElevationRequests ligger i admin-schema.
/// </summary>
public sealed class JitElevationService(
    IAdminDbContextFactory contextFactory,
    RoleManager<IdentityRole> roleManager,
    IOptionsMonitor<JitElevationPolicyOptions> policyOptionsMonitor,
    TimeProvider timeProvider,
    ISecurityAlertService securityAlertService) : IJitElevationService
{
    public async Task<RoleElevationRequestDto> RequestElevationAsync(
        string userId, string roleName, int minutes, string reason, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var policy = policyOptionsMonitor.CurrentValue;

        if (policy.MinRequestedMinutes > policy.MaxRequestedMinutes)
        {
            throw new InvalidOperationException(
                $"Ugyldig JitElevationPolicy: MinRequestedMinutes ({policy.MinRequestedMinutes}) " +
                $"er større end MaxRequestedMinutes ({policy.MaxRequestedMinutes}).");
        }

        if (minutes < policy.MinRequestedMinutes || minutes > policy.MaxRequestedMinutes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minutes), minutes,
                $"RequestedMinutes skal være mellem {policy.MinRequestedMinutes} og {policy.MaxRequestedMinutes}.");
        }

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            throw new InvalidOperationException($"Rollen '{roleName}' findes ikke.");
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var request = new RoleElevationRequest
        {
            RequesterUserId = userId,
            RoleName = roleName,
            RequestedMinutes = minutes,
            Reason = reason
        };

        await context.RoleElevationRequests.AddAsync(request, CancellationToken.None);
        await context.SaveChangesAsync(cancellationToken);

        return ToDto(request);
    }

    public async Task<RoleElevationRequestDto> ApproveElevationAsync(
        string approverUserId, Guid requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(approverUserId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var request = await LoadPendingRequestAsync(context, approverUserId, requestId, "godkendes", cancellationToken);

        var now = timeProvider.GetUtcNow();
        request.Status = RoleElevationStatus.Approved;
        request.ApproverUserId = approverUserId;
        request.ValidFromUtc = now;
        request.ValidToUtc = now.AddMinutes(request.RequestedMinutes);

        await context.SaveChangesAsync(cancellationToken);

        request.ValidFromUtc = now;
        request.ValidToUtc = now.AddMinutes(request.RequestedMinutes);

        await context.SaveChangesAsync(cancellationToken);

        await securityAlertService.AlertJitRequestedAsync(request.RequesterUserId, request.RoleName, cancellationToken);

        return ToDto(request);
    }

    public async Task<RoleElevationRequestDto> RejectElevationAsync(
        string approverUserId, Guid requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(approverUserId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var request = await LoadPendingRequestAsync(context, approverUserId, requestId, "afvises", cancellationToken);

        request.Status = RoleElevationStatus.Rejected;
        request.ApproverUserId = approverUserId;

        await context.SaveChangesAsync(cancellationToken);

        return ToDto(request);
    }

    public async Task<bool> HasActiveElevationAsync(
        string userId, string roleName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var approvedRequests = await context.RoleElevationRequests
            .Where(r => r.RequesterUserId == userId
                && r.RoleName == roleName
                && r.Status == RoleElevationStatus.Approved)
            .ToListAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();
        return approvedRequests.Any(r => r.ValidFromUtc <= now && r.ValidToUtc >= now);
    }

    private static async Task<RoleElevationRequest> LoadPendingRequestAsync(
        PlannerDbContext context, string approverUserId, Guid requestId, string action, CancellationToken cancellationToken)
    {
        var request = await context.RoleElevationRequests
            .SingleOrDefaultAsync(r => r.Id == requestId, cancellationToken)
            ?? throw new InvalidOperationException($"Ingen elevations-anmodning fundet med Id {requestId}.");

        return request.RequesterUserId == approverUserId
            ? throw new InvalidOperationException(
                $"Anmodningen kan ikke {action} af ansøgeren selv (dual-custody / peer approval).")
            : request.Status != RoleElevationStatus.Pending
            ? throw new InvalidOperationException($"Anmodningen kan ikke {action} fra status '{request.Status}'.")
            : request;
    }

    private static RoleElevationRequestDto ToDto(RoleElevationRequest request) => new(
        request.Id, request.RequesterUserId, request.ApproverUserId, request.RoleName, request.Status,
        request.Reason, request.RequestedMinutes, request.ValidFromUtc, request.ValidToUtc, request.CreatedAtUtc);
}