namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteC;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using System.Globalization;
using static MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteC.DeletePersonalDataPage;

/// <summary>
/// Tjekliste C6: haver gemmes ved kontosletning og tilbydes ved genregistrering (R10.1b/R10.1e). Sole-member-haver
/// og haver valgt "slet for alle" soft-slettes med en gendannelsesbillet; haver, der overdrages eller har en
/// anden ejer, gemmes ikke. Gendannelsen i checkout testes i C7. Forventede værdier er hardcodet; billettens
/// hash udregnes uafhængigt af produktionskoden (GardenTestData.ExpectedOwnerEmailHash).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class GemteHaverTests(PlaywrightAppFixture fixture)
{
    private const string WelcomeTitle = "Kom i gang – MyGardenPlanner";
    private const string WelcomeHeading = "Velkommen til MyGardenPlanner";
    private const string RestoreBannerText = "Du har gemte haver, der kan gendannes";
    private const string RestoreLinkName = "Vælg abonnement og gendan";
    private const string SandboxButtonName = "Opret gratis prøveperiode";
    private const int DefaultRetentionDays = 30;

    private static readonly TimeSpan Tolerance = TimeSpan.FromMinutes(10);

    private string ConnectionString => fixture.AppConnectionString;

    [Fact]
    public async Task SletKonto_EnesteMedlem_HavenGemmesSoftSletMedGendannelsesbillet()
    {
        var user = await NewUserAsync();
        var userId = await IdOfAsync(user);
        var garden = await GardenTestData.SeedAsync(ConnectionString,
            new GardenSeed(NewName("Gemt"), [GardenMemberSeed.Owner(user.Email)]));

        var page = await DeletePersonalDataPage.OpenAsync(fixture, user);
        await Assertions.Expect(DeleteButton(page)).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("fieldset")).ToHaveCountAsync(0);
        await SubmitAsync(page, user);
        await ExpectLandingPageAsync(page);

        var found = await GardenTestData.FindGardenAsync(ConnectionString, garden.Id);
        found.Should().NotBeNull();
        found!.IsDeleted.Should().BeTrue();
        found.DeletedByUserId.Should().Be(userId);
        found.DeletedByEmail.Should().BeNull();

        (await GardenTestData.GetMembershipsAsync(ConnectionString, garden.Id)).Should().BeEquivalentTo(
        [
            new MembershipSnapshot(userId, true, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, true),
        ]);
        (await GardenTestData.GetEntitlementsAsync(ConnectionString, garden.Id)).Should().BeEquivalentTo(
        [
            new EntitlementSnapshot(
                userId, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, BillingCycle.Annual, false, true),
        ]);
        await ExpectTicketAsync(garden.Id, user.Email, userId);
    }

    [Fact]
    public async Task SletKonto_SletHavenForAlle_GemmerHavenMedBilletTilEjeren()
    {
        var owner = await NewUserAsync();
        var other = await NewUserAsync();
        var ownerId = await IdOfAsync(owner);
        var garden = await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            NewName("Slet for alle"),
            [GardenMemberSeed.Owner(owner.Email), GardenMemberSeed.Member(other.Email)]));

        var page = await DeletePersonalDataPage.OpenAsync(fixture, owner);
        await ChooseDeleteForAllAsync(page, 0, garden.Name);
        await SubmitAsync(page, owner);
        await ExpectLandingPageAsync(page);

        await ExpectTicketAsync(garden.Id, owner.Email, ownerId);
    }

    [Fact]
    public async Task SletKonto_HaverHvorBrugerenIkkeErEneste_GemmesIkke()
    {
        var owner = await NewUserAsync();
        var coOwner = await NewUserAsync();
        var member = await NewUserAsync();
        var coOwned = await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            NewName("To ejere"),
            [GardenMemberSeed.Owner(owner.Email), GardenMemberSeed.Owner(coOwner.Email)]));
        var transferred = await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            NewName("Overdrages"),
            [GardenMemberSeed.Owner(owner.Email), GardenMemberSeed.Member(member.Email)]));

        var page = await DeletePersonalDataPage.OpenAsync(fixture, owner);
        // Kun haven med blokering vises, og den er altid indeks 0.
        await Assertions.Expect(page.Locator("fieldset legend")).ToHaveTextAsync(transferred.Name);
        await ChooseTransferAsync(page, 0, await IdOfAsync(member));
        await SubmitAsync(page, owner);
        await ExpectLandingPageAsync(page);

        (await GardenTestData.FindGardenAsync(ConnectionString, coOwned.Id))!.IsDeleted.Should().BeFalse();
        (await GardenTestData.FindGardenAsync(ConnectionString, transferred.Id))!.IsDeleted.Should().BeFalse();
        (await GardenTestData.FindPendingRestoreAsync(ConnectionString, coOwned.Id)).Should().BeNull();
        (await GardenTestData.FindPendingRestoreAsync(ConnectionString, transferred.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Genregistrering_MedSammeEmail_ViserGemteHaverOgLinkTilCheckout()
    {
        var user = await NewUserAsync();
        var userId = await IdOfAsync(user);
        var first = await GardenTestData.SeedAsync(ConnectionString,
            new GardenSeed(NewName("Alfa"), [GardenMemberSeed.Owner(user.Email)]));
        var second = await GardenTestData.SeedAsync(ConnectionString,
            new GardenSeed(NewName("Bravo"), [GardenMemberSeed.Owner(user.Email)]));

        var page = await DeletePersonalDataPage.OpenAsync(fixture, user);
        await SubmitAsync(page, user);
        await ExpectLandingPageAsync(page);
        var ticket = await ExpectTicketAsync(first.Id, user.Email, userId);
        await ExpectTicketAsync(second.Id, user.Email, userId);

        await GardenRestoreFlow.RegisterAndLoginAsync(fixture, page, user.Email);
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.OnboardingWelcome}");
        await SmokePageAssertions.ExpectPageAsync(page, WelcomeTitle, WelcomeHeading);

        var banner = page.GetByText(RestoreBannerText);
        await Assertions.Expect(banner).ToContainTextAsync(first.Name);
        await Assertions.Expect(banner).ToContainTextAsync(second.Name);
        var expiry = ticket.ExpiresUtc.UtcDateTime.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        await Assertions.Expect(banner).ToContainTextAsync($"Den nærmeste frist er {expiry} (UTC).");
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = RestoreLinkName, Exact = true }))
            .ToHaveAttributeAsync("href", E2ERoutes.OnboardingCheckout);
    }

    [Fact]
    public async Task Genregistrering_MedAndenEmail_ViserIngenGemteHaver()
    {
        var user = await NewUserAsync();
        await GardenTestData.SeedAsync(ConnectionString,
            new GardenSeed(NewName("Privat"), [GardenMemberSeed.Owner(user.Email)]));

        var page = await DeletePersonalDataPage.OpenAsync(fixture, user);
        await SubmitAsync(page, user);
        await ExpectLandingPageAsync(page);

        await GardenRestoreFlow.RegisterAndLoginAsync(fixture, page, RegisteredUserFlow.NewEmail());
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.OnboardingWelcome}");
        await SmokePageAssertions.ExpectPageAsync(page, WelcomeTitle, WelcomeHeading);

        // Positiv kontrol før negativ assert: siden er renderet og interaktiv, før fraværet kontrolleres.
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = SandboxButtonName, Exact = true }))
            .ToBeVisibleAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Assertions.Expect(page.GetByText(RestoreBannerText)).ToHaveCountAsync(0);
    }

    // ---------- Hjælpere ----------

    private Task<SmokeTestUser> NewUserAsync() => fixture.SeedExtraUserAsync(twoFactorEnabled: false);

    private Task<string> IdOfAsync(SmokeTestUser user) =>
        SmokeTestDataSeeder.GetUserIdAsync(ConnectionString, user.Email);

    private static string NewName(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    private async Task<PendingRestoreSnapshot> ExpectTicketAsync(Guid gardenId, string email, string formerUserId)
    {
        var ticket = await GardenTestData.FindPendingRestoreAsync(ConnectionString, gardenId);

        ticket.Should().NotBeNull();
        ticket!.FormerUserId.Should().Be(formerUserId);
        ticket.OwnerEmailHash.Should().Be(GardenTestData.ExpectedOwnerEmailHash(email));
        ticket.DeletedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, Tolerance);
        ticket.ExpiresUtc.Should().BeCloseTo(DateTimeOffset.UtcNow.AddDays(DefaultRetentionDays), Tolerance);
        return ticket;
    }
}