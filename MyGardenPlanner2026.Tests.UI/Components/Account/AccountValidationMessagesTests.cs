namespace MyGardenPlanner2026.Tests.UI.Components.Account;

using FluentAssertions;
using MyGardenPlanner2026.Components.Account;
using MyGardenPlanner2026.Components.Account.Pages;
using MyGardenPlanner2026.Components.Account.Pages.Manage;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Xunit;
using ProfileIndex = MyGardenPlanner2026.Components.Account.Pages.Manage.Index;

public class AccountValidationMessagesTests
{
    private static List<string> Validate(Type pageType, params (string Property, object? Value)[] values)
    {
        var modelType = pageType.GetNestedType("InputModel", BindingFlags.NonPublic)!;
        var model = Activator.CreateInstance(modelType)!;

        foreach (var (property, value) in values)
        {
            modelType.GetProperty(property)!.SetValue(model, value);
        }

        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

        return [.. results.Select(r => r.ErrorMessage!)];
    }

    [Fact]
    public void Register_EmptyFields_GiveDanishRequiredMessages()
    {
        var messages = Validate(typeof(Register));

        messages.Should().Contain("Feltet 'E-mail' er påkrævet.");
        messages.Should().Contain("Feltet 'Adgangskode' er påkrævet.");
    }

    [Fact]
    public void Register_InvalidEmail_GivesDanishMessage()
    {
        var messages = Validate(typeof(Register),
            ("Email", "abc"), ("Password", "Abcdef1"), ("ConfirmPassword", "Abcdef1"));

        messages.Should().ContainSingle().Which.Should().Be("Angiv en gyldig e-mailadresse.");
    }

    [Fact]
    public void Register_ShortPassword_GivesDanishLengthMessage()
    {
        var messages = Validate(typeof(Register),
            ("Email", "a@test.dk"), ("Password", "abc"), ("ConfirmPassword", "abc"));

        messages.Should().ContainSingle()
            .Which.Should().Be("Adgangskoden skal være mindst 6 og højst 100 tegn.");
    }

    [Fact]
    public void Register_PasswordMismatch_GivesDanishMessage()
    {
        var messages = Validate(typeof(Register),
            ("Email", "a@test.dk"), ("Password", "Abcdef1"), ("ConfirmPassword", "Abcdef2"));

        messages.Should().ContainSingle()
            .Which.Should().Be("Adgangskoden og bekræftelsen er ikke ens.");
    }

    [Fact]
    public void Register_ValidInput_GivesNoMessages()
    {
        var messages = Validate(typeof(Register),
            ("Email", "a@test.dk"), ("Password", "Abcdef1"), ("ConfirmPassword", "Abcdef1"));

        messages.Should().BeEmpty();
    }

    [Fact]
    public void Login_EmptyFields_GiveDanishRequiredMessages()
    {
        var messages = Validate(typeof(Login));

        messages.Should().Contain("Feltet 'E-mail' er påkrævet.");
        messages.Should().Contain("Feltet 'Adgangskode' er påkrævet.");
    }

    [Fact]
    public void Login_InvalidEmail_GivesDanishMessage()
    {
        var messages = Validate(typeof(Login), ("Email", "abc"), ("Password", "x"));

        messages.Should().ContainSingle().Which.Should().Be("Angiv en gyldig e-mailadresse.");
    }

    [Fact]
    public void Login_ValidInput_GivesNoMessages()
    {
        var messages = Validate(typeof(Login), ("Email", "a@test.dk"), ("Password", "x"));

        messages.Should().BeEmpty();
    }

    [Fact]
    public void InvalidLoginAttempt_IsDanish_AndKeepsEnglishErrorPrefix() => AccountMessages.InvalidLoginAttempt.Should().Be("Error: Ugyldigt loginforsøg.");

    [Fact]
    public void UserNotLoaded_KeepsEnglishErrorPrefix_AndIncludesUserId()
    {
        var message = AccountMessages.UserNotLoaded("abc-123");

        message.Should().StartWith("Error");
        message.Should().Contain("abc-123");
    }

