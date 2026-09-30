namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

public sealed class AuditLogExportJobProcessorTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 9, 28, 12, 0, 0, TimeSpan.Zero);
    private static readonly JsonSerializerOptions JsonOptions = new() { Converters = { new JsonStringEnumConverter() } };

    private readonly SqliteConnection connection;
    private readonly ExportTestAdminDbFactory factory;

    public AuditLogExportJobProcessorTests()
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        factory = new ExportTestAdminDbFactory(connection);
        using var context = factory.CreateDbContext();
        context.Database.EnsureCreated();
    }

    public void Dispose() => connection.Dispose();

    private AuditLogExportJobProcessor CreateProcessor(
        FakeExportService export, FakeQueryService? query = null, AuditLogExportJobOptions? options = null) =>
        new(
            factory,
            query ?? new FakeQueryService(5),
            export,
            new ExportTestOptionsMonitor<AuditLogExportJobOptions>(options ?? new AuditLogExportJobOptions()),
            new ExportTestTimeProvider(Now),
            NullLogger<AuditLogExportJobProcessor>.Instance);

    private static string FilterJson() =>
        JsonSerializer.Serialize(new AuditLogFilterDto(null, null, null, null, null, null, null), JsonOptions);

    private static AuditLogExportJob PendingJob(
        AuditLogExportFormat format = AuditLogExportFormat.Csv,
        DateTimeOffset? createdAt = null,
        string? filterJson = null) => new()
        {
            RequestedByUserId = "user-1",
            Format = format,
            FilterJson = filterJson ?? FilterJson(),
            Status = AuditLogExportJobStatus.Pending,
            CreatedAtUtc = createdAt ?? Now
        };

    private async Task SeedAsync(params AuditLogExportJob[] jobs)
    {
        await using var context = await factory.CreateDbContextAsync();
        await context.AuditLogExportJobs.AddRangeAsync(jobs);
        await context.SaveChangesAsync();
    }

    private async Task<AuditLogExportJob> LoadAsync(Guid id)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.AuditLogExportJobs.SingleAsync(j => j.Id == id);
    }

    [Fact]
    public async Task ProcessNextAsync_NoPendingJobs_ReturnsFalse()
    {
        var export = new FakeExportService();

        var result = await CreateProcessor(export).ProcessNextAsync(TestContext.Current.CancellationToken);

        result.Should().BeFalse();
        export.Calls.Should().Be(0);
    }

    [Fact]
    public async Task ProcessNextAsync_NonPendingJobs_AreIgnored()
    {
        var running = PendingJob();
        running.Status = AuditLogExportJobStatus.Running;
        var completed = PendingJob();
        completed.Status = AuditLogExportJobStatus.Completed;
        await SeedAsync(running, completed);

        var result = await CreateProcessor(new FakeExportService()).ProcessNextAsync(TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessNextAsync_PendingJob_CompletesWithFileAndMetadata()
    {
        var job = PendingJob();
        await SeedAsync(job);
        var export = new FakeExportService((stream, _) => stream.WriteAsync(new byte[] { 1, 2, 3 }, TestContext.Current.CancellationToken).AsTask());

        var result = await CreateProcessor(export, new FakeQueryService(5)).ProcessNextAsync(TestContext.Current.CancellationToken);

        result.Should().BeTrue();
        var row = await LoadAsync(job.Id);
        row.Status.Should().Be(AuditLogExportJobStatus.Completed);
        row.FileContent.Should().Equal(1, 2, 3);
        row.RowCount.Should().Be(5);
        row.ContentType.Should().Be("text/csv");
        row.FileName.Should().StartWith("audit-log-export-").And.EndWith(".csv");
        row.StartedAtUtc.Should().Be(Now);
        row.CompletedAtUtc.Should().Be(Now);
        row.ExpiresAtUtc.Should().Be(Now.AddHours(24));
        row.NotificationSeenAtUtc.Should().BeNull();
        row.ErrorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData(AuditLogExportFormat.Csv, "text/csv", ".csv")]
    [InlineData(AuditLogExportFormat.Json, "application/json", ".json")]
    [InlineData(AuditLogExportFormat.Xlsx, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx")]
    public async Task ProcessNextAsync_MapsFormatToContentTypeAndExtension(
        AuditLogExportFormat format, string contentType, string extension)
    {
        var job = PendingJob(format);
        await SeedAsync(job);
        var export = new FakeExportService((stream, _) => stream.WriteAsync(new byte[] { 1 }, TestContext.Current.CancellationToken).AsTask());

        await CreateProcessor(export).ProcessNextAsync(TestContext.Current.CancellationToken);

        var row = await LoadAsync(job.Id);
        row.ContentType.Should().Be(contentType);
        row.FileName.Should().EndWith(extension);
        export.LastFormat.Should().Be(format);
    }

    [Fact]
    public async Task ProcessNextAsync_UsesConfiguredRetention()
    {
        var job = PendingJob();
        await SeedAsync(job);
        var export = new FakeExportService((stream, _) => stream.WriteAsync(new byte[] { 1 }, TestContext.Current.CancellationToken).AsTask());

        await CreateProcessor(export, options: new AuditLogExportJobOptions { RetentionHours = 72 }).ProcessNextAsync(TestContext.Current.CancellationToken);

        (await LoadAsync(job.Id)).ExpiresAtUtc.Should().Be(Now.AddHours(72));
    }

    [Fact]
    public async Task ProcessNextAsync_MultiplePending_ProcessesOldestFirst()
    {
        var older = PendingJob(createdAt: Now.AddHours(-2));
        var newer = PendingJob(createdAt: Now.AddHours(-1));
        await SeedAsync(newer, older);
        var export = new FakeExportService((stream, _) => stream.WriteAsync(new byte[] { 1 }, TestContext.Current.CancellationToken).AsTask());

        await CreateProcessor(export).ProcessNextAsync(TestContext.Current.CancellationToken);

        (await LoadAsync(older.Id)).Status.Should().Be(AuditLogExportJobStatus.Completed);
        (await LoadAsync(newer.Id)).Status.Should().Be(AuditLogExportJobStatus.Pending);
    }

    [Fact]
    public async Task ProcessNextAsync_ExportThrows_FailsJobWithGenericMessage_AndStoresNoFile()
    {
        var job = PendingJob();
        await SeedAsync(job);
        var export = new FakeExportService((_, _) => throw new InvalidOperationException("intern detalje"));

        var result = await CreateProcessor(export).ProcessNextAsync(TestContext.Current.CancellationToken);

        result.Should().BeTrue();
        var row = await LoadAsync(job.Id);
        row.Status.Should().Be(AuditLogExportJobStatus.Failed);
        row.ErrorMessage.Should().Be(AuditLogExportJobProcessor.GenericFailureMessage);
        row.ErrorMessage.Should().NotContain("intern detalje");
        row.FileContent.Should().BeNull();
        row.CompletedAtUtc.Should().Be(Now);
    }

    [Fact]
    public async Task ProcessNextAsync_RowCountExceedsHardLimit_FailsWithoutExporting()
    {
        var job = PendingJob();
        await SeedAsync(job);
        var export = new FakeExportService();
        var query = new FakeQueryService(IAuditLogExportService.HardRowLimit + 1);

        await CreateProcessor(export, query).ProcessNextAsync(TestContext.Current.CancellationToken);

        var row = await LoadAsync(job.Id);
        row.Status.Should().Be(AuditLogExportJobStatus.Failed);
        row.ErrorMessage.Should().Contain("overstiger");
        export.Calls.Should().Be(0);
    }

    [Fact]
    public async Task ProcessNextAsync_FileExceedsMaxSize_FailsAndStoresNoFile()
    {
        var job = PendingJob();
        await SeedAsync(job);
        var export = new FakeExportService((stream, _) => stream.WriteAsync(new byte[(1024 * 1024) + 1], TestContext.Current.CancellationToken).AsTask());

        await CreateProcessor(export, options: new AuditLogExportJobOptions { MaxFileSizeMegabytes = 1 }).ProcessNextAsync(TestContext.Current.CancellationToken);

        var row = await LoadAsync(job.Id);
        row.Status.Should().Be(AuditLogExportJobStatus.Failed);
        row.ErrorMessage.Should().Contain("1 MB");
        row.FileContent.Should().BeNull();
    }

    [Fact]
    public async Task ProcessNextAsync_UnreadableFilterJson_FailsWithoutExporting()
    {
        var job = PendingJob(filterJson: "ikke json");
        await SeedAsync(job);
        var export = new FakeExportService();

        await CreateProcessor(export).ProcessNextAsync(TestContext.Current.CancellationToken);

        (await LoadAsync(job.Id)).Status.Should().Be(AuditLogExportJobStatus.Failed);
        export.Calls.Should().Be(0);
    }

    [Fact]
    public async Task ProcessNextAsync_CancelledDuringExport_RequeuesJobAndRethrows()
    {
        var job = PendingJob();
        await SeedAsync(job);
        using var cts = new CancellationTokenSource();
        var export = new FakeExportService(async (_, ct) =>
        {
            await cts.CancelAsync();
            ct.ThrowIfCancellationRequested();
        });

        var act = () => CreateProcessor(export).ProcessNextAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        var row = await LoadAsync(job.Id);
        row.Status.Should().Be(AuditLogExportJobStatus.Pending);
        row.StartedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task FailStaleRunningJobsAsync_FailsOnlyJobsRunningLongerThanThreshold()
    {
        var stale = PendingJob();
        stale.Status = AuditLogExportJobStatus.Running;
        stale.StartedAtUtc = Now.AddMinutes(-31);
        var noStart = PendingJob();
        noStart.Status = AuditLogExportJobStatus.Running;
        var fresh = PendingJob();
        fresh.Status = AuditLogExportJobStatus.Running;
        fresh.StartedAtUtc = Now.AddMinutes(-5);
        await SeedAsync(stale, noStart, fresh);

        var count = await CreateProcessor(new FakeExportService()).FailStaleRunningJobsAsync(TestContext.Current.CancellationToken);

        count.Should().Be(2);
        var staleRow = await LoadAsync(stale.Id);
        staleRow.Status.Should().Be(AuditLogExportJobStatus.Failed);
        staleRow.ErrorMessage.Should().Be(AuditLogExportJobProcessor.StaleJobMessage);
        (await LoadAsync(noStart.Id)).Status.Should().Be(AuditLogExportJobStatus.Failed);
        (await LoadAsync(fresh.Id)).Status.Should().Be(AuditLogExportJobStatus.Running);
    }

    [Fact]
    public async Task FailStaleRunningJobsAsync_NothingStale_ReturnsZero()
    {
        var count = await CreateProcessor(new FakeExportService()).FailStaleRunningJobsAsync(TestContext.Current.CancellationToken);

        count.Should().Be(0);
    }

    private sealed class FakeQueryService(int count) : IAuditLogQueryService
    {
        public Task<AuditLogQueryResultDto> SearchAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<int> CountAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default) =>
            Task.FromResult(count);

        public Task<IReadOnlyList<string>> GetDistinctEntityNamesAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class FakeExportService(Func<Stream, CancellationToken, Task>? behavior = null) : IAuditLogExportService
    {
        public int Calls { get; private set; }
        public AuditLogExportFormat? LastFormat { get; private set; }

        public Task ExportAsync(
            AuditLogFilterDto filter, AuditLogExportFormat format, Stream destination,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            LastFormat = format;
            return behavior?.Invoke(destination, cancellationToken) ?? Task.CompletedTask;
        }
    }

    [Fact]
    public async Task DeleteExpiredJobsAsync_CompletedAndExpired_DeletesRow()
    {
        var job = PendingJob();
        job.Status = AuditLogExportJobStatus.Completed;
        job.ExpiresAtUtc = Now.AddMinutes(-1);
        await SeedAsync(job);

        var count = await CreateProcessor(new FakeExportService()).DeleteExpiredJobsAsync(TestContext.Current.CancellationToken);

        count.Should().Be(1);
        await using var context = await factory.CreateDbContextAsync(TestContext.Current.CancellationToken);
        (await context.AuditLogExportJobs.AnyAsync(j => j.Id == job.Id, cancellationToken: TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteExpiredJobsAsync_CompletedButNotYetExpired_KeepsRow()
    {
        var job = PendingJob();
        job.Status = AuditLogExportJobStatus.Completed;
        job.ExpiresAtUtc = Now.AddHours(1);
        await SeedAsync(job);

        var count = await CreateProcessor(new FakeExportService()).DeleteExpiredJobsAsync(TestContext.Current.CancellationToken);

        count.Should().Be(0);
        (await LoadAsync(job.Id)).Should().NotBeNull();
    }

    [Theory]
    [InlineData(AuditLogExportJobStatus.Pending)]
    [InlineData(AuditLogExportJobStatus.Running)]
    [InlineData(AuditLogExportJobStatus.Failed)]
    public async Task DeleteExpiredJobsAsync_NonCompletedStatus_IsNeverDeleted_RegardlessOfExpiresAtUtc(AuditLogExportJobStatus status)
    {
        var job = PendingJob();
        job.Status = status;
        job.ExpiresAtUtc = Now.AddMinutes(-1);
        await SeedAsync(job);

        var count = await CreateProcessor(new FakeExportService()).DeleteExpiredJobsAsync(TestContext.Current.CancellationToken);

        count.Should().Be(0);
        (await LoadAsync(job.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteExpiredJobsAsync_NothingExpired_ReturnsZero()
    {
        var count = await CreateProcessor(new FakeExportService()).DeleteExpiredJobsAsync(TestContext.Current.CancellationToken);

        count.Should().Be(0);
    }
}