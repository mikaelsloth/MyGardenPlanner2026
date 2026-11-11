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

    public static string UserNotLoadedDetail(string? userId) =>
        $"Brugeren med ID '{userId}' kunne ikke indlæses.";

    public static string UserNotLoaded(string? userId) =>
        $"Error: {UserNotLoadedDetail(userId)}";
    public const string FieldLengthBetween = "{0} skal være mellem {2} og {1} tegn.";

    public const string InvalidVerificationCode = "Error: Bekræftelseskoden er ugyldig.";

    public const string AuthenticatorVerified = "Din authenticator-app er bekræftet.";
    public const string AuthenticatorKeyWasReset = "Din authenticator-nøgle er nulstillet. Du skal konfigurere din authenticator-app med den nye nøgle.";
    public const string TwoFactorHasBeenDisabled = "2FA er deaktiveret. Du kan aktivere 2FA igen, når du opsætter en authenticator-app.";
    public const string NewRecoveryCodesGenerated = "Du har genereret nye gendannelseskoder.";
    public const string BrowserWasForgotten = "Denne browser er glemt. Næste gang du logger ind fra den, bliver du bedt om din 2FA-kode.";
    public const string PasskeyNameTooLong = "Passkey-navnet må højst være {1} tegn.";

    // Passkeys (fejl skal starte med "Error")
    public const string InvalidPasskeyId = "Error: Det angivne passkey-ID har et ugyldigt format.";
    public const string PasskeyBrowserDidNotProvide = "Error: Browseren leverede ikke en passkey.";
    public const string PasskeyLimitReached = "Error: Du har nået det maksimale antal tilladte passkeys.";
    public const string PasskeyNotAddedToAccount = "Error: Passkeyen kunne ikke føjes til din konto.";
    public const string PasskeyDeleteFailed = "Error: Passkeyen kunne ikke slettes.";
    public const string PasskeyNotFound = "Error: Den angivne passkey blev ikke fundet.";
    public const string PasskeyUpdateFailed = "Error: Passkeyen kunne ikke opdateres.";
    public const string PasskeyDeleted = "Passkeyen er slettet.";
    public const string PasskeyUpdated = "Passkeyen er opdateret.";

    // Eksterne logins
    public const string ExternalLoginRemoveFailed = "Error: Det eksterne login blev ikke fjernet.";
    public const string ExternalLoginInfoNotLoaded = "Error: Kunne ikke indlæse oplysninger om eksternt login.";
    public const string ExternalLoginNotAdded = "Error: Det eksterne login blev ikke tilføjet. Eksterne logins kan kun knyttes til én konto.";
    public const string ExternalLoginRemoved = "Det eksterne login er fjernet.";
    public const string ExternalLoginAdded = "Det eksterne login er tilføjet.";

    public static string PasskeyNotAdded(string? detail) =>
        $"Error: Passkeyen kunne ikke tilføjes: {detail}";

    public static string UnknownAction(string? action) =>
        $"Error: Ukendt handling '{action}'.";

    public const string InvalidPhone = "Angiv et gyldigt telefonnummer.";

    public const string PhoneNumberNotSaved = "Error: Telefonnummeret kunne ikke gemmes.";

    public const string ProfileUpdated = "Din profil er opdateret.";
    public const string PasswordsWasChanged = "Din adgangskode er ændret.";
    public const string PasswordsWasSet = "Din adgangskode er angivet.";
    public const string EmailUnchanged = "Din e-mail er uændret.";
    public const string EmailChangeConfirmationSent = "Bekræftelseslink til skift af e-mail er sendt. Tjek din e-mail.";
    public const string EmailVerificationSent = "Bekræftelses-e-mail er sendt. Tjek din e-mail.";

    public const string IncorrectPasswords = "Error: Adgangskoden er forkert.";
    public const string AccountNotDeleted = "Error: Din konto kunne ikke slettes. Prøv igen.";

    public static string AccountDeletionBlocked(IEnumerable<string> gardenNames) =>
        $"Error: Du er eneste ejer af haver, der deles med andre medlemmer: {string.Join(", ", gardenNames)}. " +
        "Ejerskabet skal overdrages, før din konto kan slettes.";
}