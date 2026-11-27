namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteC;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using System.Text.RegularExpressions;
using static MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteC.CheckoutPage;
using static MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteC.WelcomePage;

/// <summary>
/// Tjekliste C7: gendannelse af en gemt have i checkout (R10.1d/R10.1e). Hver test sletter først kontoen via UI
/// (haven er eneste medlem og gemmes), registrerer igen med samme e-mail og går via /onboarding/welcome til
/// checkout. Betalingen er simuleret. Havens tidligere niveau er BedDesigner/Administrator (ejer). Dækkende køb
/// = BedDesigner/Administrator; for lavt køb = Planlaegger/Editor. Oprydning af udløbne billetter testes i xUnit.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed partial class GendannelseTests(PlaywrightAppFixture fixture)
{
    private const GardenAccessLevel FormerLayer = GardenAccessLevel.BedDesigner;
    private const GardenAccessLevel TooLowLayer = GardenAccessLevel.Planlaegger;
    private const AccessCategory TooLowCategory = AccessCategory.Editor;
    private const string IgnoredGardenName = "Bruges ikke ved gendannelse";

    private sealed record Arrangement(
        SmokeTestUser User, string FormerUserId, string NewUserId, SeededGarden Garden, IPage Page);

    private string ConnectionString => fixture.AppConnectionString;

    // ---------- Gendannelse ----------

    [Fact]
    public async Task Gendan_MedDaekkendeKoeb_GendannerHavenMedNyEjer()
    {
        var a = await ArrangeAsync();
        await CheckoutPage.OpenViaWelcomeLinkAsync(fixture, a.Page);
        await Assertions.Expect(a.Page.GetByText(RestoreBannerText)).ToBeVisibleAsync();

        await ConfigureAsync(a.Page, IgnoredGardenName, FormerLayer, AccessCategory.Administrator);

        await Assertions.Expect(RestoreChoice(a.Page)).ToBeVisibleAsync();
        await Assertions.Expect(NewGardenRadio(a.Page)).ToBeCheckedAsync();
        await Assertions.Expect(RestoreRadio(a.Page, a.Garden.Name)).ToBeEnabledAsync();
        await Assertions.Expect(RestoreChoice(a.Page)).ToContainTextAsync("Kan gendannes til");
        await Assertions.Expect(RestoreChoice(a.Page)).Not.ToContainTextAsync("Kræver mindst");

        await RestoreRadio(a.Page, a.Garden.Name).CheckAsync();
        await Assertions.Expect(PaymentPage(a.Page)).ToContainTextAsync(a.Garden.Name);
        await Assertions.Expect(ConfirmButton(a.Page, RestoreConfirmLabel)).ToBeVisibleAsync();

        var gardenUpdatesBefore = await CountAuditAsync("Garden", a.Garden.Id.ToString());
        var membershipUpdatesBefore = await CountAuditAsync("GardenMembership");

        await ConfirmButton(a.Page, RestoreConfirmLabel).ClickAsync();
        await ExpectGardenAccessPageAsync(a.Page, a.Garden);

        await ExpectGardenRestoredAsync(a.Garden, a.NewUserId, FormerLayer, AccessCategory.Administrator);

        // R10.4: gendannelsen dokumenteres eksplicit (ExecuteUpdate går uden om interceptoren).
        (await CountAuditAsync("Garden", a.Garden.Id.ToString())).Should().Be(gardenUpdatesBefore + 1);
        (await CountAuditAsync("GardenMembership")).Should().BeGreaterThanOrEqualTo(membershipUpdatesBefore + 1);
    }

    [Fact]
    public async Task Gendan_MedForLavtKoeb_KraeverOpgraderingOgGendannesBagefter()
    {
        var a = await ArrangeAsync();
        await CheckoutPage.OpenViaWelcomeLinkAsync(fixture, a.Page);
        await ConfigureAsync(a.Page, IgnoredGardenName, TooLowLayer, TooLowCategory);

        await Assertions.Expect(RestoreRadio(a.Page, a.Garden.Name)).ToBeDisabledAsync();
        await Assertions.Expect(RestoreChoice(a.Page)).ToContainTextAsync("Kræver mindst");
        await Assertions.Expect(RestoreChoice(a.Page)).ToContainTextAsync("Dit nuværende valg dækker ikke haven.");
        await Assertions.Expect(NewGardenRadio(a.Page)).ToBeCheckedAsync();

        await ChangeSubscriptionButton(a.Page).ClickAsync();
        await Assertions.Expect(ActiveStep(a.Page)).ToHaveTextAsync(ConfigurationStep);
        await Assertions.Expect(GardenNameInput(a.Page)).ToHaveValueAsync(IgnoredGardenName);

        await ChooseSubscriptionAsync(a.Page, FormerLayer, AccessCategory.Administrator);
        await Assertions.Expect(RestoreRadio(a.Page, a.Garden.Name)).ToBeEnabledAsync();
        await RestoreRadio(a.Page, a.Garden.Name).CheckAsync();
        await ConfirmButton(a.Page, RestoreConfirmLabel).ClickAsync();
        await ExpectGardenAccessPageAsync(a.Page, a.Garden);

        await ExpectGardenRestoredAsync(a.Garden, a.NewUserId, FormerLayer, AccessCategory.Administrator);
    }

    // ---------- Opret en ny have i stedet ----------

    [Fact]
    public async Task OpretNyHave_FremForGendannelse_BilletOgGammelHaveBevares()
    {
        var a = await ArrangeAsync();

        var newGarden = await PurchaseNewGardenAsync(a, NewName("Ny have"), TooLowLayer, TooLowCategory);

        (await GardenTestData.GetMembershipsAsync(ConnectionString, newGarden.Id)).Should().BeEquivalentTo(
        [
            new MembershipSnapshot(a.NewUserId, true, TooLowLayer, TooLowCategory, false),
        ]);
        (await GardenTestData.GetEntitlementsAsync(ConnectionString, newGarden.Id)).Should().BeEquivalentTo(
        [
            new EntitlementSnapshot(a.NewUserId, TooLowLayer, TooLowCategory, BillingCycle.Annual, false, false),
        ]);

        (await GardenTestData.FindGardenAsync(ConnectionString, a.Garden.Id))!.IsDeleted.Should().BeTrue();
        (await GardenTestData.GetMembershipsAsync(ConnectionString, a.Garden.Id)).Should().BeEquivalentTo(
        [
            new MembershipSnapshot(a.FormerUserId, true, FormerLayer, AccessCategory.Administrator, true),
        ]);
        (await GardenTestData.GetEntitlementsAsync(ConnectionString, a.Garden.Id)).Should().BeEquivalentTo(
        [
            new EntitlementSnapshot(
                a.FormerUserId, FormerLayer, AccessCategory.Administrator, BillingCycle.Annual, false, true),
        ]);
        var ticket = await GardenTestData.FindPendingRestoreAsync(ConnectionString, a.Garden.Id);
        ticket.Should().NotBeNull();
        ticket!.FormerUserId.Should().Be(a.FormerUserId);
    }

    [Fact]
    public async Task OpretNyHaveFoerst_DerefterGendannesDenGamleHaveMedNytKoeb()
    {
        var a = await ArrangeAsync();
        var newGarden = await PurchaseNewGardenAsync(a, NewName("Ny have"), TooLowLayer, TooLowCategory);

        await CheckoutPage.OpenViaWelcomeLinkAsync(fixture, a.Page);
        await ConfigureAsync(a.Page, IgnoredGardenName, FormerLayer, AccessCategory.Administrator);
        await Assertions.Expect(RestoreRadio(a.Page, a.Garden.Name)).ToBeEnabledAsync();
        await RestoreRadio(a.Page, a.Garden.Name).CheckAsync();
        await ConfirmButton(a.Page, RestoreConfirmLabel).ClickAsync();
        await ExpectGardenAccessPageAsync(a.Page, a.Garden);

        await ExpectGardenRestoredAsync(a.Garden, a.NewUserId, FormerLayer, AccessCategory.Administrator);

        // Den nye have er uberørt.
        (await GardenTestData.FindGardenAsync(ConnectionString, newGarden.Id))!.IsDeleted.Should().BeFalse();
        (await GardenTestData.GetMembershipsAsync(ConnectionString, newGarden.Id)).Should().BeEquivalentTo(
        [
            new MembershipSnapshot(a.NewUserId, true, TooLowLayer, TooLowCategory, false),
        ]);
    }

    // ---------- Udløbet billet ----------

    [Fact]
    public async Task UdloebetBillet_TilbydesIkkeVedWelcomeEllerCheckout()
    {
        var user = await fixture.SeedExtraUserAsync(twoFactorEnabled: false);
        await GardenTestData.SeedPendingRestoreAsync(ConnectionString, new PendingRestoreSeed(
            NewName("Udloebet"), user.Email, FormerLayer, AccessCategory.Administrator, DateTimeOffset.UtcNow.AddDays(-1)));

        var page = await fixture.LoginAndGotoAsync(user, E2ERoutes.OnboardingWelcome, null);
        await SmokePageAssertions.ExpectPageAsync(page, WelcomeTitle, WelcomeHeading);
        // Positiv kontrol før negativ assert: siden er renderet og interaktiv, før fraværet kontrolleres.
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = SandboxButtonName, Exact = true }))
            .ToBeVisibleAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Assertions.Expect(page.GetByText(RestoreBannerText)).ToHaveCountAsync(0);

        await CheckoutPage.OpenAsync(fixture, page);
        await Assertions.Expect(page.GetByText(RestoreBannerText)).ToHaveCountAsync(0);

        await ConfigureAsync(page, "Min nye have", TooLowLayer, TooLowCategory);
        await Assertions.Expect(PaymentPage(page)).ToBeVisibleAsync();
        await Assertions.Expect(RestoreChoice(page)).ToHaveCountAsync(0);
    }

    // ---------- Hjælpere ----------

    /// <summary>Seeder en have, hvor brugeren er eneste medlem, sletter kontoen via UI og registrerer igen.</summary>
    private async Task<Arrangement> ArrangeAsync()
    {
        var user = await fixture.SeedExtraUserAsync(twoFactorEnabled: false);
        var formerUserId = await IdOfAsync(user);
        var garden = await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            NewName("Gendan"), [GardenMemberSeed.Owner(user.Email, FormerLayer)]));

        var page = await GardenRestoreFlow.DeleteAccountAndReRegisterAsync(fixture, user);
        var newUserId = await IdOfAsync(user);

        return new Arrangement(user, formerUserId, newUserId, garden, page);
    }

    /// <summary>Køber en ny have ("Opret en ny have" er standardvalget) og returnerer den, når access-siden vises.</summary>
    private async Task<SeededGarden> PurchaseNewGardenAsync(
        Arrangement a, string name, GardenAccessLevel level, AccessCategory category)
    {
        await CheckoutPage.OpenViaWelcomeLinkAsync(fixture, a.Page);
        await ConfigureAsync(a.Page, name, level, category);

        await Assertions.Expect(RestoreChoice(a.Page)).ToBeVisibleAsync();
        await Assertions.Expect(NewGardenRadio(a.Page)).ToBeCheckedAsync();
        await Assertions.Expect(ConfirmButton(a.Page, CreateConfirmLabel)).ToBeVisibleAsync();
        await ConfirmButton(a.Page, CreateConfirmLabel).ClickAsync();

        await a.Page.WaitForURLAsync(MyRegex);
        var garden = new SeededGarden(Guid.Parse(new Uri(a.Page.Url).AbsolutePath.Split('/')[2]), name);
        await ExpectGardenAccessPageAsync(a.Page, garden);
        return garden;
    }

    private static async Task ExpectGardenAccessPageAsync(IPage page, SeededGarden garden)
    {
        await page.WaitForURLAsync(url => new Uri(url).AbsolutePath == $"/gardens/{garden.Id}/access");
        await SmokePageAssertions.ExpectPageAsync(page, "Haveadgang – MyGardenPlanner", $"Haveadgang – {garden.Name}");
    }

    private async Task ExpectGardenRestoredAsync(
        SeededGarden garden, string newUserId, GardenAccessLevel layer, AccessCategory category)
    {
        var found = await GardenTestData.FindGardenAsync(ConnectionString, garden.Id);
        found.Should().NotBeNull();
        found!.IsDeleted.Should().BeFalse();
        found.DeletedAtUtc.Should().BeNull();
        found.DeletedByUserId.Should().BeNull();
        found.DeletedByEmail.Should().BeNull();

        // Ejerens medlemskab flyttes til den nye bruger med det købte niveau; det gamle entitlement erstattes.
        (await GardenTestData.GetMembershipsAsync(ConnectionString, garden.Id)).Should().BeEquivalentTo(
        [
            new MembershipSnapshot(newUserId, true, layer, category, false),
        ]);
        (await GardenTestData.GetEntitlementsAsync(ConnectionString, garden.Id)).Should().BeEquivalentTo(
        [
            new EntitlementSnapshot(newUserId, layer, category, BillingCycle.Annual, false, false),
        ]);
        (await GardenTestData.FindPendingRestoreAsync(ConnectionString, garden.Id)).Should().BeNull();
    }

    private Task<int> CountAuditAsync(string entityName, string? entityId = null) =>
        AuditLogTestData.CountForEntityAsync(ConnectionString, entityName, entityId, AuditAction.Update);

    private Task<string> IdOfAsync(SmokeTestUser user) =>
        SmokeTestDataSeeder.GetUserIdAsync(ConnectionString, user.Email);

    private static string NewName(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    [GeneratedRegex("/gardens/[0-9a-f-]{36}/access$", RegexOptions.IgnoreCase, "da-DK")]
    private static partial Regex MyRegex { get; }
}