namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteD;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// R6.4: tastatur og fokus i step-up-modalen. Triggeren er "Gem" på /admin/security-policies (JIT-fanen).
/// Modalen annulleres altid, så ingen data ændres. Esc blokeret under verificering er ikke dækket her:
/// verificeringen er for hurtig til at kunne afbrydes pålideligt (dækkes af guarden i komponenten).
/// Ingen skærmbilleder.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class StepUpModalTastaturTests(PlaywrightAppFixture fixture)
{
    private static ILocator DialogWithFocus(IPage page) => page.Locator("[role='dialog']:focus-within");

    /// <summary>Frisk circuit: modalen vises ved første "Gem"-klik, uden at der ændres noget.</summary>
    private async Task<IPage> OpenModalAsync(int width)
    {
        var user = fixture.SmokeTestUsers[SmokeTestPersonas.PolicyAdmin];
        var page = await SecurityPolicyPage.OpenAsync(
            fixture, user, SecurityPolicyPage.Jit, SecurityPolicyPage.FieldIds.JitMin, width);

        await StepUpModal.OpenAsync(page, () => SecurityPolicyPage.SaveButton(page).ClickAsync());

        return page;
    }

    [Fact]
    public async Task Escape_AnnullererModalenOgReturnererFokusTilGemKnappen()
    {
        var page = await OpenModalAsync(Viewports.Desktop);

        // Initialt fokus er adgangskodefeltet. Esc må først sendes, når fokus er flyttet ind i modalen.
        await Assertions.Expect(StepUpModal.PasswordField(page)).ToBeFocusedAsync();

        await page.Keyboard.PressAsync("Escape");

        await Assertions.Expect(StepUpModal.Dialog(page)).ToBeHiddenAsync();
        // Klik fokuserer knappen i Chromium, så det er den, fokus skal returnere til.
        await Assertions.Expect(SecurityPolicyPage.SaveButton(page)).ToBeFocusedAsync();
    }

    [Theory]
    [InlineData(Viewports.Mobile)]
    [InlineData(Viewports.Desktop)]
    public async Task TabOgShiftTab_HolderFokusIModalen(int width)
    {
        var page = await OpenModalAsync(width);

        await Assertions.Expect(DialogWithFocus(page)).ToHaveCountAsync(1);

        // Admin-sider kræver 2FA, så modalen har 4 fokuserbare elementer (password, TOTP, Annullér, Bekræft).
        // Flere tryk end elementer sikrer, at fokus må krydse enderne.
        const int presses = 10;

        for (var i = 0; i < presses; i++)
        {
            await page.Keyboard.PressAsync("Tab");
            await Assertions.Expect(DialogWithFocus(page)).ToHaveCountAsync(1);
        }

        for (var i = 0; i < presses; i++)
        {
            await page.Keyboard.PressAsync("Shift+Tab");
            await Assertions.Expect(DialogWithFocus(page)).ToHaveCountAsync(1);
        }
    }

    [Fact]
    public async Task KlikPaaBaggrund_LukkerIkkeModalenOgTabHenterFokusTilbage()
    {
        var page = await OpenModalAsync(Viewports.Desktop);
        await Assertions.Expect(StepUpModal.PasswordField(page)).ToBeFocusedAsync();

        // Klik uden for dialogen (dialogen er højst 480 px bred og centreret).
        await page.Locator(".confirm-dialog-backdrop").ClickAsync(
            new() { Position = new Position { X = 10, Y = 10 } });

        // Negativ kontrol: vent, til en evt. lukning ville være sket.
        var closed = await BlazorInteraction.WaitUntilAsync(
            async () => !await StepUpModal.Dialog(page).IsVisibleAsync(), TimeSpan.FromSeconds(1));
        closed.Should().BeFalse("klik på baggrunden må ikke annullere en step-up (tilsigtet)");

        // Fokus er nu uden for dialogen; Tab skal trække det ind igen.
        await page.Keyboard.PressAsync("Tab");
        await Assertions.Expect(DialogWithFocus(page)).ToHaveCountAsync(1);
    }
}