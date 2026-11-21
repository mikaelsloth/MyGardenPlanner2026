namespace MyGardenPlanner2026.Tests.UI.Components.Account.Pages.Manage;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Components.Account.Pages.Manage;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class DeletePersonalDataTests : BunitContext
{
    private const string Password = "Rigtig123!";
    private const string UserEmail = "bruger@example.dk";

    private static readonly Guid GardenAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid GardenBId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly GardenTransferCandidateDto EditorCandidate =
        new("user-2", GardenAccessLevel.BedDesigner, AccessCategory.Editor);

    private static BlockingGardenDto BlockingGarden(Guid id, string name) => new(id, name, [EditorCandidate]);

    private (UserManager<ApplicationUser> UserManager, SignInManager<ApplicationUser> SignInManager, IAccountDeletionService AccountDeletion, ApplicationUser User) RegisterFakes(
        bool hasPassword = true, IReadOnlyList<BlockingGardenDto>? blockingGardens = null, int retentionDays = 30)
    {
        var (userManager, signInManager) = this.RegisterIdentityFakes();
        var user = new ApplicationUser { Id = "user-1" };
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.HasPasswordAsync(user).Returns(Task.FromResult(hasPassword));
        userManager.GetUserIdAsync(user).Returns(Task.FromResult("user-1"));
        userManager.GetEmailAsync(user).Returns(Task.FromResult<string?>(UserEmail));
        userManager.FindByIdAsync("user-2")
            .Returns(Task.FromResult<ApplicationUser?>(new ApplicationUser { Id = "user-2", Email = "other@example.dk" }));

        var accountDeletion = Substitute.For<IAccountDeletionService>();
        accountDeletion.GetBlockingGardensAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<BlockingGardenDto>>(blockingGardens ?? []));
        accountDeletion.DeleteUserDataAsync(
                "user-1", UserEmail, Arg.Any<IReadOnlyDictionary<Guid, GardenOwnershipResolution>>(), Arg.Any<CancellationToken>())
            .Returns(AccountDeletionResult.Completed());
        Services.AddSingleton(accountDeletion);
        Services.AddSingleton(Substitute.For<ILogger<DeletePersonalData>>());
        RegisterRestoreOptions(retentionDays);
        RegisterAntiforgery();

        return (userManager, signInManager, accountDeletion, user);
    }

    private void RegisterRestoreOptions(int retentionDays)
    {
        var restoreOptions = Substitute.For<IOptionsMonitor<GardenRestoreOptions>>();
        restoreOptions.CurrentValue.Returns(new GardenRestoreOptions { RetentionDays = retentionDays });
        Services.AddSingleton(restoreOptions);
    }

    private void RegisterAntiforgery() =>
    Services.AddSingleton<AntiforgeryStateProvider>(new FakeAntiforgeryStateProvider());

    private sealed class FakeAntiforgeryStateProvider : AntiforgeryStateProvider
    {
        public override AntiforgeryRequestToken? GetAntiforgeryToken() => new("token", "__RequestVerificationToken");
    }

    private static string ErrorText(IRenderedComponent<DeletePersonalData> cut) =>
        cut.Find(".status-message-text p").TextContent;

    // ---------- Rendering ----------

    [Fact]
    public void RendersPasswordField_WhenUserHasPassword()
    {
        RegisterFakes(hasPassword: true);
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();

        cut.FindAll("#Input\\.Password").Should().HaveCount(1);
    }

    [Fact]
    public void DoesNotRenderPasswordField_WhenUserHasNoPassword()
    {
        RegisterFakes(hasPassword: false);
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();

        cut.FindAll("#Input\\.Password").Should().BeEmpty();
    }

    [Fact]
    public void ShowsConfiguredRetentionDays()
    {
        RegisterFakes(retentionDays: 14);
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();

        cut.Markup.Should().Contain("14 dage");
    }

    [Fact]
    public void NoBlockingGardens_RendersNoGardenChoices()
    {
        RegisterFakes();
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();

        cut.FindAll("fieldset").Should().BeEmpty();
    }

    [Fact]
    public void BlockingGardens_RendersChoicePerGardenWithCandidatesAndDeleteOption()
    {
        RegisterFakes(blockingGardens: [BlockingGarden(GardenAId, "Have A"), BlockingGarden(GardenBId, "Have B")]);
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();

        cut.FindAll("fieldset legend").Select(l => l.TextContent.Trim()).Should().Equal("Have A", "Have B");
        cut.FindAll("#garden-choice-0 option").Select(o => o.TextContent.Trim()).Should().Equal(
            "Vælg…",
            "Overdrag ejerskab til other@example.dk (Redaktør, Bed Designer)",
            "Slet haven for alle medlemmer");
        cut.FindAll("#garden-choice-1").Should().HaveCount(1);
        cut.FindAll("#garden-confirm-0").Should().HaveCount(1);
    }

    [Fact]
    public void BlockingGardens_RendersGardenIdAsHiddenField()
    {
        RegisterFakes(blockingGardens: [BlockingGarden(GardenAId, "Have A")]);
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();

        cut.Find("input[name='Input.Gardens[0].GardenId']").GetAttribute("value").Should().Be(GardenAId.ToString());
    }

    // ---------- Submit ----------

    [Fact]
    public async Task Submit_WrongPassword_ShowsErrorAndDeletesNothing()
    {
        var (userManager, signInManager, accountDeletion, user) = RegisterFakes();
        userManager.CheckPasswordAsync(user, "forkert").Returns(Task.FromResult(false));
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();
        await cut.Find("#Input\\.Password").ChangeAsync("forkert");
        await cut.Find("#delete-user-form").SubmitAsync();

        cut.Markup.Should().Contain("Error: Adgangskoden er forkert.");
        await accountDeletion.DidNotReceive().DeleteUserDataAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<IReadOnlyDictionary<Guid, GardenOwnershipResolution>>(), Arg.Any<CancellationToken>());
        await userManager.DidNotReceive().DeleteAsync(Arg.Any<ApplicationUser>());
        await signInManager.DidNotReceive().SignOutAsync();
    }

    [Fact]
    public async Task Submit_BlockingGardenWithoutChoice_ShowsErrorAndDeletesNothing()
    {
        var (userManager, signInManager, accountDeletion, user) = RegisterFakes(blockingGardens: [BlockingGarden(GardenAId, "Have A")]);
        userManager.CheckPasswordAsync(user, Password).Returns(Task.FromResult(true));
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();
        await cut.Find("#Input\\.Password").ChangeAsync(Password);
        await cut.Find("#delete-user-form").SubmitAsync();

        ErrorText(cut).Should().Be("Error: Vælg, hvad der skal ske med haven 'Have A'.");
        await accountDeletion.DidNotReceive().DeleteUserDataAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<IReadOnlyDictionary<Guid, GardenOwnershipResolution>>(), Arg.Any<CancellationToken>());
        await userManager.DidNotReceive().DeleteAsync(Arg.Any<ApplicationUser>());
        await signInManager.DidNotReceive().SignOutAsync();
    }

    [Fact]
    public async Task Submit_DeleteForAllWithWrongName_ShowsErrorAndDeletesNothing()
    {
        var (userManager, signInManager, accountDeletion, user) = RegisterFakes(blockingGardens: [BlockingGarden(GardenAId, "Have A")]);
        userManager.CheckPasswordAsync(user, Password).Returns(Task.FromResult(true));
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();
        await cut.Find("#garden-choice-0").ChangeAsync("delete");
        await cut.Find("#garden-confirm-0").ChangeAsync("Forkert navn");
        await cut.Find("#Input\\.Password").ChangeAsync(Password);
        await cut.Find("#delete-user-form").SubmitAsync();

        ErrorText(cut).Should().Be("Error: Skriv havens navn 'Have A' for at bekræfte, at haven slettes for alle medlemmer.");
        await accountDeletion.DidNotReceive().DeleteUserDataAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<IReadOnlyDictionary<Guid, GardenOwnershipResolution>>(), Arg.Any<CancellationToken>());
        await signInManager.DidNotReceive().SignOutAsync();
    }

    [Fact]
    public async Task Submit_DeleteForAllWithMatchingName_SendsDeleteForAllResolution()
    {
        var (userManager, _, accountDeletion, user) = RegisterFakes(blockingGardens: [BlockingGarden(GardenAId, "Have A")]);
        userManager.CheckPasswordAsync(user, Password).Returns(Task.FromResult(true));
        userManager.DeleteAsync(user).Returns(Task.FromResult(IdentityResult.Success));
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();
        await cut.Find("#garden-choice-0").ChangeAsync("delete");
        await cut.Find("#garden-confirm-0").ChangeAsync("  have a ");
        await cut.Find("#Input\\.Password").ChangeAsync(Password);
        await cut.Find("#delete-user-form").SubmitAsync();

        await accountDeletion.Received(1).DeleteUserDataAsync(
            "user-1",
            UserEmail,
            Arg.Is<IReadOnlyDictionary<Guid, GardenOwnershipResolution>>(
                r => r.Count == 1 && r[GardenAId].Kind == GardenResolutionKind.DeleteForAll),
            Arg.Any<CancellationToken>());
        await userManager.Received(1).DeleteAsync(user);
    }

    [Fact]
    public async Task Submit_TransferChoice_SendsTransferResolution()
    {
        var (userManager, _, accountDeletion, user) = RegisterFakes(blockingGardens: [BlockingGarden(GardenAId, "Have A")]);
        userManager.CheckPasswordAsync(user, Password).Returns(Task.FromResult(true));
        userManager.DeleteAsync(user).Returns(Task.FromResult(IdentityResult.Success));
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();
        await cut.Find("#garden-choice-0").ChangeAsync("transfer:user-2");
        await cut.Find("#Input\\.Password").ChangeAsync(Password);
        await cut.Find("#delete-user-form").SubmitAsync();

        await accountDeletion.Received(1).DeleteUserDataAsync(
            "user-1",
            UserEmail,
            Arg.Is<IReadOnlyDictionary<Guid, GardenOwnershipResolution>>(
                r => r.Count == 1
                    && r[GardenAId].Kind == GardenResolutionKind.TransferOwnership
                    && r[GardenAId].NewOwnerUserId == "user-2"),
            Arg.Any<CancellationToken>());
        await userManager.Received(1).DeleteAsync(user);
    }

    [Fact]
    public async Task Submit_TwoBlockingGardens_SendsOneResolutionPerGarden()
    {
        var (userManager, _, accountDeletion, user) = RegisterFakes(
            blockingGardens: [BlockingGarden(GardenAId, "Have A"), BlockingGarden(GardenBId, "Have B")]);
        userManager.CheckPasswordAsync(user, Password).Returns(Task.FromResult(true));
        userManager.DeleteAsync(user).Returns(Task.FromResult(IdentityResult.Success));
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();
        await cut.Find("#garden-choice-0").ChangeAsync("transfer:user-2");
        await cut.Find("#garden-choice-1").ChangeAsync("delete");
        await cut.Find("#garden-confirm-1").ChangeAsync("Have B");
        await cut.Find("#Input\\.Password").ChangeAsync(Password);
        await cut.Find("#delete-user-form").SubmitAsync();

        await accountDeletion.Received(1).DeleteUserDataAsync(
            "user-1",
            UserEmail,
            Arg.Is<IReadOnlyDictionary<Guid, GardenOwnershipResolution>>(
                r => r.Count == 2
                    && r[GardenAId].Kind == GardenResolutionKind.TransferOwnership
                    && r[GardenBId].Kind == GardenResolutionKind.DeleteForAll),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Submit_ServiceStillBlocked_ShowsBlockedErrorAndDoesNotDeleteUser()
    {
        var (userManager, signInManager, accountDeletion, user) = RegisterFakes(blockingGardens: [BlockingGarden(GardenAId, "Have A")]);
        userManager.CheckPasswordAsync(user, Password).Returns(Task.FromResult(true));
        accountDeletion.DeleteUserDataAsync(
                "user-1", UserEmail, Arg.Any<IReadOnlyDictionary<Guid, GardenOwnershipResolution>>(), Arg.Any<CancellationToken>())
            .Returns(AccountDeletionResult.Blocked([
                new BlockingGardenDto(GardenAId, "Have A", [EditorCandidate]),
                new BlockingGardenDto(GardenBId, "Have B", [])]));
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();
        await cut.Find("#garden-choice-0").ChangeAsync("transfer:user-2");
        await cut.Find("#Input\\.Password").ChangeAsync(Password);
        await cut.Find("#delete-user-form").SubmitAsync();

        cut.Markup.Should().Contain("Error: Du er eneste ejer af haver, der deles med andre medlemmer:");
        await userManager.DidNotReceive().DeleteAsync(Arg.Any<ApplicationUser>());
        await signInManager.DidNotReceive().SignOutAsync();
    }

    [Fact]
    public async Task Submit_DeleteAsyncFails_ShowsErrorAndDoesNotSignOut()
    {
        var (userManager, signInManager, _, user) = RegisterFakes();
        userManager.CheckPasswordAsync(user, Password).Returns(Task.FromResult(true));
        userManager.DeleteAsync(user).Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Code = "Fejl" })));
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();
        await cut.Find("#Input\\.Password").ChangeAsync(Password);
        await cut.Find("#delete-user-form").SubmitAsync();

        cut.Markup.Should().Contain("Error: Din konto kunne ikke slettes. Prøv igen.");
        await signInManager.DidNotReceive().SignOutAsync();
    }

    [Fact]
    public async Task Submit_Success_DeletesDataThenUser_SignsOutAndRedirectsToFrontPage()
    {
        var (userManager, signInManager, accountDeletion, user) = RegisterFakes();
        userManager.CheckPasswordAsync(user, Password).Returns(Task.FromResult(true));
        userManager.DeleteAsync(user).Returns(Task.FromResult(IdentityResult.Success));
        var navMan = this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();
        await cut.Find("#Input\\.Password").ChangeAsync(Password);
        await cut.Find("#delete-user-form").SubmitAsync();

        await accountDeletion.Received(1).DeleteUserDataAsync(
            "user-1", UserEmail, Arg.Is<IReadOnlyDictionary<Guid, GardenOwnershipResolution>>(r => r.Count == 0), Arg.Any<CancellationToken>());
        await userManager.Received(1).DeleteAsync(user);
        await signInManager.Received(1).SignOutAsync();
        navMan.Uri.Should().Be(navMan.BaseUri);
    }

    [Fact]
    public async Task Submit_UserWithoutPassword_DeletesWithoutPasswordCheck()
    {
        var (userManager, _, _, user) = RegisterFakes(hasPassword: false);
        userManager.DeleteAsync(user).Returns(Task.FromResult(IdentityResult.Success));
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();
        await cut.Find("#delete-user-form").SubmitAsync();

        await userManager.DidNotReceive().CheckPasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
        await userManager.Received(1).DeleteAsync(user);
    }

    [Fact]
    public void UserNotFound_RedirectsToInvalidUser()
    {
        var (userManager, _) = this.RegisterIdentityFakes();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(null));
        Services.AddSingleton(Substitute.For<IAccountDeletionService>());
        Services.AddSingleton(Substitute.For<ILogger<DeletePersonalData>>());
        RegisterRestoreOptions(30);
        RegisterAntiforgery();
        var navMan = this.UseIdentityRedirectManager();

        this.RenderWithHttpContext<DeletePersonalData>();

        navMan.Uri.Should().Contain("Account/InvalidUser");
    }

    [Fact]
    public void RendersBackupDownloadForm()
    {
        RegisterFakes();
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();

        var form = cut.Find("#account-backup-form");
        form.GetAttribute("action").Should().Be("Account/Manage/DownloadAccountBackup");
        form.GetAttribute("method").Should().Be("post");
        form.QuerySelector("button")!.TextContent.Should().Contain("Download backup");
    }
}