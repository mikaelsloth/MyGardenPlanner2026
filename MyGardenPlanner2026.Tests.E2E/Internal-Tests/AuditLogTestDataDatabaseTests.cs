namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>Kører mod en engangsdatabase med app-forbindelsen (i CI: mgp_app_user med INSERT/SELECT på AuditLogs).</summary>
public sealed class AuditLogTestDataDatabaseTests : IAsyncLifetime
{
    private E2ETestDatabase _database = default!;

    public async ValueTask InitializeAsync() => _database = await E2ETestDatabase.CreateAsync();

    public async ValueTask DisposeAsync()
    {
        if (_database is not null)
        {
            await _database.DisposeAsync();
        }
    }

    [Fact]
    public async Task SeedAsync_IndsaetterRaekkerOgCountAsyncTaellerPaaMarkoerer()
    {
        var entity = AuditLogTestData.NewEntityName("T");
        var email = AuditLogTestData.NewUserEmail();
        var ts = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);

        await AuditLogTestData.SeedAsync(_database.AppConnectionString,
        [
            AuditLogTestData.Row(entity, "1", AuditAction.Create, ts, userEmail: email, newValues: "{\"a\":1}"),
            AuditLogTestData.Row(entity, "2", AuditAction.Update, ts.AddSeconds(1), userEmail: email,
                oldValues: "{\"a\":1}", newValues: "{\"a\":2}"),
            AuditLogTestData.Row(AuditLogTestData.NewEntityName("T"), "3", AuditAction.Delete, ts),
        ]);

        (await AuditLogTestData.CountAsync(_database.AppConnectionString, entityName: entity)).Should().Be(2);
        (await AuditLogTestData.CountAsync(_database.AppConnectionString, userEmail: email)).Should().Be(2);
        (await AuditLogTestData.CountAsync(_database.AppConnectionString, entity, email)).Should().Be(2);
    }

    [Fact]
    public async Task SeedBulkAsync_IndsaetterPraecistDetOenskedeAntal()
    {
        var entity = AuditLogTestData.NewEntityName("B");
        var email = AuditLogTestData.NewUserEmail();

        await AuditLogTestData.SeedBulkAsync(_database.AppConnectionString, new AuditLogTestData.BulkSeed(
            entity, email, Count: 1_500, AuditAction.Update, new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero)));

        (await AuditLogTestData.CountAsync(_database.AppConnectionString, entity, email)).Should().Be(1_500);
    }

    [Fact]
    public async Task CountForEntityAsync_AfgraenserPaaEntitetEntitetsIdOgHandling()
    {
        var entity = AuditLogTestData.NewEntityName("C");
        var ts = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);

        await AuditLogTestData.SeedAsync(_database.AppConnectionString,
        [
            AuditLogTestData.Row(entity, "a", AuditAction.Create, ts),
            AuditLogTestData.Row(entity, "a", AuditAction.Update, ts.AddSeconds(1)),
            AuditLogTestData.Row(entity, "a", AuditAction.Update, ts.AddSeconds(2)),
            AuditLogTestData.Row(entity, "b", AuditAction.Update, ts.AddSeconds(3)),
        ]);

        var connection = _database.AppConnectionString;

        (await AuditLogTestData.CountForEntityAsync(connection, entity)).Should().Be(4);
        (await AuditLogTestData.CountForEntityAsync(connection, entity, "a")).Should().Be(3);
        (await AuditLogTestData.CountForEntityAsync(connection, entity, "a", AuditAction.Update)).Should().Be(2);
        (await AuditLogTestData.CountForEntityAsync(connection, entity, action: AuditAction.Update)).Should().Be(3);
        (await AuditLogTestData.CountForEntityAsync(connection, entity, "ukendt")).Should().Be(0);
    }
}