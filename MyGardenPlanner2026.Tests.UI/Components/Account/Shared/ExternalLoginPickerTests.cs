namespace MyGardenPlanner2026.Tests.UI.Components.Account.Shared;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Core.Entities;
using NSubstitute;
using Xunit;

public class ExternalLoginPickerTests : BunitContext
{
    private static SignInManager<ApplicationUser> CreateSignInManager(IEnumerable<AuthenticationScheme> schemes)
    {
        var signInManager = IdentityTestDoubles.CreateSignInManager(IdentityTestDoubles.CreateUserManager());
        signInManager.GetExternalAuthenticationSchemesAsync().Returns(Task.FromResult(schemes));

        return signInManager;
    }

    [Fact]
    public void ExternalLoginPicker_NoProvidersConfigured_ShowsDanishInfoMessage()
    {
        Services.AddSingleton(CreateSignInManager([]));

        var cut = Render<ExternalLoginPicker>();

        cut.Markup.Should().Contain("ikke konfigureret");
        cut.FindAll("button").Should().BeEmpty();
    }

    [Fact]
    public void ExternalLoginPicker_ProvidersConfigured_RendersOneButtonPerProvider()
    {
        var scheme = new AuthenticationScheme("Google", "Google", typeof(IAuthenticationHandler));
        Services.AddSingleton(CreateSignInManager([scheme]));

        var cut = Render<ExternalLoginPicker>();

        cut.FindAll(".btn-row button.btn-secondary").Should().HaveCount(1);
        cut.Markup.Should().Contain("Google");
    }
}