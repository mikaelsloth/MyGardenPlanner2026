namespace MyGardenPlanner2026.Tests.Unit.Services.AuditLog.ExportJobs;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Services.AuditLog.ExportJobs;
using NSubstitute;
using Xunit;

public sealed class AuditLogExportJobPolicyAdminServiceTests : TestDbContext
{
    private readonly ISecurityPolicyChangeSignal changeSignal = Substitute.For<ISecurityPolicyChangeSignal>();
    private readonly ISecurityAlertService alertService = Substitute.For<ISecurityAlertService>();
    private readonly AuditLogExportJobPolicyAdminService service;

    public AuditLogExportJobPolicyAdminServiceTests()
    {
        service = new AuditLogExportJobPolicyAdminService(CreateAdminDbContextFactory(), changeSignal, alertService);
    }

    private async Task SeedAsync(int retentionHours = 24, int maxActive = 3)
    {
        await using var context = CreateDbContext();
        await context.AuditLogExportJobPolicySettings.AddAsync(new AuditLogExportJobPolicySettings
        {
            RetentionHours = retentionHours,
            MaxActiveJobsPerUser = maxActive,
            XlsxMaxRows = 50_000
        });
        await context.SaveChangesAsync();
    }

    private async Task<AuditLogExportJobPolicySettings> LoadAsync()
    {
        await using var context = CreateDbContext();
        return await context.AuditLogExportJobPolicySettings.SingleAsync(TestContext.Current.CancellationToken);
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

        var result = await service.UpdateAsync(new AuditLogExportJobPolicyDto(72, 10, 50_000), "admin-1", TestContext.Current.CancellationToken);

        result.RetentionHours.Should().Be(72);
        result.MaxActiveJobsPerUser.Should().Be(10);
        (await service.GetAsync(TestContext.Current.CancellationToken)).Should().BeEquivalentTo(result);
    }

    [Fact]
    public async Task UpdateAsync_ValidValues_TriggersChangeSignalForAuditLogExportJobOptions()
    {
        await SeedAsync();

        await service.UpdateAsync(new AuditLogExportJobPolicyDto(72, 10, 50_000), "admin-1", TestContext.Current.CancellationToken);

        changeSignal.Received(1).TriggerChange<AuditLogExportJobOptions>();
    }

    [Fact]
    public async Task UpdateAsync_ValidValues_SendsPolicyChangedAlert()
    {
        await SeedAsync();

        await service.UpdateAsync(new AuditLogExportJobPolicyDto(72, 10, 50_000), "admin-1", TestContext.Current.CancellationToken);

        await alertService.Received(1).AlertPolicyChangedAsync(
            "admin-1", "AuditLogExportJobPolicy", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0, 3, 50_000)]
    [InlineData(-1, 3, 50_000)]
    [InlineData(24, 0, 50_000)]
    [InlineData(24, -1, 50_000)]
    [InlineData(24, 3, -50_000)]
    public async Task UpdateAsync_NonPositiveValues_ThrowsArgumentOutOfRangeException(int retentionHours, int maxActive, int xlsxMaxRows)
    {
        await SeedAsync();

        var act = () => service.UpdateAsync(new AuditLogExportJobPolicyDto(retentionHours, maxActive, xlsxMaxRows), "admin-1");

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task UpdateAsync_NotSeeded_ThrowsInvalidOperationException()
    {
        var act = () => service.UpdateAsync(new AuditLogExportJobPolicyDto(24, 3, 50_000), "admin-1");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateAsync_MissingUserId_ThrowsArgumentException()
    {
        await SeedAsync();

        var act = () => service.UpdateAsync(new AuditLogExportJobPolicyDto(24, 3, 50_000), " ");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetAsync_ReturnsSeededValuesIncludingXlsxMaxRows()
    {
        await SeedAsync();

        var dto = await service.GetAsync(TestContext.Current.CancellationToken);

        dto.Should().Be(new AuditLogExportJobPolicyDto(24, 3, 50_000));
    }

    [Fact]
    public async Task UpdateAsync_ValidValues_PersistsReturnsSignalsAndAlerts()
    {
        await SeedAsync();

        var dto = await service.UpdateAsync(
            new AuditLogExportJobPolicyDto(48, 5, 75_000), "admin-1", TestContext.Current.CancellationToken);

        dto.Should().Be(new AuditLogExportJobPolicyDto(48, 5, 75_000));
        var row = await LoadAsync();
        row.RetentionHours.Should().Be(48);
        row.MaxActiveJobsPerUser.Should().Be(5);
        row.XlsxMaxRows.Should().Be(75_000);
        changeSignal.Received(1).TriggerChange<AuditLogExportJobOptions>();
        await alertService.Received(1).AlertPolicyChangedAsync(
            "admin-1", "AuditLogExportJobPolicy", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(IAuditLogExportService.HardRowLimit)]
    public async Task UpdateAsync_XlsxMaxRowsAtBoundary_IsAccepted(int xlsxMaxRows)
    {
        await SeedAsync();

        var dto = await service.UpdateAsync(
            new AuditLogExportJobPolicyDto(24, 3, xlsxMaxRows), "admin-1", TestContext.Current.CancellationToken);

        dto.XlsxMaxRows.Should().Be(xlsxMaxRows);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(IAuditLogExportService.HardRowLimit + 1)]
    public async Task UpdateAsync_XlsxMaxRowsOutOfRange_ThrowsAndChangesNothing(int xlsxMaxRows)
    {
        await SeedAsync();

        var act = () => service.UpdateAsync(
            new AuditLogExportJobPolicyDto(48, 5, xlsxMaxRows), "admin-1", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        var row = await LoadAsync();
        row.RetentionHours.Should().Be(24);
        row.XlsxMaxRows.Should().Be(50_000);
        changeSignal.DidNotReceive().TriggerChange<AuditLogExportJobOptions>();
        await alertService.DidNotReceive().AlertPolicyChangedAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_BlankUserId_ThrowsArgumentException()
    {
        await SeedAsync();

        var act = () => service.UpdateAsync(new AuditLogExportJobPolicyDto(24, 3, 50_000), " ");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void Options_DefaultXlsxMaxRows_MatchesPolicyDefault() =>
        new AuditLogExportJobOptions().XlsxMaxRows.Should().Be(AuditLogExportJobPolicySettings.DefaultXlsxMaxRows);
}