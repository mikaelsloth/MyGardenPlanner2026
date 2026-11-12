namespace MyGardenPlanner2026.Components.Account;

using Microsoft.AspNetCore.Identity;
using System.Globalization;

/// <summary>
/// Danske brugerrettede fejltekster for ASP.NET Core Identity (R8.5).
/// Fejlkoderne (<see cref="IdentityError.Code"/>) er identiske med standardens;
/// kun <see cref="IdentityError.Description"/> er oversat.
/// </summary>
public sealed class DanishIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() =>
        Create(nameof(DefaultError), "Der opstod en ukendt fejl.");

    public override IdentityError ConcurrencyFailure() =>
        Create(nameof(ConcurrencyFailure), "Dataene er ændret af en anden. Prøv igen.");

    public override IdentityError PasswordMismatch() =>
        Create(nameof(PasswordMismatch), "Adgangskoden er forkert.");

    public override IdentityError InvalidToken() =>
        Create(nameof(InvalidToken), "Ugyldigt eller udløbet token.");

    public override IdentityError RecoveryCodeRedemptionFailed() =>
        Create(nameof(RecoveryCodeRedemptionFailed), "Gendannelseskoden kunne ikke indløses.");

    public override IdentityError LoginAlreadyAssociated() =>
        Create(nameof(LoginAlreadyAssociated), "Der findes allerede en bruger med dette eksterne login.");

    public override IdentityError InvalidUserName(string? userName) =>
        Create(nameof(InvalidUserName), $"Brugernavnet '{userName}' er ugyldigt. Brug kun bogstaver og tal.");

    public override IdentityError InvalidEmail(string? email) =>
        Create(nameof(InvalidEmail), $"E-mailadressen '{email}' er ugyldig.");

    public override IdentityError DuplicateUserName(string userName) =>
        Create(nameof(DuplicateUserName), $"Brugernavnet '{userName}' er allerede i brug.");

    public override IdentityError DuplicateEmail(string email) =>
        Create(nameof(DuplicateEmail), $"E-mailadressen '{email}' er allerede i brug.");

    public override IdentityError InvalidRoleName(string? role) =>
        Create(nameof(InvalidRoleName), $"Rollenavnet '{role}' er ugyldigt.");

    public override IdentityError DuplicateRoleName(string role) =>
        Create(nameof(DuplicateRoleName), $"Rollen '{role}' findes allerede.");

    public override IdentityError UserAlreadyHasPassword() =>
        Create(nameof(UserAlreadyHasPassword), "Brugeren har allerede en adgangskode.");

    public override IdentityError UserLockoutNotEnabled() =>
        Create(nameof(UserLockoutNotEnabled), "Kontolåsning er ikke aktiveret for denne bruger.");

    public override IdentityError UserAlreadyInRole(string role) =>
        Create(nameof(UserAlreadyInRole), $"Brugeren har allerede rollen '{role}'.");

    public override IdentityError UserNotInRole(string role) =>
        Create(nameof(UserNotInRole), $"Brugeren har ikke rollen '{role}'.");

    public override IdentityError PasswordTooShort(int length) =>
        Create(nameof(PasswordTooShort),
            string.Create(CultureInfo.InvariantCulture, $"Adgangskoden skal være på mindst {length} tegn."));

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
        Create(nameof(PasswordRequiresUniqueChars),
            string.Create(CultureInfo.InvariantCulture, $"Adgangskoden skal indeholde mindst {uniqueChars} forskellige tegn."));

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        Create(nameof(PasswordRequiresNonAlphanumeric), "Adgangskoden skal indeholde mindst ét specialtegn.");

    public override IdentityError PasswordRequiresDigit() =>
        Create(nameof(PasswordRequiresDigit), "Adgangskoden skal indeholde mindst ét tal (0-9).");

    public override IdentityError PasswordRequiresLower() =>
        Create(nameof(PasswordRequiresLower), "Adgangskoden skal indeholde mindst ét lille bogstav (a-z).");

    public override IdentityError PasswordRequiresUpper() =>
        Create(nameof(PasswordRequiresUpper), "Adgangskoden skal indeholde mindst ét stort bogstav (A-Z).");

    private static IdentityError Create(string code, string description) =>
        new() { Code = code, Description = description };
}