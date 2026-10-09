namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;

/// <summary>Brugeren med aktiveret 2FA (nøgle udfyldt) og de 10 gendannelseskoder, der blev vist ved aktiveringen.</summary>
public sealed record TwoFactorEnrollment(SmokeTestUser User, IReadOnlyList<string> RecoveryCodes);

/// <summary>
/// Aktiverer 2FA via UI på /Account/Manage/EnableAuthenticator. Nøglen læses fra siden (<kbd>) og
/// omsættes til TOTP-koder med TotpHelper, så ingen rigtig authenticator-app eller QR-scanning
/// er nødvendig. Brugeren skal være logget ind og uden 2FA.
/// </summary>
public static class TwoFactorSetup
{
    /// <summary>Læser nøglen fra EnableAuthenticator-siden som rå Base32 (store bogstaver, uden mellemrum).</summary>
    public static async Task<string> ReadSharedKeyAsync(IPage page)
    {
        var formatted = await page.Locator("kbd").TextContentAsync()
            ?? throw new InvalidOperationException("Authenticator-nøglen (<kbd>) blev ikke fundet på siden.");

        return NormalizeKey(formatted);
    }

    public static string NormalizeKey(string formattedKey) =>
        string.Concat(formattedKey.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();

    /// <summary>Indtaster en gyldig kode, klikker "Verify" og returnerer de viste gendannelseskoder.</summary>
    public static async Task<IReadOnlyList<string>> VerifyAsync(IPage page, string key)
    {
        await page.GetByLabel("Verification Code").FillAsync(TotpHelper.GenerateCode(key));
        await page.GetByRole(AriaRole.Button, new() { Name = "Verify", Exact = true }).ClickAsync();

        try
        {
            await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Gendannelseskoder" }))
                .ToBeVisibleAsync();
        }
        catch (PlaywrightException)
        {
            var errorText = await page.Locator(".field-message, .text-danger, [role='alert']").AllTextContentsAsync();

            throw new InvalidOperationException(
                $"Gendannelseskoder blev ikke vist efter verificering. URL: '{page.Url}'. " +
                $"Fejltekst på siden: [{string.Join(" | ", errorText)}]");
        }

        return await page.Locator("code.recovery-code").AllTextContentsAsync();
    }

    /// <summary>Åbner EnableAuthenticator direkte, aktiverer 2FA og returnerer brugeren med nøgle samt gendannelseskoder.</summary>
    public static async Task<TwoFactorEnrollment> EnableAsync(IPage page, string rootUri, SmokeTestUser user)
    {
        await page.GotoAsync($"{rootUri}{E2ERoutes.EnableAuthenticator}");

        var key = await ReadSharedKeyAsync(page);
        var recoveryCodes = await VerifyAsync(page, key);

        return new TwoFactorEnrollment(user with { TwoFactorEnabled = true, AuthenticatorKey = key }, recoveryCodes);
    }
}