namespace MyGardenPlanner2026.Tests.UI.Components.Account.Shared;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using Xunit;

public class ManageShellTests : BunitContext
{
    private void RegisterFakes(params AuthenticationScheme[] externalSchemes)
    {
        var (_, signInManager) = this.RegisterIdentityFakes();
        signInManager.GetExternalAuthenticationSchemesAsync()
            .Returns(Task.FromResult<IEnumerable<AuthenticationScheme>>(externalSchemes));
    }

    private static List<string> LinkTexts(IRenderedComponent<ManageNavMenu> cut) =>
        [.. cut.FindAll("a.nav-link").Select(a => a.TextContent.Trim())];

    [Fact]
    public void ManageNavMenu_WithoutExternalSchemes_ShowsDanishLinksWithoutExternalLogins()
    {
        RegisterFakes();

        var cut = Render<ManageNavMenu>();

        LinkTexts(cut).Should().Equal(
            "Profil", "E-mail", "Adgangskode", "Totrinsbekræftelse", "Passkeys", "Personlige data");
    }

    [Fact]
    public void ManageNavMenu_WithExternalScheme_ShowsExternalLoginsLinkInPlace()
    {
        RegisterFakes(new AuthenticationScheme("Google", "Google", typeof(IAuthenticationHandler)));

        var cut = Render<ManageNavMenu>();

        LinkTexts(cut).Should().Equal(
            "Profil", "E-mail", "Adgangskode", "Eksterne logins", "Totrinsbekræftelse", "Passkeys", "Personlige data");
    }

    [Fact]
    public void ManageLayout_RendersDanishHeadings_AndBody()
    {
        RegisterFakes();

        var cut = Render<ManageLayout>(p => p
            .Add(x => x.Body, builder => builder.AddContent(0, "Sidens indhold")));

        cut.FindAll("h1").Should().ContainSingle().Which.TextContent.Should().Be("Administrer din konto");
        cut.Find("h2").TextContent.Should().Be("Skift dine kontoindstillinger");
        cut.Markup.Should().Contain("Sidens indhold");
    }
}