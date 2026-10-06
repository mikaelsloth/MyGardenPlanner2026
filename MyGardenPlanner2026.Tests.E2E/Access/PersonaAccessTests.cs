namespace MyGardenPlanner2026.Tests.E2E.Access;

using FluentAssertions;
using Microsoft.Playwright;

[Collection(PlaywrightAppCollection.Name)]
public sealed class PersonaAccessTests(PlaywrightAppFixture fixture)
{
    [Theory]
    [InlineData(SmokeTestPersonas.Admin, E2ERoutes.AdminSubscriptions, "Administrer abonnementer")]
    [InlineData(SmokeTestPersonas.DataAdmin, E2ERoutes.AdminJitRequests, "JIT-adgang")]
    [InlineData(SmokeTestPersonas.PolicyAdmin, E2ERoutes.AdminSecurityPolicies, "Sikkerhedspolicies")]
    [InlineData(SmokeTestPersonas.Auditor, E2ERoutes.AdminAuditLog, "AuditLog")]
    public async Task AdminPersona_LoginMedToFactor_TilgaarSinAdminside(
        string persona, string path, string expectedHeading)
    {
        var page = await fixture.LoginAndGotoAsync(persona, path);

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = expectedHeading }))
            .ToBeVisibleAsync();
    }

    [Theory]
    [InlineData(SmokeTestPersonas.NoMfa, E2ERoutes.AdminSubscriptions)]
    [InlineData(SmokeTestPersonas.Plain, E2ERoutes.AdminJitRequests)]
    public async Task PersonaUdenToFactor_ForsoegerAdgang_RedirectesTilEnableAuthenticator(
        string persona, string path)
    {
        var page = await fixture.LoginAndGotoAsync(persona, path);

        page.Url.Should().EndWith(E2ERoutes.EnableAuthenticator);
    }

    [Fact]
    public async Task Requester_UdenAdminRolle_KanIkkeTilgaaJitAdgangssiden()
    {
        var page = await fixture.LoginAndGotoAsync(SmokeTestPersonas.Requester, E2ERoutes.AdminJitRequests);

        page.Url.Should().Contain(E2ERoutes.AccessDenied);
        await Assertions.Expect(page.GetByText("Du har ikke adgang til denne ressource."))
            .ToBeVisibleAsync();
    }
}