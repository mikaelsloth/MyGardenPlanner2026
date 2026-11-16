namespace MyGardenPlanner2026.Core.Contracts.Account;

/// <summary>
/// Brugerens valg for en blokerende have: overdrag ejerskabet (modtageren får IsOwner = true og
/// hæves til Administrator; Layer er uændret, og der oprettes intet entitlement) eller slet
/// haven for alle (soft-slettet og gendannelig i opbevaringsperioden, se GardenRestoreOptions).
/// </summary>
public sealed record GardenOwnershipResolution
{
    public GardenResolutionKind Kind { get; private init; }

    /// <summary>Kun sat ved <see cref="GardenResolutionKind.TransferOwnership"/>.</summary>
    public string? NewOwnerUserId { get; private init; }

    public static GardenOwnershipResolution TransferTo(string newOwnerUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newOwnerUserId);
        return new() { Kind = GardenResolutionKind.TransferOwnership, NewOwnerUserId = newOwnerUserId };
    }

    public static GardenOwnershipResolution DeleteForAll() =>
        new() { Kind = GardenResolutionKind.DeleteForAll };
}