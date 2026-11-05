namespace MyGardenPlanner2026.Tests.Unit.Services.AuditLog.Export;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Services.AuditLog;
using MyGardenPlanner2026.Infrastructure.Services.AuditLog.Export;
using MyGardenPlanner2026.Infrastructure.Services.AuditLog.ExportJobs;
using System.Diagnostics;
using System.Globalization;
using Xunit;

/// <summary>
/// Måling (ikke en regressionstest) til beslutningen om Excel-eksport ved HardRowLimit rækker
/// (R5.3, B4). Kører kun med MGP_RUN_VOLUME_MEASUREMENT=1, og kun mod en reel SQL Express-instans
/// (se TestSqlExpressDbContext). Kør helst i Release og ét format ad gangen, så tallene ikke
/// påvirker hinanden. Hukommelsestal er målt i testprocessen (managed heap og working set).
/// </summary>
[Trait("Category", "VolumeMeasurement")]
public sealed class AuditLogExportVolumeMeasurementTests(ITestOutputHelper output) : TestSqlExpressDbContext
{
    private const string EnableVariable = "MGP_RUN_VOLUME_MEASUREMENT";
    private const int RowCount = IAuditLogExportService.HardRowLimit;
    private const string resultPath = "C:\\Users\\mikae\\source\\repos\\MyGardenPlanner2026\\TestResults\\";
    private const string measurementPath = "Measurements\\";

    // Længden af tekstfeltet i hver af Old-/NewValues. Justér til en realistisk værdi for produktionsdata.
    private const int PayloadCharacters = 300;

    [Theory]
    [InlineData(AuditLogExportFormat.Csv)]
    [InlineData(AuditLogExportFormat.Json)]
    [InlineData(AuditLogExportFormat.Xlsx)]
    public async Task Export_AtHardRowLimit_MeasuresTimeSizeAndMemory(AuditLogExportFormat format)
    {
        Assert.SkipUnless(
            Environment.GetEnvironmentVariable(EnableVariable) == "1",
            $"Sæt miljøvariablen {EnableVariable}=1 for at køre målingen (tung: {RowCount} rækker).");

        await SeedAsync();

        var options = new TestOptionsMonitor<AuditLogExportJobOptions>(
            new AuditLogExportJobOptions { XlsxMaxRows = IAuditLogExportService.HardRowLimit });
        var sut = new AuditLogExportService(new AuditLogQueryService(CreateDbContextFactory()), options);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var baselineManaged = GC.GetTotalMemory(forceFullCollection: true);
        var allocatedBefore = GC.GetTotalAllocatedBytes(precise: true);
        var gen2Before = GC.CollectionCount(2);

        var sampler = new PeakMemorySampler();
        var stopwatch = Stopwatch.StartNew();

        using var stream = new MemoryStream();
        await sut.ExportAsync(AuditLogTestData.EmptyFilter, format, stream, TestContext.Current.CancellationToken);

        stopwatch.Stop();
        var (ManagedBytes, WorkingSetBytes) = await sampler.StopAsync();

        var allocated = GC.GetTotalAllocatedBytes(precise: true) - allocatedBefore;
        var gen2 = GC.CollectionCount(2) - gen2Before;

        string[] parts =
        [
            $"utc={DateTimeOffset.UtcNow:O}",
            $"format={format}",
            $"rows={RowCount}",
            $"payloadChars={PayloadCharacters}",
            $"elapsed={stopwatch.Elapsed.TotalSeconds.ToString("F1", CultureInfo.InvariantCulture)}s",
            $"file={ToMegabytes(stream.Length)}MB",
            $"managedPeakDelta={ToMegabytes(ManagedBytes - baselineManaged)}MB",
            $"workingSetPeak={ToMegabytes(WorkingSetBytes)}MB",
            $"allocated={ToMegabytes(allocated)}MB",
            $"gen2Collections={gen2}"
        ];

        var line = string.Join(' ', parts);
        output.WriteLine(line);
        await File.AppendAllTextAsync(
            Path.Combine(resultPath, measurementPath, "volume-measurement.txt"),
            line + Environment.NewLine,
            TestContext.Current.CancellationToken);

        stream.Length.Should().BeGreaterThan(0);
    }

    private static string ToMegabytes(long bytes) =>
        (bytes / 1024d / 1024d).ToString("F1", CultureInfo.InvariantCulture);

    /// <summary>Set-baseret INSERT … SELECT (samme princip som AuditLogTestData.SeedBulkAsync i E2E).</summary>
    private async Task SeedAsync()
    {
        await using var context = CreateDbContext();
        context.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));

        // Tegnene {{ og }} er escaping af { og } for ExecuteSqlRawAsync (parametre er {0}, {1}, {2}).
        await context.Database.ExecuteSqlRawAsync(
            """
            ;WITH numbers AS (
                SELECT TOP ({0}) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
                FROM sys.all_objects a CROSS JOIN sys.all_objects b
            )
            INSERT INTO [admin].[AuditLogs]
                ([UserId], [UserEmail], [IpAddress], [Action], [EntityName], [EntityId], [OldValues], [NewValues], [TimestampUtc])
            SELECT
                'volume-user', 'volume@example.com', '10.0.0.7', {1}, 'VolumeMeasurement', CONVERT(nvarchar(100), n),
                CONCAT('{{"Name":"Old ', n, '","Text":"', REPLICATE('x', {2}), '"}}'),
                CONCAT('{{"Name":"New ', n, '","Text":"', REPLICATE('y', {2}), '"}}'),
                DATEADD(SECOND, -CONVERT(int, n), SYSDATETIMEOFFSET())
            FROM numbers
            """,
            RowCount,
            (int)AuditAction.Update,
            PayloadCharacters);
    }

    private sealed class PeakMemorySampler
    {
        private readonly CancellationTokenSource cancellation = new();
        private readonly Task loop;
        private long peakManaged;
        private long peakWorkingSet;

        public PeakMemorySampler() => loop = Task.Run(SampleAsync, cancellation.Token);

        public async Task<(long ManagedBytes, long WorkingSetBytes)> StopAsync()
        {
            await cancellation.CancelAsync();
            await loop;
            cancellation.Dispose();

            return (peakManaged, peakWorkingSet);
        }

        private async Task SampleAsync()
        {
            using var process = Process.GetCurrentProcess();

            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    Record(process);
                    await Task.Delay(25, cancellation.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Forventet ved StopAsync.
            }

            Record(process);
        }

        private void Record(Process process)
        {
            process.Refresh();
            peakManaged = Math.Max(peakManaged, GC.GetTotalMemory(forceFullCollection: false));
            peakWorkingSet = Math.Max(peakWorkingSet, process.WorkingSet64);
        }
    }
}