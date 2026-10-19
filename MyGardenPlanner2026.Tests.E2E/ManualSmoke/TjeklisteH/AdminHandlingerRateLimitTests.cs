namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteH;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using Ids = MyGardenPlanner2026.Tests.E2E.SecurityPolicyPage.FieldIds;

/// <summary>
/// Tjekliste H2: gem-spam i admin-editor giver rate-limit-fejl. Ny PolicyAdmin A sætter Admin-API PermitLimit
/// 100 → 10 (step-up, frisk limiter) og gemmer derefter ReAuth MaxAge 10 gange i hurtig rækkefølge
/// (16..20..15). Gem 2-10 viser ingen modal (vinduet er åbent), og ingen unik succesbesked, så hvert gem
/// bekræftes via alarm-mailen "Policy: ReAuthenticationPolicy". 11. gem (25) afvises af AdminActionGuard
/// før servicen: fejlen vises, ingen mail, og værdien forbliver 15. Gendannelse (PermitLimit 100, MaxAge 1)
/// sker i finally via en ny PolicyAdmin B, som åbnes FØR ændringen (A er blokeret op til ca. 60 sek.).
/// Skærmbilleder: RateLimiting_H2_&lt;bredde&gt; (efter 11. forsøg).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class AdminHandlingerRateLimitTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste H";
    private const string TestName = "RateLimiting";

    private const int OriginalPermit = 100;
    private const int ChangedPermit = 10;
    private const int OriginalMaxAge = PlaywrightAppFixture.ReAuthMaxAgeMinutes;
    private const int BlockedMaxAge = 25;

    private const string RateLimitedText = "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen.";

    private static readonly TimeSpan MailTimeout = TimeSpan.FromSeconds(5);

    // Præcis 10 forskellige skift i træk; sidste værdi (15) er den, der skal stå tilbage efter 11. forsøg.
    private static readonly int[] MaxAgeSequence = [16, 17, 18, 19, 20, 19, 18, 17, 16, 15];

    [Theory]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task GemSpam_ElvteForsoeg_ViserRateLimitFejlOgGemmerSkaermbillede(int width)
    {
        var userA = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);
        var userB = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);
        var userAId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, userA.Email);

        // B åbnes før ændringen: frisk circuit = step-up ved første gem, og B har egen kvote.
        var pageB = await SecurityPolicyPage.OpenAsync(
            fixture, userB, SecurityPolicyPage.AdminApiRateLimit, Ids.AdminApiPermit);
        var pageA = await SecurityPolicyPage.OpenAsync(
            fixture, userA, SecurityPolicyPage.AdminApiRateLimit, Ids.AdminApiPermit, width);
        await SecurityPolicyPage.ExpectFieldValueAsync(pageA, Ids.AdminApiPermit, OriginalPermit);

        try
        {
            // a. PermitLimit → 10 (step-up; limiteren genopbygges, så A's kvote er ny).
            await SecurityPolicyPage.SaveWithStepUpAsync(
                pageA, userA, Ids.AdminApiPermit, ChangedPermit,
                SecurityPolicyPage.AdminApiRateLimit.SavedText);

            // b. 10 hurtige gem af ReAuth MaxAge.
            await SecurityPolicyPage.SelectTabAsync(pageA, SecurityPolicyPage.ReAuth.Name);
            await SecurityPolicyPage.ExpectFieldValueAsync(pageA, Ids.ReAuthMaxAge, OriginalMaxAge);

            for (var i = 0; i < MaxAgeSequence.Length; i++)
            {
                await SecurityPolicyPage.SetFieldAsync(pageA, Ids.ReAuthMaxAge, MaxAgeSequence[i]);
                await SecurityPolicyPage.SaveButton(pageA).ClickAsync();

                // Mailen er eneste unikke signal pr. gem, og afventningen undgår, at editorens
                // genindlæsning efter gem overskriver næste indtastning.
                await ExpectReAuthMailCountAsync(pageA, userAId, i + 1);
            }

            await Assertions.Expect(StepUpModal.Dialog(pageA)).ToHaveCountAsync(0);
            await Assertions.Expect(SecurityPolicyPage.Error(pageA)).ToHaveCountAsync(0);

            // 11. forsøg: afvises før servicen.
            await SecurityPolicyPage.SetFieldAsync(pageA, Ids.ReAuthMaxAge, BlockedMaxAge);
            await SecurityPolicyPage.SaveButton(pageA).ClickAsync();

            await Assertions.Expect(SecurityPolicyPage.Error(pageA)).ToContainTextAsync(RateLimitedText);
            await Assertions.Expect(StepUpModal.Dialog(pageA)).ToHaveCountAsync(0);
            ReAuthMailCount(userAId).Should().Be(MaxAgeSequence.Length, "11. gem må ikke nå servicen");

            await ScreenshotRecorder.CaptureAsync(pageA, Chapter, Checklist, TestName, "H2");

            // Værdien er stadig den sidste gemte (15), ikke 25: fanehop genindlæser editoren.
            await SecurityPolicyPage.SelectTabAsync(pageA, SecurityPolicyPage.Jit.Name);
            await SecurityPolicyPage.SelectTabAsync(pageA, SecurityPolicyPage.ReAuth.Name);
            await SecurityPolicyPage.ExpectFieldValueAsync(pageA, Ids.ReAuthMaxAge, MaxAgeSequence[^1]);
        }
        finally
        {
            await RestoreAsync(pageB, userB);
        }
    }

    private int ReAuthMailCount(string userId) =>
        SecurityAlerts.CountPolicyChangedMails(
            fixture.SmtpServer.Messages, userId, SecurityPolicyPage.ReAuth.PolicyName);

    private async Task ExpectReAuthMailCountAsync(IPage page, string userId, int expected)
    {
        var reached = await BlazorInteraction.WaitUntilAsync(
            () => Task.FromResult(ReAuthMailCount(userId) >= expected), MailTimeout);

        if (!reached)
        {
            var errors = await SecurityPolicyPage.Error(page).AllTextContentsAsync();

            throw new InvalidOperationException(
                $"Gem nr. {expected} sendte ingen policy-alarm inden for {MailTimeout}. " +
                $"URL: '{page.Url}'. Fejltekst på siden: [{string.Join(" | ", errors)}]");
        }

        ReAuthMailCount(userId).Should().Be(expected);
    }

    /// <summary>
    /// B's circuit er frisk (step-up ved første gem, uanset hvilken fane). Fanehop genindlæser editorerne
    /// med de aktuelle værdier; kun værdier, der afviger fra seed, skrives tilbage.
    /// </summary>
    private static async Task RestoreAsync(IPage page, SmokeTestUser user)
    {
        await RestoreFieldAsync(
            page, user, SecurityPolicyPage.AdminApiRateLimit, Ids.AdminApiPermit, OriginalPermit);
        await RestoreFieldAsync(
            page, user, SecurityPolicyPage.ReAuth, Ids.ReAuthMaxAge, OriginalMaxAge);

        // Kontrol efter genindlæsning.
        await OpenTabFreshAsync(page, SecurityPolicyPage.AdminApiRateLimit, Ids.AdminApiPermit);
        await SecurityPolicyPage.ExpectFieldValueAsync(page, Ids.AdminApiPermit, OriginalPermit);

        await OpenTabFreshAsync(page, SecurityPolicyPage.ReAuth, Ids.ReAuthMaxAge);
        await SecurityPolicyPage.ExpectFieldValueAsync(page, Ids.ReAuthMaxAge, OriginalMaxAge);
    }

    private static async Task RestoreFieldAsync(
        IPage page, SmokeTestUser user, SecurityPolicyPage.PolicyTab tab, string fieldId, int original)
    {
        await OpenTabFreshAsync(page, tab, fieldId);

        if (await SecurityPolicyPage.ReadFieldAsync(page, fieldId) == original)
        {
            return;
        }

        await SecurityPolicyPage.SetFieldAsync(page, fieldId, original);

        // Modalen vises kun ved B's første gem, så den håndteres kun, hvis den dukker op.
        await AdminSaveFlow.SaveWithStepUpAsync(
            page,
            user,
            SecurityPolicyPage.SaveButton(page),
            page.Locator(".status-success").Filter(new() { HasText = tab.SavedText }),
            SecurityPolicyPage.Error(page),
            $"Gendannelse af {tab.PolicyName}");
    }

    /// <summary>Fanehop via JIT-fanen genindlæser fanens editor, så den viser de aktuelle værdier.</summary>
    private static async Task OpenTabFreshAsync(IPage page, SecurityPolicyPage.PolicyTab tab, string fieldId)
    {
        await SecurityPolicyPage.SelectTabAsync(page, SecurityPolicyPage.Jit.Name);
        await SecurityPolicyPage.SelectTabAsync(page, tab.Name);
        await Assertions.Expect(SecurityPolicyPage.Field(page, fieldId)).ToBeVisibleAsync();
    }
}