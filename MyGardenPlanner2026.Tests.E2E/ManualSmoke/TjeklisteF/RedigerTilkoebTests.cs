namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteF;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste F3: tilkøb. Ny SystemAdmin redigerer prisen på to tilkøb på /admin/subscriptions, én række
/// ad gangen, med step-up ved første gem, og forsøger derefter at oprette et tilkøb med en eksisterende
/// type (afvises: ingen ny række). En anonym bruger ser de nye priser på tilkøbskortene på /pricing.
/// Tilkøb deles med A2 (tilkøbstælling og priser) og gendannes altid i finally via en ny circuit; en evt.
/// ekstra række (duplikatet blev ved en fejl oprettet) slettes først. OldPrice og navn er seed
/// (Prismatrix tabel 3) og kontrolleres før redigering. Skærmbilleder: Tilkoeb_AdminSubscriptions_Gem,
/// Tilkoeb_AdminSubscriptions_Duplikat og Tilkoeb_Pricing_Efter (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class RedigerTilkoebTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste F";
    private const string TestName = "Tilkoeb";

    private const string DuplicateName = "e2e-duplikat";
    private const AddOnType DuplicateType = AddOnType.BedforslagNiveau2;

    private sealed record AddOnChange(
        AddOnType Type, string Name, BillingCycle Cycle, decimal OldPrice, decimal NewPrice);

    private static readonly AddOnChange[] Changes =
    [
        new(AddOnType.BedforslagNiveau2, "Bedforslag (Niveau 2)", BillingCycle.Monthly, 15.00m, 17.50m),
        new(AddOnType.ArtefaktpakkeA, "Artefaktpakke A", BillingCycle.Annual, 48.00m, 55.00m),
    ];

    [Theory]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task RedigerTilkoeb_MedStepUp_ViserNyePriserPaaPricingAfviserDuplikatOgGendannes(int width)
    {
        var user = await fixture.SeedExtraUserAsync(RoleNames.SystemAdmin);
        var page = await AdminAddOnsPage.OpenAsync(fixture, user, width);

        await Assertions.Expect(AdminAddOnsPage.Rows(page)).ToHaveCountAsync(AdminAddOnsPage.SeededAddOnCount);

        foreach (var change in Changes)
        {
            (await AdminAddOnsPage.ReadNameAsync(page, change.Type)).Should().Be(change.Name);
            (await AdminAddOnsPage.ReadPriceAsync(page, change.Type, change.Cycle))
                .Should().Be(
                    change.OldPrice,
                    "{0} ({1}) skal have seed-prisen før testen; er den ændret, blev en tidligere kørsel ikke gendannet",
                    change.Type, change.Cycle);
        }

        try
        {
            // Gem er pr. række, og et gem nulstiller ugemte rækker: redigér og gem én ad gangen.
            foreach (var change in Changes)
            {
                await AdminAddOnsPage.SetPriceAsync(page, change.Type, change.Cycle, change.NewPrice);
                await AdminAddOnsPage.SaveAddOnAsync(page, user, change.Type, change.Name);
            }

            await Assertions.Expect(AdminAddOnsPage.Error(page)).ToHaveCountAsync(0);
            await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "AdminSubscriptions_Gem");

            // Duplikat-type afvises, og der oprettes ingen række.
            await AdminAddOnsPage.AddDuplicateAsync(page, user, DuplicateType, DuplicateName);
            await Assertions.Expect(AdminAddOnsPage.Rows(page)).ToHaveCountAsync(AdminAddOnsPage.SeededAddOnCount);
            await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "AdminSubscriptions_Duplikat");

            // Anonym bruger: nye priser på tilkøbskortene, og intet duplikat-kort.
            var pricing = await PricingMatrix.OpenAnonymousAsync(fixture, width);

            await Assertions.Expect(PricingAddOns.Cards(pricing))
                .ToHaveCountAsync(AdminAddOnsPage.SeededAddOnCount);
            await Assertions.Expect(pricing.GetByRole(AriaRole.Heading, new() { Name = DuplicateName }))
                .ToHaveCountAsync(0);

            foreach (var change in Changes)
            {
                await PricingAddOns.ExpectPriceAsync(pricing, change.Name, change.Cycle, change.NewPrice);
            }

            await ScreenshotRecorder.CaptureAsync(pricing, Chapter, Checklist, TestName, "Pricing_Efter");
        }
        finally
        {
            await RestoreAsync(user, width);
        }
    }

    /// <summary>
    /// Ny circuit (ny step-up): sletter en evt. ekstra række (først, så rækkerne kan findes entydigt),
    /// skriver seed-priserne tilbage og kontrollerer dem.
    /// </summary>
    private async Task RestoreAsync(SmokeTestUser user, int width)
    {
        var page = await AdminAddOnsPage.OpenAsync(fixture, user, width);

        if (await AdminAddOnsPage.Rows(page).CountAsync() > AdminAddOnsPage.SeededAddOnCount)
        {
            await AdminAddOnsPage.DeleteLastRowAsync(page, user);
        }

        await Assertions.Expect(AdminAddOnsPage.Rows(page)).ToHaveCountAsync(AdminAddOnsPage.SeededAddOnCount);

        foreach (var change in Changes)
        {
            await AdminAddOnsPage.SetPriceAsync(page, change.Type, change.Cycle, change.OldPrice);
            await AdminAddOnsPage.SaveAddOnAsync(page, user, change.Type, change.Name);
        }

        foreach (var change in Changes)
        {
            (await AdminAddOnsPage.ReadPriceAsync(page, change.Type, change.Cycle))
                .Should().Be(change.OldPrice);
        }
    }
}