namespace MyGardenPlanner2026.Components.Account;

/// <summary>
/// Danske brugerrettede tekster for kontosiderne (R8).
/// Beskeder, der skal vises som fejl i <c>StatusMessage</c>, SKAL starte med det engelske
/// præfiks "Error" (StatusMessage.razor.cs bruger StartsWith("Error"), se D1 = A).
/// </summary>
internal static class AccountMessages
{
    // Valideringsbeskeder (bruges i ErrorMessage; {0} = Display-navn, {1} = max, {2} = min)
    public const string FieldRequired = "Feltet '{0}' er påkrævet.";
    public const string InvalidEmail = "Angiv en gyldig e-mailadresse.";
    public const string PasswordsLength = "Adgangskoden skal være mindst {2} og højst {1} tegn.";
    public const string PasswordsDoNotMatch = "Adgangskoden og bekræftelsen er ikke ens.";

    // Statusbeskeder (skal starte med "Error")
    public const string InvalidLoginAttempt = "Error: Ugyldigt loginforsøg.";

    public static string UserNotLoaded(string? userId) =>
        $"Error: Brugeren med ID '{userId}' kunne ikke indlæses.";

    public const string FieldLengthBetween = "{0} skal være mellem {2} og {1} tegn.";

    public const string InvalidVerificationCode = "Error: Bekræftelseskoden er ugyldig.";

    public const string AuthenticatorVerified = "Din authenticator-app er bekræftet.";
    public const string AuthenticatorKeyWasReset = "Din authenticator-nøgle er nulstillet. Du skal konfigurere din authenticator-app med den nye nøgle.";
    public const string TwoFactorHasBeenDisabled = "2FA er deaktiveret. Du kan aktivere 2FA igen, når du opsætter en authenticator-app.";
    public const string NewRecoveryCodesGenerated = "Du har genereret nye gendannelseskoder.";
    public const string BrowserWasForgotten = "Denne browser er glemt. Næste gang du logger ind fra den, bliver du bedt om din 2FA-kode.";
}