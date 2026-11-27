namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteC;

using Microsoft.Playwright;

internal static class GardenRestoreFlow
{
    /// <summary>
    /// Registrerer, bekræfter og logger ind som ny bruger med den angivne e-mail. Bruges samme e-mail som en
    /// slettet brugers, er det en genregistrering (R10.1e).
    /// </summary>
    public static async Task<SmokeTestUser> RegisterAndLoginAsync(PlaywrightAppFixture fixture, IPage page, string email)
    {
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Register}");
        var user = await RegisteredUserFlow.RegisterAsync(page, email);
        await RegisteredUserFlow.ConfirmAsync(page);

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
        await LoginFlow.LoginAsync(page, user);
        return user;
    }
}