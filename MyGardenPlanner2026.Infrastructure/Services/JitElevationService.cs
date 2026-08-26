namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// JIT-eskaleringsmotor. Håndhæver:
/// - RoleName skal eksistere i Identity (RoleManager.RoleExistsAsync).
/// - RequestedHours skal være 1-8.
/// - Peer approval / dual-custody: godkender/afviser må ikke være ansøgeren selv.
/// Skriver via IAdminDbContextFactory, da RoleElevationRequests ligger i admin-schema.
/// </summary>
public sealed class JitElevationService(
    IAdminDbContextFactory contextFactory,
    RoleManager<IdentityRole> roleManager) : IJitElevationService
{
    public const int MinRequestedHours = 1;
    public const int MaxRequestedHours = 8;

    public async Task<RoleElevationRequestDto> RequestElevationAsync(
        string userId, string roleName, int hours, string reason, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (hours is < MinRequestedHours or > MaxRequestedHours)
        {
            throw new ArgumentOutOfRangeException(
                nameof(hours), hours, $"RequestedHours skal være mellem {MinRequestedHours} og {MaxRequestedHours}.");
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
            RequestedHours = hours,
            Reason = reason
        };

        context.RoleElevationRequests.Add(request);
        await context.SaveChangesAsync(cancellationToken);

        return ToDto(request);
    }

    public async Task<RoleElevationRequestDto> ApproveElevationAsync(
        string approverUserId, Guid requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(approverUserId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var request = await LoadPendingRequestAsync(context, approverUserId, requestId, "godkendes", cancellationToken);

        var now = DateTimeOffset.UtcNow;
        request.Status = RoleElevationStatus.Approved;
        request.ApproverUserId = approverUserId;
        request.ValidFromUtc = now;
        request.ValidToUtc = now.AddHours(request.RequestedHours);

        await context.SaveChangesAsync(cancellationToken);

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

        // For ens adfærd på tværs af providers hentes de godkendte anmodninger for
        // bruger+rolle (translatérbar WHERE), og tidsvinduet tjekkes client-side.
        var approvedRequests = await context.RoleElevationRequests
            .Where(r => r.RequesterUserId == userId
                && r.RoleName == roleName
                && r.Status == RoleElevationStatus.Approved)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        return approvedRequests.Any(r => r.ValidFromUtc <= now && r.ValidToUtc >= now);
    }

    private static async Task<RoleElevationRequest> LoadPendingRequestAsync(
        PlannerDbContext context, string approverUserId, Guid requestId, string action, CancellationToken cancellationToken)
    {
        var request = await context.RoleElevationRequests
            .SingleOrDefaultAsync(r => r.Id == requestId, cancellationToken)
            ?? throw new InvalidOperationException($"Ingen elevations-anmodning fundet med Id {requestId}.");

        if (request.RequesterUserId == approverUserId)
        {
            throw new InvalidOperationException(
                $"Anmodningen kan ikke {action} af ansøgeren selv (dual-custody / peer approval).");
        }

        if (request.Status != RoleElevationStatus.Pending)
        {
            throw new InvalidOperationException($"Anmodningen kan ikke {action} fra status '{request.Status}'.");
        }

        return request;
    }

    private static RoleElevationRequestDto ToDto(RoleElevationRequest request) => new(
        request.Id, request.RequesterUserId, request.ApproverUserId, request.RoleName, request.Status,
        request.Reason, request.RequestedHours, request.ValidFromUtc, request.ValidToUtc, request.CreatedAtUtc);
}