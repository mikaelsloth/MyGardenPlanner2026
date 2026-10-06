namespace MyGardenPlanner2026.Tests.E2E.Access;

using FluentAssertions;
using Microsoft.Playwright;

[Collection(PlaywrightAppCollection.Name)]
public sealed class PersonaAccessTests(PlaywrightAppFixture fixture)
{
    [Theory]
    [InlineData("Admin", "/admin/subscriptions", "Administrer abonnementer")]
    [InlineData("DataAdmin", "/admin/jit-requests", "JIT-adgang")]
    [InlineData("PolicyAdmin", "/admin/security-policies", "Sikkerhedspolicies")]
    [InlineData("Auditor", "/admin/audit-log", "AuditLog")]
    public async Task AdminPersona_LoginMedToFactor_TilgaarSinAdminside(
        string persona, string path, string expectedHeading)
    {
        var page = await fixture.LoginAndGotoAsync(persona, path);

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = expectedHeading }))
            .ToBeVisibleAsync();
    }

    [Theory]
    [InlineData("NoMfa", "/admin/subscriptions")]
    [InlineData("Plain", "/admin/jit-requests")]
    public async Task PersonaUdenToFactor_ForsoegerAdgang_RedirectesTilEnableAuthenticator(
        string persona, string path)
    {
        var page = await fixture.LoginAndGotoAsync(persona, path);

        page.Url.Should().EndWith("/Account/Manage/EnableAuthenticator");
    }

    [Fact]
    public async Task Requester_UdenAdminRolle_KanIkkeTilgaaJitAdgangssiden()
    {
        var page = await fixture.LoginAndGotoAsync("Requester", "/admin/jit-requests");

        page.Url.Should().Contain("/Account/AccessDenied");
        await Assertions.Expect(page.GetByText("Du har ikke adgang til denne ressource."))
            .ToBeVisibleAsync();
    }
}