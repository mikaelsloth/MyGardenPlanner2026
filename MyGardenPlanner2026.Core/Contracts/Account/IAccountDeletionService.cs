namespace MyGardenPlanner2026.Core.Contracts.Account;

/// <summary>
/// Sletter en brugers data i forretningsdatabasen (haver, medlemskaber, entitlements,
/// checkout-kladder, invitationer og re-auth-fejlforsøg) før selve Identity-brugeren slettes.
/// </summary>
public interface IAccountDeletionService
{
    /// <summary>
    /// Sletter brugerens data. <paramref name="email"/> er brugerens nuværende e-mail: invitationer
    /// sendt TIL den slettes. Er brugeren eneste ejer i en have med andre medlemmer, ændres intet,
    /// og resultatet er blokeret (ejerskabsoverdragelse mangler, se R10).
    /// </summary>
    Task<AccountDeletionResult> DeleteUserDataAsync(
        string userId, string? email, CancellationToken cancellationToken = default);
}