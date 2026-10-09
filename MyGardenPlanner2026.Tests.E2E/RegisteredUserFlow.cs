namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;

/// <summary>
/// Opretter og bekræfter en ny bruger via UI (Register → RegisterConfirmation → ConfirmEmail).
/// Giver hver test sin egen bruger uden at røre de delte personas. E-mails ligger i
/// @test.dk, så ResetSmokeTestUsersInDB.sql rydder dem.
/// </summary>
public static class RegisteredUserFlow
{
    private const string ConfirmationLinkText = "Klik her for at bekræfte din konto";
    private const string ConfirmedText = "Tak, fordi du bekræftede din e-mail.";

    public static string NewEmail() => $"e2e-{Guid.NewGuid():N}@test.dk";

    /// <summary>Udfylder og sender formularen. Siden skal være /Account/Register. Slutter på RegisterConfirmation.</summary>
    public static async Task<SmokeTestUser> RegisterAsync(IPage page, string? email = null)
    {
        var user = new SmokeTestUser(
            Email: email ?? NewEmail(),
            Password: SmokeTestDataSeeder.SharedPassword,
            Role: null,
            TwoFactorEnabled: false,
            AuthenticatorKey: null);

        await page.GetByLabel("E-mail", new() { Exact = true }).FillAsync(user.Email);
        await page.GetByLabel("Adgangskode", new() { Exact = true }).FillAsync(user.Password);
        await page.GetByLabel("Bekræft adgangskode", new() { Exact = true }).FillAsync(user.Password);

        await page.GetByRole(AriaRole.Button, new() { Name = "Opret bruger", Exact = true }).ClickAsync();

        try
        {
            await page.WaitForURLAsync(
                url => url.Contains(E2ERoutes.RegisterConfirmation, StringComparison.OrdinalIgnoreCase),
                new() { Timeout = 10000 });
        }
        catch (TimeoutException)
        {
            var errorText = await page.Locator(".field-message, .text-danger, [role='alert']").AllTextContentsAsync();

            throw new InvalidOperationException(
                $"Oprettelse af '{user.Email}' nåede ikke {E2ERoutes.RegisterConfirmation}. " +
                $"Nuværende URL: '{page.Url}'. Fejltekst på siden: [{string.Join(" | ", errorText)}]");
        }

        return user;
    }

    /// <summary>Følger bekræftelseslinket på RegisterConfirmation og verificerer, at kontoen er bekræftet.</summary>
    public static async Task ConfirmAsync(IPage page)
    {
        await page.GetByRole(AriaRole.Link, new() { Name = ConfirmationLinkText }).ClickAsync();
        await Assertions.Expect(page.GetByText(ConfirmedText)).ToBeVisibleAsync();
    }

    public static async Task<SmokeTestUser> RegisterAndConfirmAsync(IPage page, string rootUri)
    {
        await page.GotoAsync($"{rootUri}{E2ERoutes.Register}");
        var user = await RegisterAsync(page);
        await ConfirmAsync(page);
        return user;
    }
}