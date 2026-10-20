namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;

public static class LoginFlow
{
    public static async Task LoginAsync(IPage page, SmokeTestUser user)
    {
        await SubmitPasswordAsync(page, user);

        if (user.TwoFactorEnabled)
        {
            await SubmitTwoFactorCodeAsync(page, user);
        }
    }

    /// <summary>Trin 1: e-mail og adgangskode. Slutter, når siden har skiftet URL (fx til LoginWith2fa).</summary>
    public static async Task SubmitPasswordAsync(IPage page, SmokeTestUser user)
    {
        await FillCredentialsAsync(page, user.Email, user.Password);

        await ClickAndVerifyNavigationAsync(page, $"Login (credentials) for '{user.Email}'");
    }

    /// <summary>Trin 2: TOTP-kode udregnet ud fra brugerens authenticator-nøgle. Slutter, når siden har skiftet URL.</summary>
    public static async Task SubmitTwoFactorCodeAsync(IPage page, SmokeTestUser user)
    {
        var code = TotpHelper.GenerateCode(user.AuthenticatorKey!);
        await page.GetByLabel("Godkendelseskode").FillAsync(code);

        await ClickAndVerifyNavigationAsync(page, $"2FA-login for '{user.Email}' med kode '{code}'");
    }

    public static async Task FillCredentialsAsync(IPage page, string email, string password)
    {
        await page.GetByLabel("E-mail").FillAsync(email);
        await page.GetByLabel("Adgangskode", new() { Exact = true }).FillAsync(password);
    }

    public static ILocator LoginButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = "Log ind", Exact = true });

    /// <summary>
    /// Ét loginforsøg uden 2FA: ryd cookies, åbn login, send formularen og returnér HTTP-statussen på POST'en.
    /// Bruges, når svaret kan være 429 (LoginAsync venter på URL-skift, som et 429 aldrig giver).
    /// </summary>
    public static async Task<int> AttemptStatusAsync(IPage page, string rootUri, SmokeTestUser user)
    {
        await page.Context.ClearCookiesAsync();
        await page.GotoAsync($"{rootUri}{E2ERoutes.Login}");
        await FillCredentialsAsync(page, user.Email, user.Password);

        var response = await page.RunAndWaitForResponseAsync(
            () => LoginButton(page).ClickAsync(),
            r => r.Request.Method == "POST"
                 && r.Url.Contains(E2ERoutes.Login, StringComparison.OrdinalIgnoreCase));

        return response.Status;
    }

    private static async Task ClickAndVerifyNavigationAsync(IPage page, string context)
    {
        var beforeUrl = page.Url;
        await LoginButton(page).ClickAsync();

        try
        {
            await page.WaitForURLAsync(url => url != beforeUrl, new() { Timeout = 10000 });
        }
        catch (TimeoutException)
        {
            var errorText = await page.Locator(".field-message, .text-danger, [role='alert']").AllTextContentsAsync();
            var title = await page.TitleAsync();

            throw new InvalidOperationException(
                $"{context} skiftede ikke URL væk fra '{beforeUrl}'. " +
                $"Nuværende URL: '{page.Url}'. Sidetitel: '{title}'. " +
                $"Fejltekst på siden: [{string.Join(" | ", errorText)}]");
        }
    }
}