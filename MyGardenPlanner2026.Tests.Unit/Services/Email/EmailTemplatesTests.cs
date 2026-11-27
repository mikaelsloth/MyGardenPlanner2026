namespace MyGardenPlanner2026.Tests.Unit.Services.Email;

using FluentAssertions;
using MyGardenPlanner2026.Infrastructure.Services.Email;

public sealed class EmailTemplatesTests
{
    private const string EncodedLink = "https://localhost/Account/ConfirmEmail?userId=1&amp;code=abc";

    [Fact]
    public void ConfirmEmail_UsesDanishSubjectAndKeepsEncodedLinkInHref()
    {
        var content = EmailTemplates.ConfirmEmail(EncodedLink);

        content.Subject.Should().Be("Bekræft din e-mail – MyGardenPlanner");
        content.HtmlBody.Should().Contain($"href=\"{EncodedLink}\"");
    }

    [Fact]
    public void ResetPassword_UsesDanishSubjectAndKeepsEncodedLinkInHref()
    {
        var content = EmailTemplates.ResetPassword(EncodedLink);

        content.Subject.Should().Be("Nulstil din adgangskode – MyGardenPlanner");
        content.HtmlBody.Should().Contain($"href=\"{EncodedLink}\"");
    }

    [Fact]
    public void ResetPasswordCode_HtmlEncodesTheCode()
    {
        var content = EmailTemplates.ResetPasswordCode("<b>123</b>");

        content.HtmlBody.Should().Contain("&lt;b&gt;123&lt;/b&gt;");
        content.HtmlBody.Should().NotContain("<b>123</b>");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Templates_RejectBlankInput(string? value)
    {
        var confirm = () => EmailTemplates.ConfirmEmail(value!);
        var reset = () => EmailTemplates.ResetPassword(value!);
        var code = () => EmailTemplates.ResetPasswordCode(value!);

        confirm.Should().Throw<ArgumentException>();
        reset.Should().Throw<ArgumentException>();
        code.Should().Throw<ArgumentException>();
    }
}