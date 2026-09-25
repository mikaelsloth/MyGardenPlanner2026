namespace MyGardenPlanner2026.Tests.E2E;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Infrastructure;
using Xunit;

[Collection(PlaywrightAppCollection.Name)]
public sealed class PersonaAccessTests(PlaywrightAppFixture fixture)
{
    [Fact]
    public async Task Admin_LoginMedToFactor_TilgaarAbonnementssiden()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.RootUri}/admin/subscriptions");

        await LoginFlow.LoginAsync(page, fixture.SmokeTestUsers["Admin"]);

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Administrer abonnementer" }))
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task DataAdmin_LoginMedToFactor_TilgaarJitAdgangssiden()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.RootUri}/admin/jit-requests");

        await LoginFlow.LoginAsync(page, fixture.SmokeTestUsers["DataAdmin"]);

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "JIT-adgang" }))
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task PolicyAdmin_LoginMedToFactor_TilgaarSikkerhedspolicySiden()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.RootUri}/admin/security-policies");

        await LoginFlow.LoginAsync(page, fixture.SmokeTestUsers["PolicyAdmin"]);

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Sikkerhedspolicies" }))
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task Auditor_LoginMedToFactor_TilgaarAuditLogSiden()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.RootUri}/admin/audit-log");

        await LoginFlow.LoginAsync(page, fixture.SmokeTestUsers["Auditor"]);

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "AuditLog" }))
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task NoMfa_ForsoegerAdgangUdenToFactor_RedirectesTilEnableAuthenticator()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.RootUri}/admin/subscriptions");

        await LoginFlow.LoginAsync(page, fixture.SmokeTestUsers["NoMfa"]);

        page.Url.Should().EndWith("/Account/Manage/EnableAuthenticator");
    }

    [Fact]
    public async Task Plain_UdenRolle_RedirectesTilEnableAuthenticator()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.RootUri}/admin/jit-requests");

        await LoginFlow.LoginAsync(page, fixture.SmokeTestUsers["Plain"]);

        page.Url.Should().EndWith("/Account/Manage/EnableAuthenticator");
    }

    [Fact]
    public async Task Requester_UdenAdminRolle_KanIkkeTilgaaJitAdgangssiden()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{fixture.RootUri}/admin/jit-requests");

        await LoginFlow.LoginAsync(page, fixture.SmokeTestUsers["Requester"]);

        page.Url.Should().Contain("/Account/AccessDenied");
        await Assertions.Expect(page.GetByText("Du har ikke adgang til denne ressource."))
            .ToBeVisibleAsync();
    }
}