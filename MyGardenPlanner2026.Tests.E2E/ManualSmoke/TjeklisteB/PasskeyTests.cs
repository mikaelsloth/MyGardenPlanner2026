namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteB;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Text.RegularExpressions;

/// <summary>
/// Tjekliste B5: passkey. Kører på http://localhost:&lt;port&gt; (WebAuthn accepterer ikke en IP som RP-id)
/// med en virtual authenticator. Ny bruger opretter en passkey, navngiver den, logger ud og logger ind
/// med "Log ind med en passkey". Conditional mediation (autofill) slås fra, så login altid sker via
/// knappen. Skærmbilleder: Passkey_Passkeys, Passkey_RenamePasskey og Passkey_Login (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed partial class PasskeyTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste B";
    private const string TestName = "Passkey";

    // Manage-siderne er ikke oversat endnu (engelske tekster) og ligger i ManageLayout (h1).
    private const string ManageHeading = "Manage your account";
    private const string PasskeyName = "E2E-nøgle";

    private const string DisableConditionalMediation =
        "PublicKeyCredential.isConditionalMediationAvailable = async () => false;";

    // Identity-sider stiller samme RP-id til rådighed som hosten; localhost er gyldigt, 127.0.0.1 er det ikke.
    private string LocalhostUri => fixture.RootUri.Replace("127.0.0.1", "localhost", StringComparison.Ordinal);

    [GeneratedRegex(@"/account/manage/renamepasskey/[^/?]+$", RegexOptions.IgnoreCase)]
    private static partial Regex RenamePasskeyUrl { get; }

    [GeneratedRegex("/account/manage/passkeys$", RegexOptions.IgnoreCase)]
    private static partial Regex PasskeysUrl { get; }

    [GeneratedRegex("/account/login$", RegexOptions.IgnoreCase)]
    private static partial Regex LoginUrl { get; }

    [Theory]
    [InlineData(Viewports.Mobile)]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task Passkey_OpretOgLogIndMedPasskey_Lykkes(int width)
    {
        var page = await fixture.NewPageAsync(width);
        await page.AddInitScriptAsync(DisableConditionalMediation);
        var authenticator = await VirtualAuthenticator.AddAsync(page);

        var user = await RegisteredUserFlow.RegisterAndConfirmAsync(page, LocalhostUri);

        await page.GotoAsync($"{LocalhostUri}{E2ERoutes.Login}");
        await LoginFlow.LoginAsync(page, user);

        // Opret passkey.
        await page.GotoAsync($"{LocalhostUri}{E2ERoutes.Passkeys}");
        await SmokePageAssertions.ExpectPageAsync(page, "Manage your passkeys", ManageHeading);
        await Assertions.Expect(page.GetByText("No passkeys are registered.")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Add a new passkey", Exact = true }).ClickAsync();
        await ExpectAsync(page, "Oprettelse af passkey", () =>
            Assertions.Expect(page).ToHaveURLAsync(RenamePasskeyUrl, new() { Timeout = 10000 }));

        // Navngiv passkey (RenamePasskey har ingen PageTitle, så titlen tjekkes ikke).
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToHaveTextAsync(ManageHeading);
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Enter a name for your passkey" }))
            .ToBeVisibleAsync();
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "RenamePasskey");

        await page.GetByLabel("Passkey name").FillAsync(PasskeyName);
        await page.GetByRole(AriaRole.Button, new() { Name = "Continue", Exact = true }).ClickAsync();

        await ExpectAsync(page, "Navngivning af passkey", () =>
            Assertions.Expect(page).ToHaveURLAsync(PasskeysUrl, new() { Timeout = 10000 }));
        await Assertions.Expect(page.GetByText("Passkey updated successfully.")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Cell, new() { Name = PasskeyName, Exact = true }))
            .ToBeVisibleAsync();
        (await authenticator.CountCredentialsAsync()).Should().Be(1);
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "Passkeys");

        // Log ud og ind med passkey (tomt e-mailfelt: discoverable credential).
        await page.Context.ClearCookiesAsync();
        await page.GotoAsync($"{LocalhostUri}{E2ERoutes.Login}");
        await SmokePageAssertions.ExpectPageAsync(page, "Log ind – MyGardenPlanner", "Log ind");
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "Login");

        await page.GetByRole(AriaRole.Button, new() { Name = "Log ind med en passkey", Exact = true }).ClickAsync();
        await ExpectAsync(page, "Login med passkey", () =>
            Assertions.Expect(page).Not.ToHaveURLAsync(LoginUrl, new() { Timeout = 10000 }));

        // Logget ind: Passkeys-siden kræver login og viser den navngivne passkey.
        await page.GotoAsync($"{LocalhostUri}{E2ERoutes.Passkeys}");
        await SmokePageAssertions.ExpectPageAsync(page, "Manage your passkeys", ManageHeading);
        await Assertions.Expect(page.GetByRole(AriaRole.Cell, new() { Name = PasskeyName, Exact = true }))
            .ToBeVisibleAsync();
    }

    /// <summary>Passkey-fejl vises som StatusMessage (role=alert) efter redirect; tag dem med i fejlen.</summary>
    private static async Task ExpectAsync(IPage page, string step, Func<Task> assertion)
    {
        try
        {
            await assertion();
        }
        catch (PlaywrightException)
        {
            var alerts = await page.Locator("[role='alert']").AllTextContentsAsync();

            throw new InvalidOperationException(
                $"{step} nåede ikke forventet side. URL: '{page.Url}'. Fejltekst på siden: [{string.Join(" | ", alerts)}]");
        }
    }
}