namespace MyGardenPlanner2026.Tests.UI.Internal_Tests;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities;
using Xunit;

public sealed class IdentityTestDoublesTests : BunitContext
{
    [Fact]
    public void RegisterIdentityFakes_RegistersReturnedManagersInServices()
    {
        var (userManager, signInManager) = this.RegisterIdentityFakes();

        Services.GetRequiredService<UserManager<ApplicationUser>>().Should().BeSameAs(userManager);
        Services.GetRequiredService<SignInManager<ApplicationUser>>().Should().BeSameAs(signInManager);
    }

    [Fact]
    public void RegisterReAuthFakes_RegistersServicesAccessorAndLogger()
    {
        var reAuthenticationService = this.RegisterReAuthFakes<IdentityTestDoublesTests>();

        Services.GetRequiredService<IReAuthenticationService>().Should().BeSameAs(reAuthenticationService);
        Services.GetRequiredService<IReAuthFailureTracker>().Should().NotBeNull();
        Services.GetRequiredService<ILogger<IdentityTestDoublesTests>>().Should().NotBeNull();
        Services.GetRequiredService<ICurrentUserAccessor>().GetCurrent().Should().NotBeNull();
    }

    [Fact]
    public async Task SetupTwoFactorLoginUser_ConfiguresTwoFactorUserAndUserId()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        var signInManager = IdentityTestDoubles.CreateSignInManager(userManager);

        var user = IdentityTestDoubles.SetupTwoFactorLoginUser(userManager, signInManager, "user-9");

        user.Id.Should().Be("user-9");
        (await signInManager.GetTwoFactorAuthenticationUserAsync()).Should().BeSameAs(user);
        (await userManager.GetUserIdAsync(user)).Should().Be("user-9");
    }
}