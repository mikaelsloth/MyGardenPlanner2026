namespace MyGardenPlanner2026.Tests.UI.Internal_Tests;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public sealed class AuditLogTestDataTests
{
    [Fact]
    public void Entry_Defaults_AreUpdateOnSubscriptionTierWithoutValues()
    {
        var (id, userId, userEmail, ipAddress, action, entityName, entityId, oldValues, newValues, timestamp) =
            AuditLogTestData.Entry();

        id.Should().Be(1);
        userId.Should().Be("user-1");
        userEmail.Should().Be("user1@example.com");
        ipAddress.Should().Be("127.0.0.1");
        action.Should().Be(AuditAction.Update);
        entityName.Should().Be("SubscriptionTier");
        entityId.Should().Be("abc");
        oldValues.Should().BeNull();
        newValues.Should().BeNull();
        timestamp.Should().Be(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Entry_Overrides_AreApplied()
    {
        var (id, _, _, _, action, entityName, entityId, oldValues, newValues, _) = AuditLogTestData.Entry(
            7, AuditAction.Delete, "SubscriptionAddOn", "xyz", "{\"A\":1}", "{\"A\":2}");

        id.Should().Be(7);
        action.Should().Be(AuditAction.Delete);
        entityName.Should().Be("SubscriptionAddOn");
        entityId.Should().Be("xyz");
        oldValues.Should().Be("{\"A\":1}");
        newValues.Should().Be("{\"A\":2}");
    }

    [Fact]
    public void ExportJob_Defaults_AreCompletedCsvJobWithTwelveRows()
    {
        var (id, userId, format, status, _, _, _, expiresAt, rowCount, fileName, errorMessage, seenAt) =
            AuditLogTestData.ExportJob();

        id.Should().NotBeEmpty();
        userId.Should().Be("user-1");
        format.Should().Be(AuditLogExportFormat.Csv);
        status.Should().Be(AuditLogExportJobStatus.Completed);
        expiresAt.Should().BeNull();
        rowCount.Should().Be(12);
        fileName.Should().Be("audit-log-export.csv");
        errorMessage.Should().BeNull();
        seenAt.Should().BeNull();
    }

    [Fact]
    public void ExportJob_Overrides_AreApplied()
    {
        var expires = DateTimeOffset.UtcNow.AddHours(1);
        var seen = DateTimeOffset.UtcNow;

        var (_, _, _, status, _, _, _, expiresAt, rowCount, fileName, errorMessage, seenAt) =
            AuditLogTestData.ExportJob(
                AuditLogExportJobStatus.Failed, expires, seen, rowCount: null, fileName: null, errorMessage: "Fejl.");

        status.Should().Be(AuditLogExportJobStatus.Failed);
        expiresAt.Should().Be(expires);
        seenAt.Should().Be(seen);
        rowCount.Should().BeNull();
        fileName.Should().BeNull();
        errorMessage.Should().Be("Fejl.");
    }

    [Fact]
    public void EmptyFilter_HasNoSearchCriteria()
    {
        var filter = AuditLogTestData.EmptyFilter();

        filter.EntityName.Should().BeNull();
        filter.EntityId.Should().BeNull();
        filter.UserId.Should().BeNull();
        filter.UserEmail.Should().BeNull();
        filter.Action.Should().BeNull();
    }

    [Fact]
    public void EmptyFilter_WithExpression_SetsSingleCriterion()
    {
        var filter = AuditLogTestData.EmptyFilter() with { EntityName = "SubscriptionTier" };

        filter.EntityName.Should().Be("SubscriptionTier");
        filter.EntityId.Should().BeNull();
    }
}