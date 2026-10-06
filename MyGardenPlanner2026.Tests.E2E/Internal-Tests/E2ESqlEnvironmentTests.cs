namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;

/// <summary>
/// Låser formatet på connection strings, så refaktorering af E2ESqlEnvironment ikke
/// utilsigtet ændrer dem. Kræver ingen database. De tests der er specifikke for lokal
/// tilstand springes over i CI (E2E_SQL_SERVER er sat dér).
/// </summary>
public sealed class E2ESqlEnvironmentTests
{
    [Fact]
    public void MasterConnectionString_ErMigrationConnectionStringMedDatabaseMaster()
    {
        E2ESqlEnvironment.MasterConnectionString()
            .Should().Be(E2ESqlEnvironment.MigrationConnectionString("master"));
    }

    [Fact]
    public void MigrationConnectionString_IndeholderDatabasenavnet()
    {
        E2ESqlEnvironment.MigrationConnectionString("MinDb")
            .Should().Contain("Database=MinDb;");
    }

    [Fact]
    public void LokalTilstand_MigrationConnectionString_BrugerTrustedConnectionModSqlExpress()
    {
        Assert.SkipWhen(E2ESqlEnvironment.IsCi, "Kun relevant lokalt (E2E_SQL_SERVER er sat).");

        E2ESqlEnvironment.MigrationConnectionString("MinDb")
            .Should().Be(@"Server=.\SQLEXPRESS;Database=MinDb;Trusted_Connection=True;TrustServerCertificate=True");
    }

    [Fact]
    public void LokalTilstand_AppOgAdminConnectionString_ErIdentiskMedMigration()
    {
        Assert.SkipWhen(E2ESqlEnvironment.IsCi, "Kun relevant lokalt (E2E_SQL_SERVER er sat).");

        var expected = E2ESqlEnvironment.MigrationConnectionString("MinDb");

        E2ESqlEnvironment.AppConnectionString("MinDb").Should().Be(expected);
        E2ESqlEnvironment.AdminConnectionString("MinDb").Should().Be(expected);
    }

    [Fact]
    public void CiTilstand_AppOgAdminConnectionString_BrugerDeBegraensedeDatabasebrugere()
    {
        Assert.SkipUnless(E2ESqlEnvironment.IsCi, "Kun relevant i CI (E2E_SQL_SERVER er ikke sat).");

        E2ESqlEnvironment.AppConnectionString("MinDb").Should().Contain("User Id=mgp_app_user;");
        E2ESqlEnvironment.AdminConnectionString("MinDb").Should().Contain("User Id=mgp_admin_user;");
        E2ESqlEnvironment.MigrationConnectionString("MinDb").Should().Contain("User Id=sa;");
    }
}