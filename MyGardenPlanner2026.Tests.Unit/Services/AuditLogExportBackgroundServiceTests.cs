namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public sealed class AuditLogExportBackgroundServiceTests
{
    private static AuditLogExportBackgroundService CreateService(FakeProcessor processor)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAuditLogExportJobProcessor>(processor);
        var provider = services.BuildServiceProvider();

        return new AuditLogExportBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            new ExportTestOptionsMonitor<AuditLogExportJobOptions>(new AuditLogExportJobOptions()),
            NullLogger<AuditLogExportBackgroundService>.Instance);
    }

    [Fact]
    public async Task RunOnceAsync_RecoversStaleJobs_ThenDeletesExpired_ThenProcessesUntilNoneLeft()
    {
        var processor = new FakeProcessor(true, true, false);

        var processed = await CreateService(processor).RunOnceAsync(TestContext.Current.CancellationToken);

        processed.Should().Be(2);
        processor.StaleCalls.Should().Be(1);
        processor.DeleteExpiredCalls.Should().Be(1);
        processor.ProcessCalls.Should().Be(3);
    }

    [Fact]
    public async Task RunOnceAsync_NoPendingJobs_ReturnsZero()
    {
        var processor = new FakeProcessor(false);

        var processed = await CreateService(processor).RunOnceAsync(TestContext.Current.CancellationToken);

        processed.Should().Be(0);
    }

    [Fact]
    public async Task RunOnceAsync_AlreadyCancelled_DoesNotProcessAnyJob()
    {
        var processor = new FakeProcessor(true);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var processed = await CreateService(processor).RunOnceAsync(cts.Token);

        processed.Should().Be(0);
        processor.ProcessCalls.Should().Be(0);
    }

    private sealed class FakeProcessor(params bool[] results) : IAuditLogExportJobProcessor
    {
        private readonly Queue<bool> queue = new(results);

        public int ProcessCalls { get; private set; }
        public int StaleCalls { get; private set; }
        public int DeleteExpiredCalls { get; private set; }

        public Task<bool> ProcessNextAsync(CancellationToken cancellationToken = default)
        {
            ProcessCalls++;
            return Task.FromResult(queue.Count > 0 && queue.Dequeue());
        }

        public Task<int> FailStaleRunningJobsAsync(CancellationToken cancellationToken = default)
        {
            StaleCalls++;
            return Task.FromResult(0);
        }

        public Task<int> DeleteExpiredJobsAsync(CancellationToken cancellationToken = default)
        {
            DeleteExpiredCalls++;
            return Task.FromResult(0);
        }
    }
}