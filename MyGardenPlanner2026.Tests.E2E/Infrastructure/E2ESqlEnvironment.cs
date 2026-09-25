namespace MyGardenPlanner2026.Tests.E2E.Infrastructure;

/// <summary>
/// Løser hvilken SQL Server-instans og hvilke akkreditiver E2E-testene skal bruge.
/// Lokalt: Windows-integreret godkendelse mod .\SQLEXPRESS, med en GUID-navngivet
/// engangsdatabase pr. testkørsel (undgår kollision ved parallelle lokale kørsler),
/// og SAMME forbindelse bruges til både migrationer, app- og admin-forbindelse —
/// mgp_app_user/mgp_admin_user findes ikke lokalt.
///
/// I CI (E2E_SQL_SERVER er sat): SQL-godkendelse mod en mssql-servicecontainer, med
/// et fast databasenavn der matcher Documentation/Database-scriptenes hardkodede
/// "USE MyGardenPlanner2026;". Her adskilles migrations (sa), app-forbindelse
/// (mgp_app_user) og admin-forbindelse (mgp_admin_user), så vi tester den reelle
/// rettighedsadskillelse fra produktionsscriptene.
/// </summary>
public static class E2ESqlEnvironment
{
    private const string LocalSqlExpressServer = @".\SQLEXPRESS";
    public const string CiDatabaseName = "MyGardenPlanner2026";

    public static bool IsCi => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("E2E_SQL_SERVER"));

    public static string ResolveDatabaseName() =>
        IsCi ? CiDatabaseName : $"MyGardenPlanner2026_E2E_{Guid.NewGuid():N}";

    public static string MasterConnectionString() =>
        IsCi
            ? $"Server={SqlServerHost()};Database=master;User Id=sa;Password={SaPassword()};TrustServerCertificate=True"
            : $@"Server={LocalSqlExpressServer};Database=master;Trusted_Connection=True;TrustServerCertificate=True";

    public static string MigrationConnectionString(string databaseName) =>
        IsCi
            ? $"Server={SqlServerHost()};Database={databaseName};User Id=sa;Password={SaPassword()};TrustServerCertificate=True"
            : $@"Server={LocalSqlExpressServer};Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";

    /// <summary>Til appens ConnectionStrings:SqlExpressConnection (mgp_app_user i CI, samme forbindelse som migrations lokalt).</summary>
    public static string AppConnectionString(string databaseName) =>
        IsCi
            ? $"Server={SqlServerHost()};Database={databaseName};User Id=mgp_app_user;Password={AppUserPassword()};TrustServerCertificate=True"
            : MigrationConnectionString(databaseName);

    /// <summary>Til appens ConnectionStrings:AdminSqlExpressConnection (mgp_admin_user i CI, samme forbindelse som migrations lokalt).</summary>
    public static string AdminConnectionString(string databaseName) =>
        IsCi
            ? $"Server={SqlServerHost()};Database={databaseName};User Id=mgp_admin_user;Password={AdminUserPassword()};TrustServerCertificate=True"
            : MigrationConnectionString(databaseName);

    public static string SqlServerHost() => RequireEnv("E2E_SQL_SERVER");
    public static string SaPassword() => RequireEnv("E2E_SQL_SA_PASSWORD");
    public static string AppUserPassword() => RequireEnv("E2E_SQL_APP_USER_PASSWORD");
    public static string AdminUserPassword() => RequireEnv("E2E_SQL_ADMIN_USER_PASSWORD");

    private static string RequireEnv(string name) =>
        Environment.GetEnvironmentVariable(name)
            ?? throw new InvalidOperationException($"Miljøvariablen '{name}' er ikke sat.");
}