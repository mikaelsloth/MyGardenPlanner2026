namespace MyGardenPlanner2026.Tests.Unit.Internal_Tests;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.Unit.Services.AuditLog;
using Xunit;

public sealed class AuditLogTestDataTests : TestDbContext
{
    [Fact]
    public void Log_WithoutArguments_UsesDefaults()
    {
        var log = AuditLogTestData.Log();

        log.EntityName.Should().Be("SubscriptionTier");
        log.EntityId.Should().Be("1");
        log.UserId.Should().Be("user-1");
        log.UserEmail.Should().Be("user1@example.com");
        log.Action.Should().Be(AuditAction.Update);
        log.TimestampUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Log_WithArguments_OverridesDefaults()
    {
        var timestamp = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        var log = AuditLogTestData.Log("SubscriptionAddOn", "abc", "user-9", "u9@example.com", AuditAction.Delete, timestamp);

        log.EntityName.Should().Be("SubscriptionAddOn");
        log.EntityId.Should().Be("abc");
        log.UserId.Should().Be("user-9");
        log.UserEmail.Should().Be("u9@example.com");
        log.Action.Should().Be(AuditAction.Delete);
        log.TimestampUtc.Should().Be(timestamp);
    }

    [Fact]
    public void EmptyFilter_HasNoFilterConditions()
    {
        var filter = AuditLogTestData.EmptyFilter;

        filter.EntityName.Should().BeNull();
        filter.EntityId.Should().BeNull();
        filter.UserId.Should().BeNull();
        filter.UserEmail.Should().BeNull();
        filter.Action.Should().BeNull();
        filter.FromUtc.Should().BeNull();
        filter.ToUtc.Should().BeNull();
    }

    [Fact]
    public async Task SeedAsync_PersistsAllRowsViaProvidedContextSource()
    {
        await AuditLogTestData.SeedAsync(
            CreateDbContext, AuditLogTestData.Log(entityId: "a"), AuditLogTestData.Log(entityId: "b"));

        await using var context = CreateDbContext();
        (await context.AuditLogs.CountAsync(TestContext.Current.CancellationToken)).Should().Be(2);
    }
}