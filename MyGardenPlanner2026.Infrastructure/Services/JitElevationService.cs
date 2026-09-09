namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities;
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
/// Alle anmodninger, godkendelser, afvisninger og valideringsfejl logges som Information
/// til brug for driftsmæssig sporing (§4.2-relateret, om end ikke en sikkerhedsalarm i sig selv).
/// </summary>
public sealed partial class JitElevationService(
    IAdminDbContextFactory contextFactory,
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IOptionsMonitor<JitElevationPolicyOptions> policyOptionsMonitor,
    TimeProvider timeProvider,
    ISecurityAlertService securityAlertService,
    ILogger<JitElevationService> logger) : IJitElevationService
{
    [LoggerMessage(EventId = 1027, Level = LogLevel.Information, Message = "Bruger '{UserId}' anmodede om JIT-eskalering til rollen '{RoleName}' i {Minutes} minutter.")]
    static partial void ElevationRequested(ILogger logger, string UserId, string RoleName, int Minutes);

    [LoggerMessage(EventId = 1028, Level = LogLevel.Information, Message = "Ugyldig JitElevationPolicy ved anmodning fra bruger '{UserId}': MinRequestedMinutes ({MinMinutes}) er større end MaxRequestedMinutes ({MaxMinutes}).")]
    static partial void ElevationRequestPolicyMisconfigured(ILogger logger, string UserId, int MinMinutes, int MaxMinutes);

    [LoggerMessage(EventId = 1029, Level = LogLevel.Information, Message = "Bruger '{UserId}' anmodede om {Minutes} minutter, hvilket ligger uden for policy-grænsen [{MinMinutes}-{MaxMinutes}].")]
    static partial void ElevationRequestMinutesOutOfRange(ILogger logger, string UserId, int Minutes, int MinMinutes, int MaxMinutes);

    [LoggerMessage(EventId = 1030, Level = LogLevel.Information, Message = "Bruger '{UserId}' anmodede om ukendt rolle '{RoleName}'.")]
    static partial void ElevationRequestUnknownRole(ILogger logger, string UserId, string RoleName);

    [LoggerMessage(EventId = 1031, Level = LogLevel.Information, Message = "Anmodning '{RequestId}' om rollen '{RoleName}' blev godkendt af bruger '{ApproverUserId}'.")]
    static partial void ElevationApproved(ILogger logger, Guid RequestId, string RoleName, string ApproverUserId);

    [LoggerMessage(EventId = 1032, Level = LogLevel.Information, Message = "Anmodning '{RequestId}' om rollen '{RoleName}' blev afvist af bruger '{ApproverUserId}'.")]
    static partial void ElevationRejected(ILogger logger, Guid RequestId, string RoleName, string ApproverUserId);

    [LoggerMessage(EventId = 1033, Level = LogLevel.Information, Message = "Bruger '{ApproverUserId}' forsøgte at {Action} egen anmodning '{RequestId}' (dual-custody afvist).")]
    static partial void ElevationSelfActionRejected(ILogger logger, string ApproverUserId, string Action, Guid RequestId);

    [LoggerMessage(EventId = 1034, Level = LogLevel.Information, Message = "Anmodning '{RequestId}' kunne ikke {Action} fra status '{Status}'.")]
    static partial void ElevationInvalidStatusForAction(ILogger logger, Guid RequestId, string Action, RoleElevationStatus Status);

    public async Task<RoleElevationRequestDto> RequestElevationAsync(
        string userId, string roleName, int minutes, string reason, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var policy = policyOptionsMonitor.CurrentValue;

        if (policy.MinRequestedMinutes > policy.MaxRequestedMinutes)
        {
            ElevationRequestPolicyMisconfigured(logger, userId, policy.MinRequestedMinutes, policy.MaxRequestedMinutes);
            throw new InvalidOperationException(
                $"Ugyldig JitElevationPolicy: MinRequestedMinutes ({policy.MinRequestedMinutes}) " +
                $"er større end MaxRequestedMinutes ({policy.MaxRequestedMinutes}).");
        }

        if (minutes < policy.MinRequestedMinutes || minutes > policy.MaxRequestedMinutes)
        {
            ElevationRequestMinutesOutOfRange(logger, userId, minutes, policy.MinRequestedMinutes, policy.MaxRequestedMinutes);
            throw new ArgumentOutOfRangeException(
                nameof(minutes), minutes,
                $"RequestedMinutes skal være mellem {policy.MinRequestedMinutes} og {policy.MaxRequestedMinutes}.");
        }

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            ElevationRequestUnknownRole(logger, userId, roleName);
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

        ElevationRequested(logger, userId, roleName, minutes);

        return ToDto(request);
    }

    public async Task<RoleElevationRequestDto> ApproveElevationAsync(
        string approverUserId, Guid requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(approverUserId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var request = await LoadPendingRequestAsync(context, logger, approverUserId, requestId, "godkendes", cancellationToken);

        var now = timeProvider.GetUtcNow();
        request.Status = RoleElevationStatus.Approved;
        request.ApproverUserId = approverUserId;
        request.ValidFromUtc = now;
        request.ValidToUtc = now.AddMinutes(request.RequestedMinutes);

        await context.SaveChangesAsync(cancellationToken);

        ElevationApproved(logger, request.Id, request.RoleName, approverUserId);

        await securityAlertService.AlertJitRequestedAsync(request.RequesterUserId, request.RoleName, cancellationToken);

        return ToDto(request);
    }

    public async Task<RoleElevationRequestDto> RejectElevationAsync(
        string approverUserId, Guid requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(approverUserId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var request = await LoadPendingRequestAsync(context, logger, approverUserId, requestId, "afvises", cancellationToken);

        request.Status = RoleElevationStatus.Rejected;
        request.ApproverUserId = approverUserId;

        await context.SaveChangesAsync(cancellationToken);

        ElevationRejected(logger, request.Id, request.RoleName, approverUserId);

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

    public async Task<IReadOnlyList<RoleElevationRequestDto>> GetRequestsForUserAsync(
            string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // OBS: sortering på CreatedAtUtc (DateTimeOffset) sker client-side — SQLite kan
        // ikke oversætte ORDER BY på DateTimeOffset til SQL (se memory-noter).
        var requests = await context.RoleElevationRequests
            .Where(r => r.RequesterUserId == userId)
            .ToListAsync(cancellationToken);

        return [.. requests
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(ToDto)];
    }

    public async Task<IReadOnlyList<RoleElevationRequestDto>> GetPendingRequestsForApprovalAsync(
        string approverUserId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(approverUserId);

        var approverUser = await userManager.FindByIdAsync(approverUserId);
        if (approverUser is null)
        {
            return [];
        }

        var approverRoles = await userManager.GetRolesAsync(approverUser);
        if (approverRoles.Count == 0)
        {
            return [];
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // OBS: sortering på CreatedAtUtc (DateTimeOffset) sker client-side — se note ovenfor.
        var requests = await context.RoleElevationRequests
            .Where(r => r.Status == RoleElevationStatus.Pending
                && r.RequesterUserId != approverUserId
                && approverRoles.Contains(r.RoleName))
            .ToListAsync(cancellationToken);

        return [.. requests
            .OrderBy(r => r.CreatedAtUtc)
            .Select(ToDto)];
    }

    private static async Task<RoleElevationRequest> LoadPendingRequestAsync(
        PlannerDbContext context, ILogger logger, string approverUserId, Guid requestId, string action, CancellationToken cancellationToken)
    {
        var request = await context.RoleElevationRequests
            .SingleOrDefaultAsync(r => r.Id == requestId, cancellationToken)
            ?? throw new InvalidOperationException($"Ingen elevations-anmodning fundet med Id {requestId}.");

        if (request.RequesterUserId == approverUserId)
        {
            ElevationSelfActionRejected(logger, approverUserId, action, requestId);
            throw new InvalidOperationException(
                $"Anmodningen kan ikke {action} af ansøgeren selv (dual-custody / peer approval).");
        }

        if (request.Status != RoleElevationStatus.Pending)
        {
            ElevationInvalidStatusForAction(logger, requestId, action, request.Status);
            throw new InvalidOperationException($"Anmodningen kan ikke {action} fra status '{request.Status}'.");
        }

        return request;
    }

    private static RoleElevationRequestDto ToDto(RoleElevationRequest request) => new(
        request.Id, request.RequesterUserId, request.ApproverUserId, request.RoleName, request.Status,
        request.Reason, request.RequestedMinutes, request.ValidFromUtc, request.ValidToUtc, request.CreatedAtUtc);
}