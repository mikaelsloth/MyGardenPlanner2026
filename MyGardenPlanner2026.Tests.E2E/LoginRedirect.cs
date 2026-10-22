namespace MyGardenPlanner2026.Tests.E2E;

using System.Web;

/// <summary>
/// Udleder stien i ReturnUrl fra en login-URL. Appen redirecter på to måder: auth-middleware sender en
/// relativ "ReturnUrl" (/admin/x), mens RedirectToLogin sender en absolut "returnUrl"
/// (http://host/admin/x). Begge giver samme sti her. Nøglen er ikke versalfølsom.
/// </summary>
public static class LoginRedirect
{
    public static string? ReturnPath(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var value = HttpUtility.ParseQueryString(uri.Query)["ReturnUrl"];
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        // Uri.TryCreate("/sti", Absolute) giver en fil-URI på Linux, så skemaet tjekkes eksplicit.
        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return new Uri(value).AbsolutePath;
        }

        var queryStart = value.IndexOf('?', StringComparison.Ordinal);
        return queryStart < 0 ? value : value[..queryStart];
    }
}