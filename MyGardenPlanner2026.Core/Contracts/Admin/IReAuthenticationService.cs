namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Sporer tidspunktet for brugerens seneste adgangskode- eller TOTP-verifikation
/// inden for den aktuelle Blazor Server-circuit ("session"). Bruges til at afgøre,
/// om step-up re-autentificering er nødvendig før følsomme handlinger (se PR2).
/// </summary>
public interface IReAuthenticationService
{
    /// <summary>Tidspunkt for seneste verificerede adgangskode/TOTP, eller null hvis endnu ikke sat i denne circuit.</summary>
    DateTimeOffset? LastAuthTimestampUtc { get; }

    /// <summary>Markerer, at brugeren netop har bekræftet sin adgangskode og/eller TOTP-kode.</summary>
    void MarkReAuthenticated();

    /// <summary>True hvis seneste verifikation er yngre end eller lig med <paramref name="maxAge"/>.</summary>
    bool IsReAuthValid(TimeSpan maxAge);
}