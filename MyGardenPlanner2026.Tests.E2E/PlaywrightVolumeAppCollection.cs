namespace MyGardenPlanner2026.Tests.E2E;

/// <summary>
/// Egen collection (og dermed egen PlaywrightAppFixture: app, database og SMTP-sink) til tests med meget
/// store datasæt, så de ikke forurener den fælles database. Fx rammer G2c et baggrundseksport med tomt
/// filter og ville fejle, hvis databasen indeholdt over 250.000 auditlog-rækker.
/// </summary>
[CollectionDefinition(Name)]
public sealed class PlaywrightVolumeAppCollection : ICollectionFixture<PlaywrightAppFixture>
{
    public const string Name = "Playwright App Volume";
}