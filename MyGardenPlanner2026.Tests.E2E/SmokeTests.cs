namespace MyGardenPlanner2026.Tests.E2E;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Infrastructure;
using Xunit;

[Collection(PlaywrightAppCollection.Name)]
public sealed class SmokeTests(PlaywrightAppFixture fixture)
{
    [Fact]
    public async Task Forsiden_Loader_OgViserBrandNavn()
    {
        var page = await fixture.NewPageAsync();

        var response = await page.GotoAsync(fixture.RootUri);
        response.Should().NotBeNull();
        response!.Ok.Should().BeTrue();

        await Assertions.Expect(page.GetByText("MyGardenPlanner").First).ToBeVisibleAsync();
    }
}