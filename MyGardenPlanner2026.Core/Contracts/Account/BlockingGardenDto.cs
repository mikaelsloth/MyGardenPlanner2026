namespace MyGardenPlanner2026.Core.Contracts.Account;

/// <summary>
/// En have, der blokerer kontosletning, fordi brugeren er eneste ejer, og haven har andre
/// medlemmer. <paramref name="Candidates"/> er de aktive medlemmer, ejerskabet kan overdrages
/// til (sorteret efter højeste kategori, derefter tilmeldingstidspunkt).
/// </summary>
public sealed record BlockingGardenDto(
    Guid GardenId, string Name, IReadOnlyList<GardenTransferCandidateDto> Candidates);