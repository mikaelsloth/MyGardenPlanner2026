namespace MyGardenPlanner2026.Tests.UI.Components.Account;

using FluentAssertions;
using MyGardenPlanner2026.Components.Account;
using MyGardenPlanner2026.Components.Account.Pages;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Xunit;

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
}