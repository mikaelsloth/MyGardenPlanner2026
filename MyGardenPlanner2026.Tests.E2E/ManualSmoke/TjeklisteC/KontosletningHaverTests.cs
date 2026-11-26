namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteC;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Tjekliste C5: kontosletning for en bruger, der er eneste ejer i haver med andre medlemmer (R10.1a/R10.1b).
/// Brugere og haver seedes direkte i databasen (SeedExtraUserAsync, GardenTestData); selve sletningen sker
/// via /Account/Manage/DeletePersonalData. Forventede værdier er hardcodet. Gendannelse efter sletning
/// (sole-member-haver) testes i C6.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class KontosletningHaverTests(PlaywrightAppFixture fixture)
{
    private const string DeleteButtonName = "Slet data og luk min konto";
    private const string DeleteChoice = "delete";
    private const string TransferPrefix = "transfer:";
    private const string NoChoiceOption = "Vælg…";
    private const string DeleteOptionText = "Slet haven for alle medlemmer";

    private string ConnectionString => fixture.AppConnectionString;

    // ---------- Overdragelse ----------

    [Fact]
    public async Task SletKonto_EneEjerOverdragerEjerskab_HavenBeholdesMedNyEjer()
    {
        var owner = await NewUserAsync();
        var recipient = await NewUserAsync();
        var recipientId = await IdOfAsync(recipient);
        var garden = await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            NewName("Overdragelse"),
            [
                GardenMemberSeed.Owner(owner.Email),
                GardenMemberSeed.Member(recipient.Email, AccessCategory.Editor, GardenAccessLevel.Planlaegger),
            ]));

        var page = await OpenDeletePageAsync(owner);
        await Assertions.Expect(page.Locator("fieldset legend")).ToHaveTextAsync(garden.Name);
        await ChooseTransferAsync(page, 0, recipientId);
        await SubmitAsync(page, owner);
        await ExpectLandingPageAsync(page);

        (await GardenTestData.FindGardenAsync(ConnectionString, garden.Id))!.IsDeleted.Should().BeFalse();

        // Modtageren er ejer og Administrator; niveauet er uændret. Ejerens rækker er væk, og der er intet nyt entitlement.
        (await GardenTestData.GetMembershipsAsync(ConnectionString, garden.Id)).Should().BeEquivalentTo(
        [
            new MembershipSnapshot(recipientId, true, GardenAccessLevel.Planlaegger, AccessCategory.Administrator, false),
        ]);
        (await GardenTestData.GetEntitlementsAsync(ConnectionString, garden.Id)).Should().BeEmpty();

        await ExpectUserDeletedAsync(owner);

        var login = await LoginFlow.AttemptAsync(page, fixture.RootUri, owner);
        login.Should().NotBeNull();
        await Assertions.Expect(page.GetByText("Ugyldigt loginforsøg.")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task SletKonto_ValglisteMedFlereKandidater_HoejesteKategoriFoerst()
    {
        var owner = await NewUserAsync();
        var editor = await NewUserAsync();
        var admin = await NewUserAsync();
        await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            NewName("Kandidater"),
            [
                GardenMemberSeed.Owner(owner.Email),
                GardenMemberSeed.Member(editor.Email, AccessCategory.Editor),
                GardenMemberSeed.Member(admin.Email, AccessCategory.Administrator),
            ]));

        var page = await OpenDeletePageAsync(owner);

        var options = await Choice(page, 0).Locator("option").AllInnerTextsAsync();
        options.Should().HaveCount(4);
        options[0].Should().Be(NoChoiceOption);
        options[1].Should().StartWith($"Overdrag ejerskab til {admin.Email} (");
        options[2].Should().StartWith($"Overdrag ejerskab til {editor.Email} (");
        options[3].Should().Be(DeleteOptionText);
    }

    // ---------- Slet for alle ----------

    [Fact]
    public async Task SletKonto_SletHavenForAlle_SoftSletterHavenOgFjernerAndresAdgang()
    {
        var owner = await NewUserAsync();
        var other = await NewUserAsync();
        var ownerId = await IdOfAsync(owner);
        var garden = await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            NewName("Slet for alle"),
            [
                GardenMemberSeed.Owner(owner.Email),
                GardenMemberSeed.Member(other.Email, AccessCategory.Editor, withEntitlement: true),
            ]));

        var page = await OpenDeletePageAsync(owner);
        await ChooseDeleteForAllAsync(page, 0, garden.Name);
        await SubmitAsync(page, owner);
        await ExpectLandingPageAsync(page);

        var found = await GardenTestData.FindGardenAsync(ConnectionString, garden.Id);
        found.Should().NotBeNull();
        found!.IsDeleted.Should().BeTrue();
        found.DeletedAtUtc.Should().NotBeNull();
        found.DeletedByUserId.Should().Be(ownerId);
        found.DeletedByEmail.Should().BeNull();

        // Andres adgang slettes fysisk; ejerens rækker bevares soft-slettet til gendannelse.
        (await GardenTestData.GetMembershipsAsync(ConnectionString, garden.Id)).Should().BeEquivalentTo(
        [
            new MembershipSnapshot(ownerId, true, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, true),
        ]);
        (await GardenTestData.GetEntitlementsAsync(ConnectionString, garden.Id)).Should().BeEquivalentTo(
        [
            new EntitlementSnapshot(
                ownerId, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, BillingCycle.Annual, false, true),
        ]);

        await ExpectUserDeletedAsync(owner);
    }

    [Fact]
    public async Task SletKonto_HavenavnMedAndenSkrivemaadeOgMellemrum_Accepteres()
    {
        var owner = await NewUserAsync();
        var other = await NewUserAsync();
        var garden = await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            NewName("Skrivemaade"),
            [GardenMemberSeed.Owner(owner.Email), GardenMemberSeed.Member(other.Email)]));

        var page = await OpenDeletePageAsync(owner);
        await ChooseDeleteForAllAsync(page, 0, $"  {garden.Name.ToUpperInvariant()}  ");
        await SubmitAsync(page, owner);
        await ExpectLandingPageAsync(page);

        (await GardenTestData.FindGardenAsync(ConnectionString, garden.Id))!.IsDeleted.Should().BeTrue();
    }

    // ---------- Valideringsfejl ----------

    [Fact]
    public async Task SletKonto_UdenValgForHaven_ViserFejlOgSletterIntet()
    {
        var owner = await NewUserAsync();
        var other = await NewUserAsync();
        var garden = await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            NewName("Uden valg"),
            [GardenMemberSeed.Owner(owner.Email), GardenMemberSeed.Member(other.Email)]));

        var page = await OpenDeletePageAsync(owner);
        await SubmitAsync(page, owner);

        await Assertions.Expect(ErrorMessage(page))
            .ToContainTextAsync($"Vælg, hvad der skal ske med haven '{garden.Name}'.");
        await ExpectNothingDeletedAsync(owner, garden, expectedMembers: 2);
    }

    [Fact]
    public async Task SletKonto_ForkertHavenavnVedSletForAlle_ViserFejlOgSletterIntet()
    {
        var owner = await NewUserAsync();
        var other = await NewUserAsync();
        var garden = await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            NewName("Forkert navn"),
            [GardenMemberSeed.Owner(owner.Email), GardenMemberSeed.Member(other.Email)]));

        var page = await OpenDeletePageAsync(owner);
        await ChooseDeleteForAllAsync(page, 0, "et helt andet navn");
        await SubmitAsync(page, owner);

        await Assertions.Expect(ErrorMessage(page))
            .ToContainTextAsync($"Skriv havens navn '{garden.Name}' for at bekræfte, at haven slettes for alle medlemmer.");
        await ExpectNothingDeletedAsync(owner, garden, expectedMembers: 2);
    }

    // ---------- Flere haver og flere ejere ----------

    [Fact]
    public async Task SletKonto_ToHaverMedForskelligeValg_HaandteresHverForSig()
    {
        var owner = await NewUserAsync();
        var firstRecipient = await NewUserAsync();
        var secondMember = await NewUserAsync();
        var firstRecipientId = await IdOfAsync(firstRecipient);
        var ownerId = await IdOfAsync(owner);
        var first = await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            NewName("Alfa"),
            [GardenMemberSeed.Owner(owner.Email), GardenMemberSeed.Member(firstRecipient.Email)]));
        var second = await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            NewName("Bravo"),
            [GardenMemberSeed.Owner(owner.Email), GardenMemberSeed.Member(secondMember.Email)]));

        var page = await OpenDeletePageAsync(owner);
        await Assertions.Expect(page.Locator("fieldset legend")).ToHaveTextAsync([first.Name, second.Name]);
        await ChooseTransferAsync(page, 0, firstRecipientId);
        await ChooseDeleteForAllAsync(page, 1, second.Name);
        await SubmitAsync(page, owner);
        await ExpectLandingPageAsync(page);

        (await GardenTestData.FindGardenAsync(ConnectionString, first.Id))!.IsDeleted.Should().BeFalse();
        (await GardenTestData.GetMembershipsAsync(ConnectionString, first.Id)).Should().ContainSingle()
            .Which.Should().Be(new MembershipSnapshot(
                firstRecipientId, true, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, false));

        (await GardenTestData.FindGardenAsync(ConnectionString, second.Id))!.IsDeleted.Should().BeTrue();
        (await GardenTestData.GetMembershipsAsync(ConnectionString, second.Id)).Should().ContainSingle()
            .Which.UserId.Should().Be(ownerId);
    }

    [Fact]
    public async Task SletKonto_EnAfToEjere_BlokeresIkkeOgHavenBeholdesMedDenAndenEjer()
    {
        var owner = await NewUserAsync();
        var coOwner = await NewUserAsync();
        var coOwnerId = await IdOfAsync(coOwner);
        var garden = await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            NewName("To ejere"),
            [GardenMemberSeed.Owner(owner.Email), GardenMemberSeed.Owner(coOwner.Email)]));

        var page = await OpenDeletePageAsync(owner);
        await Assertions.Expect(DeleteButton(page)).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("fieldset")).ToHaveCountAsync(0);

        await SubmitAsync(page, owner);
        await ExpectLandingPageAsync(page);

        (await GardenTestData.FindGardenAsync(ConnectionString, garden.Id))!.IsDeleted.Should().BeFalse();
        (await GardenTestData.GetMembershipsAsync(ConnectionString, garden.Id)).Should().BeEquivalentTo(
        [
            new MembershipSnapshot(coOwnerId, true, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, false),
        ]);
        (await GardenTestData.GetEntitlementsAsync(ConnectionString, garden.Id)).Should().BeEquivalentTo(
        [
            new EntitlementSnapshot(
                coOwnerId, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, BillingCycle.Annual, false, false),
        ]);

        await ExpectUserDeletedAsync(owner);
    }

    // ---------- Hjælpere ----------

    private Task<SmokeTestUser> NewUserAsync() => fixture.SeedExtraUserAsync(twoFactorEnabled: false);

    private Task<string> IdOfAsync(SmokeTestUser user) =>
        SmokeTestDataSeeder.GetUserIdAsync(ConnectionString, user.Email);

    private static string NewName(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    private async Task<IPage> OpenDeletePageAsync(SmokeTestUser user)
    {
        var page = await fixture.LoginAndGotoAsync(user, E2ERoutes.DeletePersonalData, null);
        await SmokePageAssertions.ExpectPageAsync(page, ManagePageTexts.DeleteTitle, ManagePageTexts.ManageHeading);
        return page;
    }

    private static ILocator Choice(IPage page, int index) => page.Locator($"#garden-choice-{index}");

    private static ILocator Confirm(IPage page, int index) => page.Locator($"#garden-confirm-{index}");

    private static ILocator ErrorMessage(IPage page) => page.Locator(".status-message.status-danger[role=alert]");

    private static ILocator DeleteButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = DeleteButtonName, Exact = true });

    private static Task<IReadOnlyList<string>> ChooseTransferAsync(IPage page, int index, string newOwnerUserId) =>
        Choice(page, index).SelectOptionAsync(new SelectOptionValue { Value = TransferPrefix + newOwnerUserId });

    private static async Task ChooseDeleteForAllAsync(IPage page, int index, string confirmName)
    {
        await Choice(page, index).SelectOptionAsync(new SelectOptionValue { Value = DeleteChoice });
        await Confirm(page, index).FillAsync(confirmName);
    }

    private static async Task SubmitAsync(IPage page, SmokeTestUser user)
    {
        await page.GetByLabel("Adgangskode", new() { Exact = true }).FillAsync(user.Password);
        await DeleteButton(page).ClickAsync();
    }

    /// <summary>Succes redirecter til forsiden (RedirectTo("")).</summary>
    private static Task ExpectLandingPageAsync(IPage page) =>
        page.WaitForURLAsync(url => new Uri(url).AbsolutePath == "/");

    private async Task ExpectUserDeletedAsync(SmokeTestUser user)
    {
        var act = () => IdOfAsync(user);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private async Task ExpectNothingDeletedAsync(SmokeTestUser owner, SeededGarden garden, int expectedMembers)
    {
        (await IdOfAsync(owner)).Should().NotBeNullOrEmpty();
        (await GardenTestData.FindGardenAsync(ConnectionString, garden.Id))!.IsDeleted.Should().BeFalse();
        (await GardenTestData.GetMembershipsAsync(ConnectionString, garden.Id))
            .Should().HaveCount(expectedMembers).And.OnlyContain(m => !m.IsDeleted);
    }
}