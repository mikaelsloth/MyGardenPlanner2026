namespace MyGardenPlanner2026.Infrastructure.Services.Onboarding;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Infrastructure.Data;

public sealed class GardenAccessQueryService(
    IDbContextFactory<PlannerDbContext> contextFactory) : IGardenAccessQueryService
{
    public async Task<GardenSummaryDto?> GetGardenSummaryAsync(
        Guid gardenId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var garden = await context.Gardens.SingleOrDefaultAsync(g => g.Id == gardenId, cancellationToken);
        return garden is null ? null : new GardenSummaryDto(garden.Id, garden.Name, garden.Archived);
    }

    public async Task<GardenMembershipDto?> GetMembershipAsync(
        Guid gardenId, string userId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var membership = await context.GardenMemberships
            .SingleOrDefaultAsync(m => m.GardenId == gardenId && m.UserId == userId, cancellationToken);

        return membership is null ? null : ToDto(membership);
    }

    public async Task<IReadOnlyList<GardenMembershipDto>> GetMembersAsync(
        Guid gardenId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var members = await context.GardenMemberships
            .Where(m => m.GardenId == gardenId)
            .OrderByDescending(m => m.IsOwner)
            .ToListAsync(cancellationToken);

        return [.. members.Select(ToDto)];
    }

    public async Task<IReadOnlyList<GardenInvitationDto>> GetInvitationsAsync(
        Guid gardenId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var invitations = await context.GardenInvitations
            .Where(i => i.GardenId == gardenId)
            .ToListAsync(cancellationToken);

        return [.. invitations.OrderByDescending(i => i.CreatedAtUtc).Select(ToDto)];
    }

    private static GardenMembershipDto ToDto(GardenMembership m) =>
        new(m.Id, m.GardenId, m.UserId, m.IsOwner, m.Layer, m.Category, m.JoinedAtUtc);

    private static GardenInvitationDto ToDto(GardenInvitation i) => new(
        i.Id, i.GardenId, i.InvitedByUserId, i.Email, i.TargetLayer, i.TargetCategory,
        i.IsFreeSlot, i.AllowSelfUpgrade, i.ExpiresUtc, i.IsAccepted, i.IsRevoked, i.CreatedAtUtc);
}