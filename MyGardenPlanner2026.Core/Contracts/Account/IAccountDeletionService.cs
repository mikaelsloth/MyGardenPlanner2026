namespace MyGardenPlanner2026.Core.Contracts.Account;

/// <summary>
/// Sletter en brugers data i forretningsdatabasen (haver, medlemskaber, entitlements,
/// checkout-kladder, invitationer og re-auth-fejlforsøg) før selve Identity-brugeren slettes.
/// </summary>
public interface IAccountDeletionService
{
    /// <summary>
    /// Returnerer de haver, hvor brugeren er eneste ejer, og der er andre medlemmer (dvs. haver,
    /// der blokerer sletning), med mulige modtagere af ejerskabet. Ændrer intet.
    /// </summary>
    Task<IReadOnlyList<BlockingGardenDto>> GetBlockingGardensAsync(
        string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sletter brugerens data uden valg for blokerende haver. Er brugeren eneste ejer i en have
    /// med andre medlemmer, ændres intet, og resultatet er blokeret.
    /// <paramref name="email"/> er brugerens nuværende e-mail: invitationer sendt TIL den slettes.
    /// </summary>
    Task<AccountDeletionResult> DeleteUserDataAsync(
        string userId, string? email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Som overloaden uden valg, men blokerende haver kan løses ved at overdrage ejerskabet
    /// (nøgle = GardenId). Er en blokerende have uden gyldigt valg (modtager skal være aktivt
    /// medlem, ikke brugeren selv), ændres intet, og resultatet er blokeret med de uløste haver.
    /// Valg for haver, der ikke blokerer, ignoreres.
    /// </summary>
    Task<AccountDeletionResult> DeleteUserDataAsync(
        string userId, string? email, IReadOnlyDictionary<Guid, GardenOwnershipResolution> resolutions,
        CancellationToken cancellationToken = default);
}