    [Fact]
    public void EnableAuthenticator_EmptyCode_GivesDanishMessages()
    {
        var messages = Validate(typeof(EnableAuthenticator));

        messages.Should().Contain("Feltet 'Bekræftelseskode' er påkrævet.");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678")]
    public void EnableAuthenticator_WrongLength_GivesDanishLengthMessage(string code)
    {
        var messages = Validate(typeof(EnableAuthenticator), ("Code", code));

        messages.Should().ContainSingle()
            .Which.Should().Be("Bekræftelseskode skal være mellem 6 og 7 tegn.");
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("123 456")]
    public void EnableAuthenticator_ValidCode_GivesNoMessages(string code) => Validate(typeof(EnableAuthenticator), ("Code", code)).Should().BeEmpty();

    [Fact]
    public void InvalidVerificationCode_IsDanish_AndKeepsEnglishErrorPrefix() => AccountMessages.InvalidVerificationCode.Should().Be("Error: Bekræftelseskoden er ugyldig.");

    [Fact]
    public void SuccessStatusMessages_DoNotStartWithError()
    {
        string[] messages =
        [
            AccountMessages.AuthenticatorVerified,
            AccountMessages.AuthenticatorKeyWasReset,
            AccountMessages.TwoFactorHasBeenDisabled,
            AccountMessages.NewRecoveryCodesGenerated,
            AccountMessages.BrowserWasForgotten,
        ];

        foreach (var message in messages)
        {
            message.Should().NotBeNullOrWhiteSpace().And.NotStartWith("Error");
        }
    }

    [Fact]
    public void RenamePasskey_EmptyName_GivesDanishRequiredMessage()
    {
        var messages = Validate(typeof(RenamePasskey));

        messages.Should().ContainSingle().Which.Should().Be("Feltet 'Passkey-navn' er påkrævet.");
    }

    [Fact]
    public void RenamePasskey_TooLongName_GivesDanishLengthMessage()
    {
        var messages = Validate(typeof(RenamePasskey), ("Name", new string('x', 201)));

        messages.Should().ContainSingle().Which.Should().Be("Passkey-navnet må højst være 200 tegn.");
    }

    [Fact]
    public void RenamePasskey_ValidName_GivesNoMessages() => Validate(typeof(RenamePasskey), ("Name", "Min telefon")).Should().BeEmpty();

    [Fact]
    public void ExternalLogin_EmptyEmail_GivesDanishRequiredMessage()
    {
        var messages = Validate(typeof(ExternalLogin));

        messages.Should().ContainSingle().Which.Should().Be("Feltet 'E-mail' er påkrævet.");
    }

    [Fact]
    public void ExternalLogin_InvalidEmail_GivesDanishMessage()
    {
        var messages = Validate(typeof(ExternalLogin), ("Email", "abc"));

        messages.Should().ContainSingle().Which.Should().Be("Angiv en gyldig e-mailadresse.");
    }

    [Fact]
    public void PasskeyAndExternalLoginErrorMessages_StartWithEnglishErrorPrefix()
    {
        string[] errors =
        [
            AccountMessages.InvalidPasskeyId,
            AccountMessages.PasskeyBrowserDidNotProvide,
            AccountMessages.PasskeyLimitReached,
            AccountMessages.PasskeyNotAddedToAccount,
            AccountMessages.PasskeyDeleteFailed,
            AccountMessages.PasskeyNotFound,
            AccountMessages.PasskeyUpdateFailed,
            AccountMessages.ExternalLoginRemoveFailed,
            AccountMessages.ExternalLoginInfoNotLoaded,
            AccountMessages.ExternalLoginNotAdded,
            AccountMessages.PasskeyNotAdded("detalje"),
            AccountMessages.UnknownAction("x"),
        ];

        foreach (var message in errors)
        {
            message.Should().StartWith("Error: ");
        }
    }

    [Fact]
    public void PasskeyAndExternalLoginSuccessMessages_DoNotStartWithError()
    {
        string[] successes =
        [
            AccountMessages.PasskeyDeleted,
            AccountMessages.PasskeyUpdated,
            AccountMessages.ExternalLoginRemoved,
            AccountMessages.ExternalLoginAdded,
        ];

        foreach (var message in successes)
        {
            message.Should().NotBeNullOrWhiteSpace().And.NotStartWith("Error");
        }
    }

    [Fact]
    public void PasskeyNotAdded_IncludesFrameworkDetail() => AccountMessages.PasskeyNotAdded("Ugyldig attestation").Should()
            .Be("Error: Passkeyen kunne ikke tilføjes: Ugyldig attestation");

    [Fact]
    public void ManageEmail_EmptyNewEmail_GivesDanishRequiredMessage()
    {
        var messages = Validate(typeof(Email));

        messages.Should().ContainSingle().Which.Should().Be("Feltet 'Ny e-mail' er påkrævet.");
    }

    [Fact]
    public void ManageEmail_InvalidNewEmail_GivesDanishMessage()
    {
        var messages = Validate(typeof(Email), ("NewEmail", "abc"));

        messages.Should().ContainSingle().Which.Should().Be("Angiv en gyldig e-mailadresse.");
    }

    [Fact]
    public void ManageEmail_ValidNewEmail_GivesNoMessages() => Validate(typeof(Email), ("NewEmail", "ny@test.dk")).Should().BeEmpty();

    [Fact]
    public void ChangePassword_EmptyFields_GiveDanishRequiredMessages()
    {
        var messages = Validate(typeof(ChangePassword));

        messages.Should().Contain("Feltet 'Nuværende adgangskode' er påkrævet.");
        messages.Should().Contain("Feltet 'Ny adgangskode' er påkrævet.");
    }

    [Fact]
    public void ChangePassword_ShortNewPassword_GivesDanishLengthMessage()
    {
        var messages = Validate(typeof(ChangePassword),
            ("OldPassword", "x"), ("NewPassword", "abc"), ("ConfirmPassword", "abc"));

        messages.Should().ContainSingle()
            .Which.Should().Be("Adgangskoden skal være mindst 6 og højst 100 tegn.");
    }

    [Fact]
    public void ChangePassword_Mismatch_GivesDanishMessage()
    {
        var messages = Validate(typeof(ChangePassword),
            ("OldPassword", "x"), ("NewPassword", "Abcdef1"), ("ConfirmPassword", "Abcdef2"));

        messages.Should().ContainSingle()
            .Which.Should().Be("Adgangskoden og bekræftelsen er ikke ens.");
    }

    [Fact]
    public void ChangePassword_ValidInput_GivesNoMessages() => Validate(typeof(ChangePassword),
            ("OldPassword", "x"), ("NewPassword", "Abcdef1"), ("ConfirmPassword", "Abcdef1"))
            .Should().BeEmpty();

    [Fact]
    public void SetPassword_EmptyNewPassword_GivesDanishRequiredMessage()
    {
        var messages = Validate(typeof(SetPassword));

        messages.Should().ContainSingle().Which.Should().Be("Feltet 'Ny adgangskode' er påkrævet.");
    }

    [Fact]
    public void SetPassword_Mismatch_GivesDanishMessage()
    {
        var messages = Validate(typeof(SetPassword),
            ("NewPassword", "Abcdef1"), ("ConfirmPassword", "Abcdef2"));

        messages.Should().ContainSingle()
            .Which.Should().Be("Adgangskoden og bekræftelsen er ikke ens.");
    }

    [Fact]
    public void SetPassword_ValidInput_GivesNoMessages() => Validate(typeof(SetPassword), ("NewPassword", "Abcdef1"), ("ConfirmPassword", "Abcdef1"))
            .Should().BeEmpty();

    [Fact]
    public void Profile_InvalidPhoneNumber_GivesDanishMessage()
    {
        var messages = Validate(typeof(ProfileIndex), ("PhoneNumber", "abc"));

        messages.Should().ContainSingle().Which.Should().Be("Angiv et gyldigt telefonnummer.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("12345678")]
    public void Profile_ValidOrEmptyPhoneNumber_GivesNoMessages(string? phoneNumber) => Validate(typeof(ProfileIndex), ("PhoneNumber", phoneNumber)).Should().BeEmpty();

    [Fact]
    public void AccountPageStatusMessages_FollowErrorPrefixConvention()
    {
        AccountMessages.PhoneNumberNotSaved.Should().StartWith("Error: ");

        string[] successes =
        [
            AccountMessages.ProfileUpdated,
            AccountMessages.PasswordsWasChanged,
            AccountMessages.PasswordsWasSet,
            AccountMessages.EmailUnchanged,
            AccountMessages.EmailChangeConfirmationSent,
            AccountMessages.EmailVerificationSent,
        ];

        foreach (var message in successes)
        {
            message.Should().NotBeNullOrWhiteSpace().And.NotStartWith("Error");
        }
    }
}