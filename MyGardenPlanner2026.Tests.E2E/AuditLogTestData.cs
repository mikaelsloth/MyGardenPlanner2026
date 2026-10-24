namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Data.SqlClient;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Data;

/// <summary>
/// Seeder rækker direkte i admin.AuditLogs via app-forbindelsen. I CI har mgp_app_user kun INSERT og
/// SELECT på tabellen (script 04), og det er alt, der bruges her. Rækkerne kan ikke ryddes (DENY DELETE),
/// så hver test bruger unikke markører (EntityName/UserEmail) og asserterer aldrig på absolutte totaler.
/// </summary>
public static class AuditLogTestData
{
    public const int MaxBulkCount = 1_000_000;

    private const string InsertSql =
        "INSERT INTO [admin].[AuditLogs] " +
        "([UserId],[UserEmail],[IpAddress],[Action],[EntityName],[EntityId],[OldValues],[NewValues],[TimestampUtc]) " +
        "VALUES (@userId,@userEmail,@ip,@action,@entityName,@entityId,@oldValues,@newValues,@ts)";

    // Set-baseret: tal 1..@count fra en cross join, så 250.001 rækker indsættes i ét kald.
    private const string BulkInsertSql =
        ";WITH n AS (" +
        "  SELECT TOP (@count) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS i " +
        "  FROM sys.all_objects a CROSS JOIN sys.all_objects b) " +
        "INSERT INTO [admin].[AuditLogs] " +
        "([UserId],[UserEmail],[IpAddress],[Action],[EntityName],[EntityId],[OldValues],[NewValues],[TimestampUtc]) " +
        "SELECT NULL, @userEmail, N'127.0.0.1', @action, @entityName, CONVERT(nvarchar(100), n.i), " +
        "NULL, N'{\"seed\":true}', DATEADD(SECOND, @stepSeconds * (n.i - 1), @firstTs) FROM n";

    public sealed record BulkSeed(
        string EntityName,
        string UserEmail,
        int Count,
        AuditAction Action,
        DateTimeOffset FirstTimestampUtc,
        int StepSeconds = 1);

    /// <summary>Unikt entitetsnavn, fx "J1-3fa85f64" (maks. 200 tegn i kolonnen).</summary>
    public static string NewEntityName(string tag) => $"{tag}-{Guid.NewGuid():N}"[..(tag.Length + 9)];

    public static string NewUserEmail() => $"j-{Guid.NewGuid():N}@test.dk";

    public static AuditLog Row(
        string entityName,
        string entityId,
        AuditAction action,
        DateTimeOffset timestampUtc,
        string? userId = null,
        string? userEmail = null,
        string? ipAddress = null,
        string? oldValues = null,
        string? newValues = null) =>
        new()
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            TimestampUtc = timestampUtc,
            UserId = userId,
            UserEmail = userEmail,
            IpAddress = ipAddress,
            OldValues = oldValues,
            NewValues = newValues,
        };

    public static async Task SeedAsync(string connectionString, IEnumerable<AuditLog> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        foreach (var row in rows)
        {
            await using var command = new SqlCommand(InsertSql, connection, transaction);
            command.Parameters.Add(Text("@userId", row.UserId));
            command.Parameters.Add(Text("@userEmail", row.UserEmail));
            command.Parameters.Add(Text("@ip", row.IpAddress));
            command.Parameters.Add(new SqlParameter("@action", SqlDbType.Int) { Value = (int)row.Action });
            command.Parameters.Add(Text("@entityName", row.EntityName));
            command.Parameters.Add(Text("@entityId", row.EntityId));
            command.Parameters.Add(Text("@oldValues", row.OldValues));
            command.Parameters.Add(Text("@newValues", row.NewValues));
            command.Parameters.Add(new SqlParameter("@ts", SqlDbType.DateTimeOffset) { Value = row.TimestampUtc });

            await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    /// <summary>
    /// Indsætter Count rækker med EntityId 1..Count og tidspunkter FirstTimestampUtc + (i-1) * StepSeconds.
    /// Brug StepSeconds til at styre, hvilke rækker et Fra/Til-filter rammer.
    /// </summary>
    public static async Task SeedBulkAsync(string connectionString, BulkSeed spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentOutOfRangeException.ThrowIfLessThan(spec.Count, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(spec.Count, MaxBulkCount);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(BulkInsertSql, connection) { CommandTimeout = 300 };
        command.Parameters.Add(new SqlParameter("@count", SqlDbType.Int) { Value = spec.Count });
        command.Parameters.Add(Text("@userEmail", spec.UserEmail));
        command.Parameters.Add(new SqlParameter("@action", SqlDbType.Int) { Value = (int)spec.Action });
        command.Parameters.Add(Text("@entityName", spec.EntityName));
        command.Parameters.Add(new SqlParameter("@stepSeconds", SqlDbType.Int) { Value = spec.StepSeconds });
        command.Parameters.Add(new SqlParameter("@firstTs", SqlDbType.DateTimeOffset) { Value = spec.FirstTimestampUtc });

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>Antal rækker, der matcher entitetsnavn og/eller e-mail (kun til unikke markører).</summary>
    public static async Task<int> CountAsync(string connectionString, string? entityName = null, string? userEmail = null)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "SELECT COUNT(*) FROM [admin].[AuditLogs] " +
            "WHERE (@entityName IS NULL OR [EntityName] = @entityName) " +
            "AND (@userEmail IS NULL OR [UserEmail] = @userEmail)", connection);
        command.Parameters.Add(Text("@entityName", entityName));
        command.Parameters.Add(Text("@userEmail", userEmail));

        return (int)(await command.ExecuteScalarAsync())!;
    }

    private static SqlParameter Text(string name, string? value) =>
        new(name, SqlDbType.NVarChar, -1) { Value = (object?)value ?? DBNull.Value };
}