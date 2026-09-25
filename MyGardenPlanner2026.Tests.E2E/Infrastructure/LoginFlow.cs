namespace MyGardenPlanner2026.Tests.E2E.Infrastructure;

using Microsoft.Playwright;

public static class LoginFlow
{
    public static async Task LoginAsync(IPage page, SmokeTestUser user)
    {
        await page.GetByLabel("E-mail").FillAsync(user.Email);
        await page.GetByLabel("Adgangskode", new() { Exact = true }).FillAsync(user.Password);

        await ClickAndVerifyNavigationAsync(page, $"Login (credentials) for '{user.Email}'");

        if (user.TwoFactorEnabled)
        {
            var code = TotpHelper.GenerateCode(user.AuthenticatorKey!);
            await page.GetByLabel("Godkendelseskode").FillAsync(code);

            await ClickAndVerifyNavigationAsync(page, $"2FA-login for '{user.Email}' med kode '{code}'");
        }
    }

    private static async Task ClickAndVerifyNavigationAsync(IPage page, string context)
    {
        var beforeUrl = page.Url;
        await page.GetByRole(AriaRole.Button, new() { Name = "Log ind", Exact = true }).ClickAsync();

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