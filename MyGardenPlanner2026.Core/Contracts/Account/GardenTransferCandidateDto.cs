namespace MyGardenPlanner2026.Core.Contracts.Account;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Et aktivt medlem af en have, som kan overtage ejerskabet, når den eneste ejer sletter
/// sin konto (se R10.1).
/// </summary>
public sealed record GardenTransferCandidateDto(string UserId, GardenAccessLevel Layer, AccessCategory Category);