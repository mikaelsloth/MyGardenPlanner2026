namespace MyGardenPlanner2026.Tests.UI.Components.Admin.AuditLog;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using NSubstitute;
using Xunit;

public class AuditLogExportJobsPanelTests : BunitContext
{
    private readonly IAuditLogExportJobService jobService = Substitute.For<IAuditLogExportJobService>();

    public AuditLogExportJobsPanelTests()
    {
        Services.AddSingleton(jobService);
    }

    private IRenderedComponent<AuditLogExportJobsPanel> RenderPanel() =>
        Render<AuditLogExportJobsPanel>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

    [Fact]
    public void NoJobs_ShowsEmptyMessage()
    {
        jobService.GetJobsForUserAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AuditLogExportJobDto>>([]));

        var cut = RenderPanel();

        cut.Markup.Should().Contain("Ingen baggrundseksporter");
    }

    [Fact]
    public void JobsPresent_RendersOneRowPerJob()
    {
        jobService.GetJobsForUserAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AuditLogExportJobDto>>([AuditLogTestData.ExportJob(), AuditLogTestData.ExportJob(AuditLogExportJobStatus.Pending)]));

        var cut = RenderPanel();

        cut.FindAll("tbody tr").Should().HaveCount(2);
    }

    [Fact]
    public void CompletedNonExpiredJob_ShowsDownloadLink()
    {
        var job = AuditLogTestData.ExportJob(expiresAt: DateTimeOffset.UtcNow.AddHours(1));
        jobService.GetJobsForUserAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AuditLogExportJobDto>>([job]));

        var cut = RenderPanel();

        cut.Find("a.btn-primary").GetAttribute("href").Should().Contain($"export-jobs/{job.Id}/download");
    }

    [Fact]
    public void CompletedExpiredJob_ShowsUdloebetInsteadOfDownloadLink()
    {
        var job = AuditLogTestData.ExportJob(expiresAt: DateTimeOffset.UtcNow.AddHours(-1));
        jobService.GetJobsForUserAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AuditLogExportJobDto>>([job]));

        var cut = RenderPanel();

        cut.FindAll("a.btn-primary").Should().BeEmpty();
        cut.Markup.Should().Contain("Udløbet");
    }

    [Fact]
    public void FailedJob_ShowsErrorMessage_WithoutDownloadLink()
    {
        var job = AuditLogTestData.ExportJob(AuditLogExportJobStatus.Failed, rowCount: null, errorMessage: "Noget gik galt.");
        jobService.GetJobsForUserAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AuditLogExportJobDto>>([job]));

        var cut = RenderPanel();

        cut.FindAll("a.btn-primary").Should().BeEmpty();
        cut.Markup.Should().Contain("Noget gik galt.");
    }

    [Fact]
    public void CompletedUnseenJob_ShowsClearNotificationButton()
    {
        var job = AuditLogTestData.ExportJob(seenAt: null);
        jobService.GetJobsForUserAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AuditLogExportJobDto>>([job]));

        var cut = RenderPanel();

        cut.FindAll("button.btn-ghost").Should().ContainSingle();
    }

    [Fact]
    public void CompletedSeenJob_DoesNotShowClearNotificationButton()
    {
        var job = AuditLogTestData.ExportJob(seenAt: DateTimeOffset.UtcNow);
        jobService.GetJobsForUserAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AuditLogExportJobDto>>([job]));

        var cut = RenderPanel();

        cut.FindAll("button.btn-ghost").Should().BeEmpty();
    }

    [Fact]
    public async Task ClickingRydNotifikation_CallsMarkNotificationSeenAsync_AndReloads()
    {
        var job = AuditLogTestData.ExportJob(seenAt: null);
        jobService.GetJobsForUserAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AuditLogExportJobDto>>([job]));

        var cut = RenderPanel();
        await cut.Find("button.btn-ghost").ClickAsync();

        await jobService.Received().MarkNotificationSeenAsync(job.Id, "user-1", Arg.Any<CancellationToken>());
        await jobService.Received(2).GetJobsForUserAsync("user-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ClickingOpdater_ReloadsJobs()
    {
        jobService.GetJobsForUserAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AuditLogExportJobDto>>([]));

        var cut = RenderPanel();
        await cut.Find(".card-header button").ClickAsync();

        await jobService.Received(2).GetJobsForUserAsync("user-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshAsync_CalledExternally_ReloadsJobs()
    {
        jobService.GetJobsForUserAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AuditLogExportJobDto>>([]));

        var cut = RenderPanel();
        await cut.InvokeAsync(() => cut.Instance.RefreshAsync());

        await jobService.Received(2).GetJobsForUserAsync("user-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NullAuthenticationStateTask_ShowsEmptyMessage_WithoutCallingService()
    {
        var cut = Render<AuditLogExportJobsPanel>();

        cut.Markup.Should().Contain("Ingen baggrundseksporter");
        await jobService.DidNotReceive().GetJobsForUserAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}