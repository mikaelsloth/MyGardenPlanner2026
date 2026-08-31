namespace MyGardenPlanner2026.Configuration.RateLimiting;

/// <summary>
/// Afgør om en HTTP-forespørgsel rammer et af de statisk renderede login-endpoints,
/// der skal underlægges den strikte rate limiter (§4.1). Ren, testbar logik uden
/// afhængighed af selve rate limiter-infrastrukturen. Matcher udelukkende POST, da
/// GET blot indlæser formularen og ikke udgør et loginforsøg.
/// </summary>
public static class AdminAuthPathMatcher
{
    private static readonly string[] ProtectedPaths =
    [
        "/account/login",
        "/account/loginwith2fa",
        "/account/loginwithrecoverycode"
    ];

    public static bool IsProtectedAuthRequest(string httpMethod, string path)
    {
        if (!string.Equals(httpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        foreach (var protectedPath in ProtectedPaths)
        {
            if (path.Equals(protectedPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}