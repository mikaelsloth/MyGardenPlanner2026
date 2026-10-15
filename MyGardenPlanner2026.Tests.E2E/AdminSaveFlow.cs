namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;

/// <summary>
/// Fælles gem-flow for admin-editors med step-up: klik "Gem", gennemfør step-up hvis modalen vises
/// (en frisk circuit har intet re-auth-tidsstempel; efterfølgende gem inden for vinduet har ingen
/// modal), og afvent succesbeskeden. Succesbeskeden skal være unik for handlingen, ellers kan en
/// bestående besked fra en tidligere handling give falsk succes.
/// </summary>
public static class AdminSaveFlow
{
    public static async Task SaveWithStepUpAsync(
        IPage page,
        SmokeTestUser user,
        ILocator saveButton,
        ILocator successMessage,
        ILocator errorMessage,
        string description)
    {
        await saveButton.ClickAsync();

        await BlazorInteraction.WaitUntilAsync(
            async () => await StepUpModal.Dialog(page).IsVisibleAsync() || await successMessage.IsVisibleAsync(),
            TimeSpan.FromSeconds(10));

        if (await StepUpModal.Dialog(page).IsVisibleAsync())
        {
            await StepUpModal.CompleteAsync(page, user);
        }

        try
        {
            await Assertions.Expect(successMessage).ToBeVisibleAsync();
        }
        catch (PlaywrightException)
        {
            var errors = await errorMessage.AllTextContentsAsync();

            throw new InvalidOperationException(
                $"{description} gav ingen succesbesked. URL: '{page.Url}'. " +
                $"Fejltekst på siden: [{string.Join(" | ", errors)}]");
        }
    }
}