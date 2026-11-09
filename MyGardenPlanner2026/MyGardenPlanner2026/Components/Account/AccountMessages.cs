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
}