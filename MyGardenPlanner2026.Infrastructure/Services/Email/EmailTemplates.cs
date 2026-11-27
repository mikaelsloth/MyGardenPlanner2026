namespace MyGardenPlanner2026.Infrastructure.Services.Email;

using MyGardenPlanner2026.Core.Contracts.Email;
using System.Net;

/// <summary>
/// Danske mailskabeloner. Links skal allerede være HTML-encodede (Identity-kaldere sender
/// dem sådan); øvrige værdier encodes her.
/// </summary>
public static class EmailTemplates
{
    public static EmailContent ConfirmEmail(string htmlEncodedLink)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(htmlEncodedLink);

        return Build(
            "Bekræft din e-mail – MyGardenPlanner",
            "Bekræft din e-mail",
            "Klik på knappen for at bekræfte din e-mail hos MyGardenPlanner.",
            htmlEncodedLink,
            "Bekræft e-mail",
            "Hvis du ikke selv har bedt om dette, kan du ignorere denne mail.");
    }

    public static EmailContent ResetPassword(string htmlEncodedLink)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(htmlEncodedLink);

        return Build(
            "Nulstil din adgangskode – MyGardenPlanner",
            "Nulstil din adgangskode",
            "Klik på knappen for at vælge en ny adgangskode.",
            htmlEncodedLink,
            "Nulstil adgangskode",
            "Hvis du ikke selv har bedt om dette, kan du ignorere denne mail.");
    }

    public static EmailContent ResetPasswordCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return Build(
            "Din kode til nulstilling af adgangskode – MyGardenPlanner",
            "Nulstil din adgangskode",
            $"Brug denne kode til at nulstille din adgangskode: <strong>{WebUtility.HtmlEncode(code)}</strong>",
            link: null,
            linkText: null,
            "Hvis du ikke selv har bedt om dette, kan du ignorere denne mail.");
    }

    private static EmailContent Build(
        string subject, string heading, string introHtml, string? link, string? linkText, string footer)
    {
        var linkHtml = link is null
            ? string.Empty
            : $"""<p><a href="{link}" style="display:inline-block;padding:10px 18px;background:#3F6B4A;color:#FFFFFF;text-decoration:none;border-radius:8px;">{linkText}</a></p>""";

        var body = $"""
            <html>
            <body style="font-family:system-ui,-apple-system,'Segoe UI',sans-serif;color:#243128;background:#FAF8F2;padding:24px;">
            <h1 style="font-size:1.25rem;">{heading}</h1>
            <p>{introHtml}</p>
            {linkHtml}
            <p style="color:#6F766D;font-size:0.9rem;">{footer}</p>
            </body>
            </html>
            """;

        return new EmailContent(subject, body);
    }
}