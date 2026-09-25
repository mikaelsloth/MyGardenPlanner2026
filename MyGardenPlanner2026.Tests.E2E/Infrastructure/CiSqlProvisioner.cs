namespace MyGardenPlanner2026.Tests.E2E.Infrastructure;

using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

/// <summary>
/// Opretter database, admin-schema, mgp_app_user og mgp_admin_user i CI ved at afspille
/// de autoritative deployment-scripts fra Documentation/Database UÆNDRET — kun de to
/// placeholder-adgangskoder i 01-scriptet substitueres i hukommelsen, filen på disk
/// røres ikke. Køres KUN når E2ESqlEnvironment.IsCi er sand — lokalt bruger fixturen
/// Trusted_Connection for alle formål, og denne klasse rammes aldrig.
/// </summary>
public static partial class CiSqlProvisioner
{
    [GeneratedRegex(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex GoBatchSeparator { get; }

    public static async Task ProvisionAsync(string databaseName)
    {
        await CreateDatabaseAsync(databaseName);

        await RunScriptAsync("01-CreateAdminSchemaAndUsers.sql", script => script
            .Replace("CHANGE_ME_APP_STRONG_PW!", E2ESqlEnvironment.AppUserPassword())
            .Replace("CHANGE_ME_ADMIN_STRONG_PW!", E2ESqlEnvironment.AdminUserPassword()));

        await RunScriptAsync("04-RestrictAuditLogsToInsertOnly.sql", script => script);
    }

    private static async Task CreateDatabaseAsync(string databaseName)
    {
        await using var connection = new SqlConnection(E2ESqlEnvironment.MasterConnectionString());
        await connection.OpenAsync();

        await using var dropCommand = new SqlCommand(
            $"IF DB_ID('{databaseName}') IS NOT NULL BEGIN " +
            $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            $"DROP DATABASE [{databaseName}]; END", connection);
        await dropCommand.ExecuteNonQueryAsync();

        await using var createCommand = new SqlCommand($"CREATE DATABASE [{databaseName}]", connection);
        await createCommand.ExecuteNonQueryAsync();
    }

    private static async Task RunScriptAsync(string fileName, Func<string, string> transform)
    {
        var repoRoot = FindRepoRoot();
        var scriptPath = Path.Combine(repoRoot, "Documentation", "Database", fileName);
        var script = transform(await File.ReadAllTextAsync(scriptPath));

        await using var connection = new SqlConnection(E2ESqlEnvironment.MasterConnectionString());
        await connection.OpenAsync();

        foreach (var batch in GoBatchSeparator.Split(script))
        {
            var trimmed = batch.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            await using var command = new SqlCommand(trimmed, connection);
            await command.ExecuteNonQueryAsync();
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && dir.GetFiles("*.slnx").Length == 0)
        {
            dir = dir.Parent;
        }

        return dir?.FullName
            ?? throw new InvalidOperationException(
                "Kunne ikke finde repo-roden (ingen .slnx fundet opad fra testoutput-mappen).");
    }
}