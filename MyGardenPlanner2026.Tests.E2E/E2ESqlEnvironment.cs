namespace MyGardenPlanner2026.Tests.E2E;

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

    public static string MasterConnectionString() => MigrationConnectionString("master");

    public static string MigrationConnectionString(string databaseName) =>
        IsCi
            ? SqlAuthConnectionString(databaseName, "sa", SaPassword())
            : TrustedConnectionString(databaseName);

    /// <summary>Til appens ConnectionStrings:SqlExpressConnection (mgp_app_user i CI, samme forbindelse som migrations lokalt).</summary>
    public static string AppConnectionString(string databaseName) =>
        IsCi
            ? SqlAuthConnectionString(databaseName, "mgp_app_user", AppUserPassword())
            : MigrationConnectionString(databaseName);

    /// <summary>Til appens ConnectionStrings:AdminSqlExpressConnection (mgp_admin_user i CI, samme forbindelse som migrations lokalt).</summary>
    public static string AdminConnectionString(string databaseName) =>
        IsCi
            ? SqlAuthConnectionString(databaseName, "mgp_admin_user", AdminUserPassword())
            : MigrationConnectionString(databaseName);

    private static string SqlAuthConnectionString(string databaseName, string userId, string password) =>
        $"Server={SqlServerHost()};Database={databaseName};User Id={userId};Password={password};TrustServerCertificate=True";

    private static string TrustedConnectionString(string databaseName) =>
        $"Server={LocalSqlExpressServer};Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";

    public static string SqlServerHost() => RequireEnv("E2E_SQL_SERVER");
    public static string SaPassword() => RequireEnv("E2E_SQL_SA_PASSWORD");
    public static string AppUserPassword() => RequireEnv("E2E_SQL_APP_USER_PASSWORD");
    public static string AdminUserPassword() => RequireEnv("E2E_SQL_ADMIN_USER_PASSWORD");

    private static string RequireEnv(string name) =>
        Environment.GetEnvironmentVariable(name)
            ?? throw new InvalidOperationException($"Miljøvariablen '{name}' er ikke sat.");
}