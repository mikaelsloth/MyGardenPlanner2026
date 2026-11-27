namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteC;

using Microsoft.Playwright;

/// <summary>Selektorer og handlinger på /Account/Manage/DeletePersonalData (Tjekliste C5/C6).</summary>
internal static class DeletePersonalDataPage
{
    public const string DeleteButtonName = "Slet data og luk min konto";
    public const string DeleteChoice = "delete";
    public const string TransferPrefix = "transfer:";
    public const string NoChoiceOption = "Vælg…";
    public const string DeleteOptionText = "Slet haven for alle medlemmer";

    public static async Task<IPage> OpenAsync(PlaywrightAppFixture fixture, SmokeTestUser user)
    {
        var page = await fixture.LoginAndGotoAsync(user, E2ERoutes.DeletePersonalData, null);
        await SmokePageAssertions.ExpectPageAsync(page, ManagePageTexts.DeleteTitle, ManagePageTexts.ManageHeading);
        return page;
    }

    public static ILocator Choice(IPage page, int index) => page.Locator($"#garden-choice-{index}");

    public static ILocator Confirm(IPage page, int index) => page.Locator($"#garden-confirm-{index}");

    public static ILocator ErrorMessage(IPage page) => page.Locator(".status-message.status-danger[role=alert]");

    public static ILocator DeleteButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = DeleteButtonName, Exact = true });

    public static Task ChooseTransferAsync(IPage page, int index, string newOwnerUserId) =>
        Choice(page, index).SelectOptionAsync(new SelectOptionValue { Value = TransferPrefix + newOwnerUserId });

    public static async Task ChooseDeleteForAllAsync(IPage page, int index, string confirmName)
    {
        await Choice(page, index).SelectOptionAsync(new SelectOptionValue { Value = DeleteChoice });
        await Confirm(page, index).FillAsync(confirmName);
    }

    public static async Task SubmitAsync(IPage page, SmokeTestUser user)
    {
        await page.GetByLabel("Adgangskode", new() { Exact = true }).FillAsync(user.Password);
        await DeleteButton(page).ClickAsync();
    }

    /// <summary>Succes redirecter til forsiden (RedirectTo("")).</summary>
    public static Task ExpectLandingPageAsync(IPage page) =>
        page.WaitForURLAsync(url => new Uri(url).AbsolutePath == "/");
}