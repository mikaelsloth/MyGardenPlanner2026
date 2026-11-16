namespace MyGardenPlanner2026.Core.Contracts.Account;

/// <summary>
/// Brugerens valg for en blokerende have: overdrag ejerskabet til <paramref name="NewOwnerUserId"/>.
/// Modtageren får IsOwner = true og hæves til Administrator (Layer er uændret, og der oprettes
/// intet entitlement). Udvides i R10.1b med "slet for alle".
/// </summary>
public sealed record GardenOwnershipResolution(string NewOwnerUserId);