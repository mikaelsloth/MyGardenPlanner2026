namespace MyGardenPlanner2026.Core.Contracts.Account;

/// <summary>
/// En slettet have, som den indloggede bruger kan gendanne, indtil <paramref name="ExpiresUtc"/>
/// (se PendingGardenRestore).
/// </summary>
public sealed record RestorableGardenDto(
    Guid PendingRestoreId, Guid GardenId, string Name, DateTimeOffset DeletedAtUtc, DateTimeOffset ExpiresUtc);