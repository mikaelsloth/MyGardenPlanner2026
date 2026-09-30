namespace MyGardenPlanner2026.Tests.E2E;

using Xunit;

[CollectionDefinition(Name)]
public sealed class PlaywrightAppCollection : ICollectionFixture<PlaywrightAppFixture>
{
    public const string Name = "Playwright App";
}