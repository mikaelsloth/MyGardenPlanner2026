namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;
using System.ComponentModel.DataAnnotations;

public sealed class AuditLogTestDataTests
{
    [Fact]
    public void NewEntityName_HarTagSomPraefiksOgErUnikt()
    {
        var first = AuditLogTestData.NewEntityName("J1");
        var second = AuditLogTestData.NewEntityName("J1");

        first.Should().StartWith("J1-").And.HaveLength(11);
        first.Should().NotBe(second);
    }

    [Fact]
    public void NewUserEmail_ErGyldigOgUnik()
    {
        var first = AuditLogTestData.NewUserEmail();

        new EmailAddressAttribute().IsValid(first).Should().BeTrue();
        first.Should().EndWith("@test.dk").And.NotBe(AuditLogTestData.NewUserEmail());
    }

    [Fact]
    public void Row_SaetterAlleFelter()
    {
        var ts = new DateTimeOffset(2026, 10, 1, 23, 59, 59, TimeSpan.Zero);

        var row = AuditLogTestData.Row(
            "E", "id-1", AuditAction.Delete, ts, userId: "u", userEmail: "e@test.dk", ipAddress: "1.2.3.4",
            oldValues: "{}", newValues: null);

        row.EntityName.Should().Be("E");
        row.EntityId.Should().Be("id-1");
        row.Action.Should().Be(AuditAction.Delete);
        row.TimestampUtc.Should().Be(ts);
        row.UserId.Should().Be("u");
        row.UserEmail.Should().Be("e@test.dk");
        row.IpAddress.Should().Be("1.2.3.4");
        row.OldValues.Should().Be("{}");
        row.NewValues.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(AuditLogTestData.MaxBulkCount + 1)]
    public async Task SeedBulkAsync_UgyldigtAntal_Kaster(int count)
    {
        var spec = new AuditLogTestData.BulkSeed("E", "e@test.dk", count, AuditAction.Create, DateTimeOffset.UtcNow);

        var act = () => AuditLogTestData.SeedBulkAsync("Server=ukendt", spec);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}