namespace MyGardenPlanner2026.Core.Contracts.Account;

/// <summary>
/// Sletter en brugers data i forretningsdatabasen (haver, medlemskaber, entitlements,
/// checkout-kladder, invitationer og re-auth-fejlforsøg) før selve Identity-brugeren slettes.
/// </summary>
public interface IAccountDeletionService
{
    /// <summary>
    /// Sletter brugerens data. Er brugeren eneste ejer i en have med andre medlemmer, ændres
    /// intet, og resultatet er blokeret (ejerskabsoverdragelse mangler, se R10).
    /// </summary>
    Task<AccountDeletionResult> DeleteUserDataAsync(string userId, CancellationToken cancellationToken = default);
}