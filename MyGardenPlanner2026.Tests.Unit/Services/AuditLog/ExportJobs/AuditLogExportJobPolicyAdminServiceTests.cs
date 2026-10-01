namespace MyGardenPlanner2026.Tests.Unit.Services.AuditLog.ExportJobs;

using FluentAssertions;
using Microsoft.Data.Sqlite;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Services.AuditLog.ExportJobs;
using MyGardenPlanner2026.Tests.Unit;
using Xunit;

public sealed class AuditLogExportJobPolicyAdminServiceTests : IDisposable
{
    private readonly SqliteConnection connection;
    private readonly SqliteAdminDbContextFactory factory;
    private readonly FakeSecurityPolicyChangeSignal changeSignal = new();
    private readonly FakeSecurityAlertService alertService = new();
    private readonly AuditLogExportJobPolicyAdminService service;

    public AuditLogExportJobPolicyAdminServiceTests()
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        factory = new SqliteAdminDbContextFactory(connection);
        using var context = factory.CreateDbContext();
        context.Database.EnsureCreated();

        service = new AuditLogExportJobPolicyAdminService(factory, changeSignal, alertService);
    }

    public void Dispose() => connection.Dispose();

    private async Task SeedAsync(int retentionHours = 24, int maxActive = 3)
    {
        await using var context = await factory.CreateDbContextAsync();
        await context.AuditLogExportJobPolicySettings.AddAsync(new AuditLogExportJobPolicySettings
        {
            RetentionHours = retentionHours,
            MaxActiveJobsPerUser = maxActive
        });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAsync_ReturnsSeededValues()
    {
        await SeedAsync(48, 5);

        var dto = await service.GetAsync(TestContext.Current.CancellationToken);

        dto.RetentionHours.Should().Be(48);
        dto.MaxActiveJobsPerUser.Should().Be(5);
    }

    [Fact]
    public async Task GetAsync_NotSeeded_ThrowsInvalidOperationException()
    {
        var act = () => service.GetAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateAsync_ValidValues_PersistsAndReturnsUpdatedDto()
    {
        await SeedAsync();

        var result = await service.UpdateAsync(new AuditLogExportJobPolicyDto(72, 10), "admin-1", TestContext.Current.CancellationToken);

        result.RetentionHours.Should().Be(72);
        result.MaxActiveJobsPerUser.Should().Be(10);
        (await service.GetAsync(TestContext.Current.CancellationToken)).Should().BeEquivalentTo(result);
    }

    [Fact]
    public async Task UpdateAsync_ValidValues_TriggersChangeSignalForAuditLogExportJobOptions()
    {
        await SeedAsync();

        await service.UpdateAsync(new AuditLogExportJobPolicyDto(72, 10), "admin-1", TestContext.Current.CancellationToken);

        changeSignal.TriggeredTypes.Should().ContainSingle(t => t == typeof(AuditLogExportJobOptions));
    }

    [Fact]
    public async Task UpdateAsync_ValidValues_SendsPolicyChangedAlert()
    {
        await SeedAsync();

        await service.UpdateAsync(new AuditLogExportJobPolicyDto(72, 10), "admin-1", TestContext.Current.CancellationToken);

        alertService.PolicyChangedCalls.Should().ContainSingle(c => c.UserId == "admin-1" && c.PolicyName == "AuditLogExportJobPolicy");
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(-1, 3)]
    [InlineData(24, 0)]
    [InlineData(24, -1)]
    public async Task UpdateAsync_NonPositiveValues_ThrowsArgumentOutOfRangeException(int retentionHours, int maxActive)
    {
        await SeedAsync();

        var act = () => service.UpdateAsync(new AuditLogExportJobPolicyDto(retentionHours, maxActive), "admin-1");

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task UpdateAsync_NotSeeded_ThrowsInvalidOperationException()
    {
        var act = () => service.UpdateAsync(new AuditLogExportJobPolicyDto(24, 3), "admin-1");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateAsync_MissingUserId_ThrowsArgumentException()
    {
        await SeedAsync();

        var act = () => service.UpdateAsync(new AuditLogExportJobPolicyDto(24, 3), " ");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private sealed class FakeSecurityPolicyChangeSignal : ISecurityPolicyChangeSignal
    {
        public List<Type> TriggeredTypes { get; } = [];

        public void TriggerChange<TOptions>() where TOptions : class => TriggeredTypes.Add(typeof(TOptions));
    }

    private sealed class FakeSecurityAlertService : ISecurityAlertService
    {
        public List<(string UserId, string PolicyName)> PolicyChangedCalls { get; } = [];

        public Task AlertFailedReAuthAsync(string userId, string ip, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task AlertJitRequestedAsync(string requesterId, string role, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task AlertPolicyChangedAsync(string userId, string policyName, CancellationToken cancellationToken = default)
        {
            PolicyChangedCalls.Add((userId, policyName));
            return Task.CompletedTask;
        }
    }
}