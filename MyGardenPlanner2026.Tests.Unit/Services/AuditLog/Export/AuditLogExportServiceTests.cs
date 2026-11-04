namespace MyGardenPlanner2026.Tests.Unit.Services.AuditLog.Export;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Services.AuditLog.Export;
using NSubstitute;
using System.Text;
using System.Text.Json;
using Xunit;
using static MyGardenPlanner2026.Tests.Unit.Services.AuditLog.AuditLogTestData;

public sealed class AuditLogExportServiceTests
{
    private readonly IAuditLogQueryService queryService = Substitute.For<IAuditLogQueryService>();
    private readonly AuditLogExportService sut;

    public AuditLogExportServiceTests()
    {
        sut = new AuditLogExportService(queryService);
    }

    private static AuditLogEntryDto Entry(long id = 1, string? oldValues = null) => new(
        id, "user-1", "user1@example.com", "127.0.0.1", AuditAction.Update,
        "SubscriptionTier", "abc", oldValues ?? "{\"Name\":\"Old\"}", "{\"Name\":\"New\"}",
        new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));

    private void SetupQueryResults(AuditLogEntryDto[] items)
    {
        queryService.CountAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(items.Length));

        var ids = items.Select(i => i.Id).ToList();

        // Simulerer keyset: listen er allerede i visningsrækkefølge; cursoren peger på sidste leverede række.
        queryService.GetBatchAsync(
                Arg.Any<AuditLogFilterDto>(), Arg.Any<AuditLogCursor?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var cursor = call.ArgAt<AuditLogCursor?>(1);
                var size = call.ArgAt<int>(2);
                var skip = cursor is null ? 0 : ids.IndexOf(cursor.Id) + 1;

                return Task.FromResult<IReadOnlyList<AuditLogEntryDto>>([.. items.Skip(skip).Take(size)]);
            });
    }

    private static AuditLogEntryDto[] NewestFirstEntries(int total) =>
    [.. Enumerable.Range(1, total).Select(i => Entry(total - i + 1))];

    [Fact]
    public async Task ExportAsync_CsvFormat_MoreRowsThanBatchSize_FetchesInBatchesAndExportsEveryRowOnceInOrder()
    {
        var total = (AuditLogExportService.BatchSize * 2) + 1;
        SetupQueryResults(NewestFirstEntries(total));
        using var stream = new MemoryStream();

        await sut.ExportAsync(EmptyFilter, AuditLogExportFormat.Csv, stream, TestContext.Current.CancellationToken);

        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

        var ids = content.Split('\n')
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Skip(1)
            .Select(l => long.Parse(l.Split(',')[0], System.Globalization.CultureInfo.InvariantCulture))
            .ToList();

        ids.Should().Equal(Enumerable.Range(1, total).Select(i => (long)(total - i + 1)));
        await queryService.Received(3).GetBatchAsync(
            Arg.Any<AuditLogFilterDto>(), Arg.Any<AuditLogCursor?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExportAsync_JsonFormat_MoreRowsThanBatchSize_ExportsEveryRowOnceInOrder()
    {
        var total = AuditLogExportService.BatchSize + 1;
        SetupQueryResults(NewestFirstEntries(total));
        using var stream = new MemoryStream();

        await sut.ExportAsync(EmptyFilter, AuditLogExportFormat.Json, stream, TestContext.Current.CancellationToken);

        stream.Position = 0;
        using var doc = await JsonDocument.ParseAsync(stream, default, TestContext.Current.CancellationToken);

        var ids = doc.RootElement.EnumerateArray().Select(e => e.GetProperty("Id").GetInt64()).ToList();

        ids.Should().Equal(Enumerable.Range(1, total).Select(i => (long)(total - i + 1)));
        await queryService.Received(2).GetBatchAsync(
            Arg.Any<AuditLogFilterDto>(), Arg.Any<AuditLogCursor?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExportAsync_JsonFormat_ZeroMatchingRows_ProducesEmptyArray()
    {
        SetupQueryResults([]);
        using var stream = new MemoryStream();

        await sut.ExportAsync(EmptyFilter, AuditLogExportFormat.Json, stream, TestContext.Current.CancellationToken);

        stream.Position = 0;
        using var doc = await JsonDocument.ParseAsync(stream, default, TestContext.Current.CancellationToken);

        doc.RootElement.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task ExportAsync_CsvFormat_WritesHeaderAndOneRowPerEntry()
    {
        SetupQueryResults([Entry()]);
        using var stream = new MemoryStream();

        await sut.ExportAsync(EmptyFilter, AuditLogExportFormat.Csv, stream, TestContext.Current.CancellationToken);

        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

        content.Should().Contain("Id,UserId,UserEmail,IpAddress,Action,EntityName,EntityId,OldValues,NewValues,TimestampUtc");
        content.Should().Contain("SubscriptionTier");
        content.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).Should().HaveCount(2);
    }

    [Fact]
    public async Task ExportAsync_CsvFormat_EscapesFieldsContainingCommasAndQuotes()
    {
        SetupQueryResults([Entry(oldValues: "{\"Name\":\"A, B\"}")]);
        using var stream = new MemoryStream();

        await sut.ExportAsync(EmptyFilter, AuditLogExportFormat.Csv, stream, TestContext.Current.CancellationToken);

        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

        content.Should().Contain("\"{\"\"Name\"\":\"\"A, B\"\"}\"");
    }

    [Fact]
    public async Task ExportAsync_JsonFormat_ProducesValidJsonArrayWithCorrectItemCount()
    {
        SetupQueryResults([Entry(1), Entry(2)]);
        using var stream = new MemoryStream();

        await sut.ExportAsync(EmptyFilter, AuditLogExportFormat.Json, stream, TestContext.Current.CancellationToken);

        stream.Position = 0;
        using var doc = await JsonDocument.ParseAsync(stream, default, TestContext.Current.CancellationToken);

        doc.RootElement.GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task ExportAsync_JsonFormat_SerializesActionAsString()
    {
        SetupQueryResults([Entry()]);
        using var stream = new MemoryStream();

        await sut.ExportAsync(EmptyFilter, AuditLogExportFormat.Json, stream, TestContext.Current.CancellationToken);

        stream.Position = 0;
        using var doc = await JsonDocument.ParseAsync(stream, default, TestContext.Current.CancellationToken);

        doc.RootElement[0].GetProperty("Action").GetString().Should().Be("Update");
    }

    [Fact]
    public async Task ExportAsync_XlsxFormat_ProducesNonEmptyWorkbookStream()
    {
        SetupQueryResults([Entry()]);
        using var stream = new MemoryStream();

        await sut.ExportAsync(EmptyFilter, AuditLogExportFormat.Xlsx, stream, TestContext.Current.CancellationToken);

        stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExportAsync_ZeroMatchingRows_WritesOnlyHeaderForCsv()
    {
        SetupQueryResults([]);
        using var stream = new MemoryStream();

        await sut.ExportAsync(EmptyFilter, AuditLogExportFormat.Csv, stream, TestContext.Current.CancellationToken);

        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

        content.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).Should().HaveCount(1);
    }

    [Fact]
    public async Task ExportAsync_CountExceedsHardLimit_ThrowsInvalidOperationException_WithoutReadingRows()
    {
        queryService.CountAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(IAuditLogExportService.HardRowLimit + 1));
        using var stream = new MemoryStream();

        var act = async () => await sut.ExportAsync(EmptyFilter, AuditLogExportFormat.Csv, stream);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await queryService.DidNotReceive().GetBatchAsync(
            Arg.Any<AuditLogFilterDto>(), Arg.Any<AuditLogCursor?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExportAsync_CountExactlyAtHardLimit_DoesNotThrow()
    {
        SetupQueryResults([Entry()]);
        queryService.CountAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(IAuditLogExportService.HardRowLimit));
        using var stream = new MemoryStream();

        var act = async () => await sut.ExportAsync(EmptyFilter, AuditLogExportFormat.Csv, stream);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExportAsync_NullFilter_ThrowsArgumentNullException()
    {
        using var stream = new MemoryStream();

        var act = async () => await sut.ExportAsync(null!, AuditLogExportFormat.Csv, stream);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExportAsync_NullDestination_ThrowsArgumentNullException()
    {
        var act = async () => await sut.ExportAsync(EmptyFilter, AuditLogExportFormat.Csv, null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}