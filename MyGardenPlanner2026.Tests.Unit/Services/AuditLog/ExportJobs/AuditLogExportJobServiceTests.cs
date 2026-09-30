namespace MyGardenPlanner2026.Tests.Unit.Services.AuditLog.ExportJobs;

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.Services;
using MyGardenPlanner2026.Infrastructure.Services.AuditLog.ExportJobs;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

public sealed class AuditLogExportJobServiceTests : IDisposable
{
    private const int DefaultMaxActive = 3;
    private static readonly DateTimeOffset Now = new(2026, 9, 28, 12, 0, 0, TimeSpan.Zero);
    private static readonly JsonSerializerOptions Options = new() { Converters = { new JsonStringEnumConverter() } };
    private readonly SqliteConnection connection;
    private readonly SqliteAdminFactory factory;
    private readonly AuditLogExportJobService service;

    public AuditLogExportJobServiceTests()
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        factory = new SqliteAdminFactory(connection);
        using (var context = factory.CreateDbContext())
        {
            context.Database.EnsureCreated();
        }

        service = new AuditLogExportJobService(
            factory,
            new FixedTimeProvider(Now),
            new ExportTestOptionsMonitor<AuditLogExportJobOptions>(new AuditLogExportJobOptions()),
            NullLogger<AuditLogExportJobService>.Instance);
    }

    public void Dispose() => connection.Dispose();

    private static AuditLogFilterDto Filter(string? entityName = null) =>
        new(entityName, null, null, null, null, null, null);

    private async Task SeedAsync(params AuditLogExportJob[] jobs)
    {
        await using var context = await factory.CreateDbContextAsync();
        await context.AuditLogExportJobs.AddRangeAsync(jobs);
        await context.SaveChangesAsync();
    }

    private static AuditLogExportJob Job(
        string userId,
        AuditLogExportJobStatus status = AuditLogExportJobStatus.Pending,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? expiresAt = null,
        DateTimeOffset? seenAt = null) => new()
        {
            RequestedByUserId = userId,
            Format = AuditLogExportFormat.Csv,
            FilterJson = "{}",
            Status = status,
            CreatedAtUtc = createdAt ?? Now,
            ExpiresAtUtc = expiresAt,
            NotificationSeenAtUtc = seenAt
        };

    [Fact]
    public async Task EnqueueAsync_ValidRequest_PersistsPendingJobWithFilter()
    {
        var dto = await service.EnqueueAsync("user-1", Filter("SubscriptionTier") with { PageNumber = 7 }, AuditLogExportFormat.Xlsx, TestContext.Current.CancellationToken);

        dto.Status.Should().Be(AuditLogExportJobStatus.Pending);
        dto.Format.Should().Be(AuditLogExportFormat.Xlsx);
        dto.RequestedByUserId.Should().Be("user-1");
        dto.CreatedAtUtc.Should().Be(Now);

        await using var context = await factory.CreateDbContextAsync(TestContext.Current.CancellationToken);
        var row = await context.AuditLogExportJobs.SingleAsync(TestContext.Current.CancellationToken);
        row.Id.Should().Be(dto.Id);
        row.FileContent.Should().BeNull();

        var storedFilter = JsonSerializer.Deserialize<AuditLogFilterDto>(row.FilterJson, Options);
        storedFilter!.EntityName.Should().Be("SubscriptionTier");
        storedFilter.PageNumber.Should().Be(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EnqueueAsync_MissingUserId_ThrowsArgumentException(string? userId)
    {
        var act = () => service.EnqueueAsync(userId!, Filter(), AuditLogExportFormat.Csv);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task EnqueueAsync_NullFilter_ThrowsArgumentNullException()
    {
        var act = () => service.EnqueueAsync("user-1", null!, AuditLogExportFormat.Csv);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EnqueueAsync_UndefinedFormat_ThrowsArgumentOutOfRangeException()
    {
        var act = () => service.EnqueueAsync("user-1", Filter(), (AuditLogExportFormat)99);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task EnqueueAsync_MaxActiveJobsReached_ThrowsInvalidOperationException()
    {
        for (var i = 0; i < DefaultMaxActive; i++)
        {
            await service.EnqueueAsync("user-1", Filter(), AuditLogExportFormat.Csv, TestContext.Current.CancellationToken);
        }

        var act = () => service.EnqueueAsync("user-1", Filter(), AuditLogExportFormat.Csv);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task EnqueueAsync_CompletedAndFailedJobsDoNotCountAsActive()
    {
        await SeedAsync(
            Job("user-1", AuditLogExportJobStatus.Completed),
            Job("user-1", AuditLogExportJobStatus.Failed),
            Job("user-1", AuditLogExportJobStatus.Completed));

        var dto = await service.EnqueueAsync("user-1", Filter(), AuditLogExportFormat.Csv, TestContext.Current.CancellationToken);

        dto.Status.Should().Be(AuditLogExportJobStatus.Pending);
    }

    [Fact]
    public async Task EnqueueAsync_ActiveJobsOfOtherUser_DoNotBlockThisUser()
    {
        for (var i = 0; i < DefaultMaxActive; i++)
        {
            await service.EnqueueAsync("user-1", Filter(), AuditLogExportFormat.Csv, TestContext.Current.CancellationToken);
        }

        var dto = await service.EnqueueAsync("user-2", Filter(), AuditLogExportFormat.Json, TestContext.Current.CancellationToken);

        dto.RequestedByUserId.Should().Be("user-2");
    }

    [Fact]
    public async Task GetJobsForUserAsync_ReturnsOnlyOwnJobs_NewestFirst()
    {
        var oldest = Job("user-1", createdAt: Now.AddHours(-3));
        var newest = Job("user-1", createdAt: Now.AddHours(-1));
        var middle = Job("user-1", createdAt: Now.AddHours(-2));
        var foreign = Job("user-2", createdAt: Now);
        await SeedAsync(oldest, newest, middle, foreign);

        var result = await service.GetJobsForUserAsync("user-1", TestContext.Current.CancellationToken);

        result.Select(j => j.Id).Should().Equal(newest.Id, middle.Id, oldest.Id);
    }

    [Fact]
    public async Task GetJobsForUserAsync_MapsCompletedJobFieldsWithoutFileContent()
    {
        var job = Job("user-1", AuditLogExportJobStatus.Completed);
        job.FileName = "audit-log-export.csv";
        job.RowCount = 42;
        job.FileContent = [1, 2, 3];
        await SeedAsync(job);

        var result = await service.GetJobsForUserAsync("user-1", TestContext.Current.CancellationToken);

        var dto = result.Should().ContainSingle().Subject;
        dto.FileName.Should().Be("audit-log-export.csv");
        dto.RowCount.Should().Be(42);
        dto.Status.Should().Be(AuditLogExportJobStatus.Completed);
    }

    [Fact]
    public async Task GetJobsForUserAsync_NoJobs_ReturnsEmptyList()
    {
        var result = await service.GetJobsForUserAsync("user-1", TestContext.Current.CancellationToken);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CountUnseenCompletedAsync_CountsOnlyOwnCompletedUnseenNonExpired()
    {
        await SeedAsync(
            Job("user-1", AuditLogExportJobStatus.Completed),                                   // tæller
            Job("user-1", AuditLogExportJobStatus.Completed, expiresAt: Now.AddHours(1)),       // tæller
            Job("user-1", AuditLogExportJobStatus.Completed, seenAt: Now.AddMinutes(-5)),       // set
            Job("user-1", AuditLogExportJobStatus.Completed, expiresAt: Now.AddMinutes(-1)),    // udløbet
            Job("user-1", AuditLogExportJobStatus.Pending),                                     // ikke færdig
            Job("user-1", AuditLogExportJobStatus.Failed),                                      // fejlet
            Job("user-2", AuditLogExportJobStatus.Completed));                                  // anden bruger

        var count = await service.CountUnseenCompletedAsync("user-1", TestContext.Current.CancellationToken);

        count.Should().Be(2);
    }

    [Fact]
    public async Task CountUnseenCompletedAsync_MissingUserId_ThrowsArgumentException()
    {
        var act = () => service.CountUnseenCompletedAsync(" ");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class SqliteAdminFactory(SqliteConnection connection) : IAdminDbContextFactory
    {
        private readonly DbContextOptions<PlannerDbContext> options =
            new DbContextOptionsBuilder<PlannerDbContext>()
                .UseSqlite(connection)
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

        public PlannerDbContext CreateDbContext() => new(options);

        public Task<PlannerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }

    [Fact]
    public async Task EnqueueAsync_ConfiguredLimitOfOne_BlocksSecondActiveJob()
    {
        var limitedService = new AuditLogExportJobService(
            factory,
            new FixedTimeProvider(Now),
            new ExportTestOptionsMonitor<AuditLogExportJobOptions>(new AuditLogExportJobOptions { MaxActiveJobsPerUser = 1 }),
            NullLogger<AuditLogExportJobService>.Instance);

        await limitedService.EnqueueAsync("user-1", Filter(), AuditLogExportFormat.Csv, TestContext.Current.CancellationToken);
        var act = () => limitedService.EnqueueAsync("user-1", Filter(), AuditLogExportFormat.Csv);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetDownloadableFileAsync_CompletedOwnJob_ReturnsFile_AndMarksSeen()
    {
        var job = Job("user-1", AuditLogExportJobStatus.Completed);
        job.FileName = "audit-log-export.csv";
        job.ContentType = "text/csv";
        job.FileContent = [1, 2, 3];
        await SeedAsync(job);

        var file = await service.GetDownloadableFileAsync(job.Id, "user-1", TestContext.Current.CancellationToken);

        file.Should().NotBeNull();
        file!.FileName.Should().Be("audit-log-export.csv");
        file.ContentType.Should().Be("text/csv");
        file.Content.Should().Equal(1, 2, 3);

        await using var context = await factory.CreateDbContextAsync(TestContext.Current.CancellationToken);
        var row = await context.AuditLogExportJobs.SingleAsync(j => j.Id == job.Id, cancellationToken: TestContext.Current.CancellationToken);
        row.NotificationSeenAtUtc.Should().Be(Now);
    }

    [Fact]
    public async Task GetDownloadableFileAsync_AlreadySeen_DoesNotOverwriteTimestamp()
    {
        var seenAt = Now.AddHours(-1);
        var job = Job("user-1", AuditLogExportJobStatus.Completed, seenAt: seenAt);
        job.FileName = "a.csv";
        job.ContentType = "text/csv";
        job.FileContent = [1];
        await SeedAsync(job);

        await service.GetDownloadableFileAsync(job.Id, "user-1", TestContext.Current.CancellationToken);

        await using var context = await factory.CreateDbContextAsync(TestContext.Current.CancellationToken);
        (await context.AuditLogExportJobs.SingleAsync(j => j.Id == job.Id, cancellationToken: TestContext.Current.CancellationToken)).NotificationSeenAtUtc.Should().Be(seenAt);
    }

    [Fact]
    public async Task GetDownloadableFileAsync_UnknownJob_ReturnsNull()
    {
        var file = await service.GetDownloadableFileAsync(Guid.NewGuid(), "user-1", TestContext.Current.CancellationToken);

        file.Should().BeNull();
    }

    [Fact]
    public async Task GetDownloadableFileAsync_ForeignUser_ReturnsNull()
    {
        var job = Job("user-2", AuditLogExportJobStatus.Completed);
        job.FileName = "a.csv";
        job.ContentType = "text/csv";
        job.FileContent = [1];
        await SeedAsync(job);

        var file = await service.GetDownloadableFileAsync(job.Id, "user-1", TestContext.Current.CancellationToken);

        file.Should().BeNull();
    }

    [Theory]
    [InlineData(AuditLogExportJobStatus.Pending)]
    [InlineData(AuditLogExportJobStatus.Running)]
    [InlineData(AuditLogExportJobStatus.Failed)]
    public async Task GetDownloadableFileAsync_NotCompleted_ReturnsNull(AuditLogExportJobStatus status)
    {
        var job = Job("user-1", status);
        await SeedAsync(job);

        var file = await service.GetDownloadableFileAsync(job.Id, "user-1", TestContext.Current.CancellationToken);

        file.Should().BeNull();
    }

    [Fact]
    public async Task GetDownloadableFileAsync_Expired_ReturnsNull()
    {
        var job = Job("user-1", AuditLogExportJobStatus.Completed, expiresAt: Now.AddMinutes(-1));
        job.FileName = "a.csv";
        job.ContentType = "text/csv";
        job.FileContent = [1];
        await SeedAsync(job);

        var file = await service.GetDownloadableFileAsync(job.Id, "user-1", TestContext.Current.CancellationToken);

        file.Should().BeNull();
    }

    [Fact]
    public async Task GetDownloadableFileAsync_CompletedButMissingFileContent_ReturnsNull()
    {
        var job = Job("user-1", AuditLogExportJobStatus.Completed);
        await SeedAsync(job);

        var file = await service.GetDownloadableFileAsync(job.Id, "user-1", TestContext.Current.CancellationToken);

        file.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetDownloadableFileAsync_MissingUserId_ThrowsArgumentException(string? userId)
    {
        var act = () => service.GetDownloadableFileAsync(Guid.NewGuid(), userId!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task MarkNotificationSeenAsync_OwnUnseenJob_SetsTimestamp_AndReturnsTrue()
    {
        var job = Job("user-1", AuditLogExportJobStatus.Completed);
        await SeedAsync(job);

        var result = await service.MarkNotificationSeenAsync(job.Id, "user-1", TestContext.Current.CancellationToken);

        result.Should().BeTrue();
        await using var context = await factory.CreateDbContextAsync(TestContext.Current.CancellationToken);
        (await context.AuditLogExportJobs.SingleAsync(j => j.Id == job.Id, cancellationToken: TestContext.Current.CancellationToken)).NotificationSeenAtUtc.Should().Be(Now);
    }

    [Fact]
    public async Task MarkNotificationSeenAsync_AlreadySeen_ReturnsTrue_WithoutChangingTimestamp()
    {
        var seenAt = Now.AddHours(-2);
        var job = Job("user-1", AuditLogExportJobStatus.Completed, seenAt: seenAt);
        await SeedAsync(job);

        var result = await service.MarkNotificationSeenAsync(job.Id, "user-1", TestContext.Current.CancellationToken);

        result.Should().BeTrue();
        await using var context = await factory.CreateDbContextAsync(TestContext.Current.CancellationToken);
        (await context.AuditLogExportJobs.SingleAsync(j => j.Id == job.Id, cancellationToken: TestContext.Current.CancellationToken)).NotificationSeenAtUtc.Should().Be(seenAt);
    }

    [Fact]
    public async Task MarkNotificationSeenAsync_UnknownJob_ReturnsFalse()
    {
        var result = await service.MarkNotificationSeenAsync(Guid.NewGuid(), "user-1", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task MarkNotificationSeenAsync_ForeignUser_ReturnsFalse()
    {
        var job = Job("user-2", AuditLogExportJobStatus.Completed);
        await SeedAsync(job);

        var result = await service.MarkNotificationSeenAsync(job.Id, "user-1", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task MarkNotificationSeenAsync_MissingUserId_ThrowsArgumentException(string? userId)
    {
        var act = () => service.MarkNotificationSeenAsync(Guid.NewGuid(), userId!);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}