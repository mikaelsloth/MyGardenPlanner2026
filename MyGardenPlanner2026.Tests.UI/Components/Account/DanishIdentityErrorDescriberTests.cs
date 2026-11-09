namespace MyGardenPlanner2026.Tests.UI.Components.Account;

using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MyGardenPlanner2026.Components.Account;
using Xunit;

public class DanishIdentityErrorDescriberTests
{
    private static readonly IdentityErrorDescriber Standard = new();
    private static readonly DanishIdentityErrorDescriber Danish = new();

    private static readonly (string Name, Func<IdentityErrorDescriber, IdentityError> Get)[] AllErrors =
    [
        (nameof(IdentityErrorDescriber.DefaultError), d => d.DefaultError()),
        (nameof(IdentityErrorDescriber.ConcurrencyFailure), d => d.ConcurrencyFailure()),
        (nameof(IdentityErrorDescriber.PasswordMismatch), d => d.PasswordMismatch()),
        (nameof(IdentityErrorDescriber.InvalidToken), d => d.InvalidToken()),
        (nameof(IdentityErrorDescriber.RecoveryCodeRedemptionFailed), d => d.RecoveryCodeRedemptionFailed()),
        (nameof(IdentityErrorDescriber.LoginAlreadyAssociated), d => d.LoginAlreadyAssociated()),
        (nameof(IdentityErrorDescriber.InvalidUserName), d => d.InvalidUserName("x")),
        (nameof(IdentityErrorDescriber.InvalidEmail), d => d.InvalidEmail("x")),
        (nameof(IdentityErrorDescriber.DuplicateUserName), d => d.DuplicateUserName("x")),
        (nameof(IdentityErrorDescriber.DuplicateEmail), d => d.DuplicateEmail("x")),
        (nameof(IdentityErrorDescriber.InvalidRoleName), d => d.InvalidRoleName("x")),
        (nameof(IdentityErrorDescriber.DuplicateRoleName), d => d.DuplicateRoleName("x")),
        (nameof(IdentityErrorDescriber.UserAlreadyHasPassword), d => d.UserAlreadyHasPassword()),
        (nameof(IdentityErrorDescriber.UserLockoutNotEnabled), d => d.UserLockoutNotEnabled()),
        (nameof(IdentityErrorDescriber.UserAlreadyInRole), d => d.UserAlreadyInRole("x")),
        (nameof(IdentityErrorDescriber.UserNotInRole), d => d.UserNotInRole("x")),
        (nameof(IdentityErrorDescriber.PasswordTooShort), d => d.PasswordTooShort(6)),
        (nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars), d => d.PasswordRequiresUniqueChars(3)),
        (nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric), d => d.PasswordRequiresNonAlphanumeric()),
        (nameof(IdentityErrorDescriber.PasswordRequiresDigit), d => d.PasswordRequiresDigit()),
        (nameof(IdentityErrorDescriber.PasswordRequiresLower), d => d.PasswordRequiresLower()),
        (nameof(IdentityErrorDescriber.PasswordRequiresUpper), d => d.PasswordRequiresUpper()),
    ];

    [Fact]
    public void AllErrors_KeepStandardCode()
    {
        foreach (var (name, get) in AllErrors)
        {
            get(Danish).Code.Should().Be(get(Standard).Code, because: name);
        }
    }

    [Fact]
    public void AllErrors_HaveNonEmptyDescription_DifferentFromEnglishStandard()
    {
        foreach (var (name, get) in AllErrors)
        {
            var description = get(Danish).Description;

            description.Should().NotBeNullOrWhiteSpace(because: name);
            description.Should().NotBe(get(Standard).Description, because: name);
        }
    }

    [Fact]
    public void PasswordTooShort_IncludesRequiredLength() => Danish.PasswordTooShort(12).Description.Should().Contain("12");

    [Fact]
    public void PasswordRequiresUniqueChars_IncludesCount() => Danish.PasswordRequiresUniqueChars(3).Description.Should().Contain("3");

    [Fact]
    public void DuplicateUserName_IncludesUserName() => Danish.DuplicateUserName("a@test.dk").Description.Should().Contain("a@test.dk");

    [Fact]
    public void DuplicateEmail_IncludesEmail() => Danish.DuplicateEmail("a@test.dk").Description.Should().Contain("a@test.dk");

    [Fact]
    public void PasswordMismatch_IsDanish() => Danish.PasswordMismatch().Description.Should().Be("Adgangskoden er forkert.");
}