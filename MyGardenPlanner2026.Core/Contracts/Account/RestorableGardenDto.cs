namespace MyGardenPlanner2026.Core.Contracts.Account;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// En slettet have, som den indloggede bruger kan gendanne, indtil <paramref name="ExpiresUtc"/>
/// (se PendingGardenRestore). <paramref name="Layer"/>/<paramref name="Category"/> er havens
/// tidligere ejerniveau, som et nyt køb skal dække (se GardenRestoreCoverage).
/// </summary>
public sealed record RestorableGardenDto(
    Guid PendingRestoreId,
    Guid GardenId,
    string Name,
    DateTimeOffset DeletedAtUtc,
    DateTimeOffset ExpiresUtc,
    GardenAccessLevel Layer,
    AccessCategory Category);