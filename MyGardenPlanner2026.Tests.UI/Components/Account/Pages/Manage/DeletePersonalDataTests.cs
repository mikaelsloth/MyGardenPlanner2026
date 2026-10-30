namespace MyGardenPlanner2026.Tests.UI.Components.Account.Pages.Manage;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Components.Account.Pages.Manage;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class DeletePersonalDataTests : BunitContext
{
    private const string Password = "Rigtig123!";
    private const string UserEmail = "bruger@example.dk";

    private (UserManager<ApplicationUser> UserManager, SignInManager<ApplicationUser> SignInManager, IAccountDeletionService AccountDeletion, ApplicationUser User) RegisterFakes(bool hasPassword = true)
    {
        var (userManager, signInManager) = this.RegisterIdentityFakes();
        var user = new ApplicationUser { Id = "user-1" };
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.HasPasswordAsync(user).Returns(Task.FromResult(hasPassword));
        userManager.GetUserIdAsync(user).Returns(Task.FromResult("user-1"));
        userManager.GetEmailAsync(user).Returns(Task.FromResult<string?>(UserEmail));

        var accountDeletion = Substitute.For<IAccountDeletionService>();
        Services.AddSingleton(accountDeletion);
        Services.AddSingleton(Substitute.For<ILogger<DeletePersonalData>>());

        return (userManager, signInManager, accountDeletion, user);
    }

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
    public async Task Submit_WrongPassword_ShowsErrorAndDeletesNothing()
    {
        var (userManager, signInManager, accountDeletion, user) = RegisterFakes();
        userManager.CheckPasswordAsync(user, "forkert").Returns(Task.FromResult(false));
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();
        await cut.Find("#Input\\.Password").ChangeAsync("forkert");
        await cut.Find("form").SubmitAsync();

        cut.Markup.Should().Contain("Error: Incorrect password.");
        await accountDeletion.DidNotReceive().DeleteUserDataAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await userManager.DidNotReceive().DeleteAsync(Arg.Any<ApplicationUser>());
        await signInManager.DidNotReceive().SignOutAsync();
    }

    [Fact]
    public async Task Submit_SoleOwnerOfSharedGardens_ShowsBlockedErrorAndDoesNotDeleteUser()
    {
        var (userManager, signInManager, accountDeletion, user) = RegisterFakes();
        userManager.CheckPasswordAsync(user, Password).Returns(Task.FromResult(true));
        accountDeletion.DeleteUserDataAsync("user-1", UserEmail, Arg.Any<CancellationToken>())
            .Returns(AccountDeletionResult.Blocked(["Have A", "Have B"]));
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();
        await cut.Find("#Input\\.Password").ChangeAsync(Password);
        await cut.Find("form").SubmitAsync();

        cut.Markup.Should().Contain("Error: You are the only owner of gardens shared with other members: Have A, Have B.");
        await userManager.DidNotReceive().DeleteAsync(Arg.Any<ApplicationUser>());
        await signInManager.DidNotReceive().SignOutAsync();
    }

    [Fact]
    public async Task Submit_DeleteAsyncFails_ShowsErrorAndDoesNotSignOut()
    {
        var (userManager, signInManager, accountDeletion, user) = RegisterFakes();
        userManager.CheckPasswordAsync(user, Password).Returns(Task.FromResult(true));
        userManager.DeleteAsync(user).Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Code = "Fejl" })));
        accountDeletion.DeleteUserDataAsync("user-1", UserEmail, Arg.Any<CancellationToken>())
            .Returns(AccountDeletionResult.Completed());
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();
        await cut.Find("#Input\\.Password").ChangeAsync(Password);
        await cut.Find("form").SubmitAsync();

        cut.Markup.Should().Contain("Error: Your account could not be deleted. Please try again.");
        await signInManager.DidNotReceive().SignOutAsync();
    }

    [Fact]
    public async Task Submit_Success_DeletesDataThenUser_SignsOutAndRedirectsToFrontPage()
    {
        var (userManager, signInManager, accountDeletion, user) = RegisterFakes();
        userManager.CheckPasswordAsync(user, Password).Returns(Task.FromResult(true));
        userManager.DeleteAsync(user).Returns(Task.FromResult(IdentityResult.Success));
        accountDeletion.DeleteUserDataAsync("user-1", UserEmail, Arg.Any<CancellationToken>())
            .Returns(AccountDeletionResult.Completed());
        var navMan = this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();
        await cut.Find("#Input\\.Password").ChangeAsync(Password);
        await cut.Find("form").SubmitAsync();

        await accountDeletion.Received(1).DeleteUserDataAsync("user-1", UserEmail, Arg.Any<CancellationToken>());
        await userManager.Received(1).DeleteAsync(user);
        await signInManager.Received(1).SignOutAsync();
        navMan.Uri.Should().Be(navMan.BaseUri);
    }

    [Fact]
    public async Task Submit_UserWithoutPassword_DeletesWithoutPasswordCheck()
    {
        var (userManager, _, accountDeletion, user) = RegisterFakes(hasPassword: false);
        userManager.DeleteAsync(user).Returns(Task.FromResult(IdentityResult.Success));
        accountDeletion.DeleteUserDataAsync("user-1", UserEmail, Arg.Any<CancellationToken>())
            .Returns(AccountDeletionResult.Completed());
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<DeletePersonalData>();
        await cut.Find("form").SubmitAsync();

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
        var navMan = this.UseIdentityRedirectManager();

        this.RenderWithHttpContext<DeletePersonalData>();

        navMan.Uri.Should().Contain("Account/InvalidUser");
    }
}