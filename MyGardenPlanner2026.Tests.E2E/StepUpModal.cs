namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;

/// <summary>
/// Page-object for StepUpReAuthModal (role=dialog, "Bekræft din identitet"). Modalen vises
/// ved første beskyttede handling i en frisk circuit: login markerer kun re-auth i
/// request-scope, så ingen ventetid er nødvendig.
/// </summary>
public static class StepUpModal
{
    public static ILocator Dialog(IPage page) =>
        page.GetByRole(AriaRole.Dialog, new() { Name = "Bekræft din identitet" });

    public static ILocator PasswordField(IPage page) =>
        Dialog(page).GetByLabel("Adgangskode", new() { Exact = true });

    public static ILocator TotpField(IPage page) =>
        Dialog(page).GetByLabel("Godkendelseskode", new() { Exact = true });

    public static ILocator ConfirmButton(IPage page) =>
        Dialog(page).GetByRole(AriaRole.Button, new() { Name = "Bekræft", Exact = true });

    public static ILocator CancelButton(IPage page) =>
        Dialog(page).GetByRole(AriaRole.Button, new() { Name = "Annullér", Exact = true });

    public static ILocator Error(IPage page) =>
        Dialog(page).Locator(".status-message.status-danger");

    /// <summary>
    /// Gentager triggeren, til modalen er vist (Blazor ignorerer klik, til kredsløbet er
    /// forbundet). Triggeren skal være idempotent, indtil modalen åbner.
    /// </summary>
    public static Task OpenAsync(IPage page, Func<Task> trigger) =>
        BlazorInteraction.ActUntilAsync(
            trigger,
            async () => await Dialog(page).IsVisibleAsync(),
            maxAttempts: 12,
            description: "Step-up modalen blev ikke vist");

    /// <summary>Korrekt password + TOTP (brugeren skal have AuthenticatorKey). Slutter, når modalen er lukket.</summary>
    public static async Task CompleteAsync(IPage page, SmokeTestUser user)
    {
        await PasswordField(page).FillAsync(user.Password);
        await TotpField(page).FillAsync(TotpHelper.GenerateCode(user.AuthenticatorKey!));
        await ConfirmButton(page).ClickAsync();

        await Assertions.Expect(Dialog(page)).ToBeHiddenAsync();
    }
}