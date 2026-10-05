namespace MyGardenPlanner2026.Tests.UI.Components.Admin.AuditLog;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class AuditLogExportPanelTests : BunitContext
{
    private readonly IAuditLogQueryService queryService = Substitute.For<IAuditLogQueryService>();
    private readonly IAuditLogExportTokenService tokenService = Substitute.For<IAuditLogExportTokenService>();
    private readonly IAuthorizationService authorizationService;
    private readonly IAdminActionRateLimiter rateLimiter;
    private readonly IAuditLogExportJobService exportJobService = Substitute.For<IAuditLogExportJobService>();

    public AuditLogExportPanelTests()
    {
        Services.AddSingleton(queryService);
        Services.AddSingleton(tokenService);
        Services.AddSingleton(exportJobService);

        var fakes = this.RegisterAdminStepUpFakes<AuditLogExportPanel>();
        authorizationService = fakes.AuthorizationService;
        rateLimiter = fakes.RateLimiter;

        // Standard: reauth gyldig, permit ledig, 0 rækker at eksportere.
        queryService.CountAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));
        tokenService.IssueToken(Arg.Any<string>()).Returns("test-token");

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<AuditLogExportPanel> RenderPanel(EventCallback? onJobEnqueued = null)
    {
        return Render<AuditLogExportPanel>(p =>
        {
            p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync());
            p.Add(x => x.CurrentFilter, AuditLogTestData.EmptyFilter());
            if (onJobEnqueued is { } callback)
            {
                p.Add(x => x.OnJobEnqueued, callback);
            }
        });
    }

    [Fact]
    public void ReAuthValid_ClickingEksportér_TriggersDownload_WithoutOpeningModal()
    {
        var cut = RenderPanel();

        cut.Find("button.btn-primary").Click();

        cut.FindAll(".confirm-dialog-backdrop").Should().BeEmpty();
        JSInterop.Invocations["open"].Should().ContainSingle();
    }

    [Fact]
    public void ReAuthValid_DownloadUrl_ContainsTokenAndSelectedFormat()
    {
        var cut = RenderPanel();

        cut.Find("button.btn-primary").Click();

        var url = JSInterop.Invocations["open"].Single().Arguments[0]!.ToString();

        url.Should().Contain("token=test-token");
        url.Should().Contain("format=Csv");
    }

    [Fact]
    public void ReAuthExpired_ClickingEksportér_OpensStepUpModal_WithoutDownloading()
    {
        authorizationService.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object?>(), Arg.Any<string>())
            .Returns(Task.FromResult(AuthorizationResult.Failed()));

        var cut = RenderPanel();

        cut.Find("button.btn-primary").Click();

        cut.ShouldShowStepUpModal();
        JSInterop.Invocations.Should().NotContain(i => i.InvocationMethodName == "open");
    }

    [Fact]
    public async Task RateLimited_ClickingEksportér_DoesNotCallCountAsync_AndShowsErrorMessage()
    {
        rateLimiter.TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));

        var cut = RenderPanel();

        await cut.Find("button.btn-primary").ClickAsync();

        cut.Find(".status-message.status-danger").Should().NotBeNull();
        await queryService.DidNotReceive().CountAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void CountExceedsSoftThreshold_ShowsConfirmationDialog_WithoutDownloadingImmediately()
    {
        queryService.CountAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(60_000));

        var cut = RenderPanel();

        cut.Find("button.btn-primary").Click();

        cut.Find(".inline-confirm").TextContent.Should().Contain("60000");
        JSInterop.Invocations.Should().NotContain(i => i.InvocationMethodName == "open");
    }

    [Fact]
    public void ConfirmingThresholdDialog_TriggersDownload()
    {
        queryService.CountAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(60_000));

        var cut = RenderPanel();
        cut.Find("button.btn-primary").Click();
        cut.Find(".inline-confirm .btn-primary").Click();

        JSInterop.Invocations["open"].Should().ContainSingle();
    }

    [Fact]
    public void CancellingThresholdDialog_DoesNotDownload_AndHidesDialog()
    {
        queryService.CountAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(60_000));

        var cut = RenderPanel();
        cut.Find("button.btn-primary").Click();
        cut.Find(".inline-confirm .btn-secondary").Click();

        cut.FindAll(".inline-confirm").Should().BeEmpty();
        JSInterop.Invocations.Should().NotContain(i => i.InvocationMethodName == "open");
    }

    [Fact]
    public void CountExceedsHardLimit_ShowsErrorMessage_WithoutConfirmOrDownload()
    {
        queryService.CountAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(IAuditLogExportService.HardRowLimit + 1));

        var cut = RenderPanel();

        cut.Find("button.btn-primary").Click();

        cut.Find(".status-message.status-danger").Should().NotBeNull();
        cut.FindAll(".inline-confirm").Should().BeEmpty();
        JSInterop.Invocations.Should().NotContain(i => i.InvocationMethodName == "open");
    }

    [Fact]
    public async Task ReAuthValid_ClickingSendTilBaggrundseksport_CallsEnqueueAsync_AndShowsSuccessMessage()
    {
        exportJobService.EnqueueAsync("user-1", Arg.Any<AuditLogFilterDto>(), AuditLogExportFormat.Csv, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(AuditLogTestData.ExportJob(AuditLogExportJobStatus.Pending, rowCount: null, fileName: null)));

        var cut = RenderPanel();

        await cut.Find("button.btn-secondary").ClickAsync();

        cut.Find(".status-message.status-success").Should().NotBeNull();
        await exportJobService.Received().EnqueueAsync(
            "user-1", Arg.Any<AuditLogFilterDto>(), AuditLogExportFormat.Csv, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReAuthExpired_ClickingSendTilBaggrundseksport_OpensStepUpModal_WithoutEnqueuing()
    {
        authorizationService.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object?>(), Arg.Any<string>())
            .Returns(Task.FromResult(AuthorizationResult.Failed()));

        var cut = RenderPanel();

        await cut.Find("button.btn-secondary").ClickAsync();

        cut.ShouldShowStepUpModal();
        await exportJobService.DidNotReceive().EnqueueAsync(
            Arg.Any<string>(), Arg.Any<AuditLogFilterDto>(), Arg.Any<AuditLogExportFormat>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RateLimited_ClickingSendTilBaggrundseksport_DoesNotCallEnqueueAsync_AndShowsErrorMessage()
    {
        rateLimiter.TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));

        var cut = RenderPanel();

        await cut.Find("button.btn-secondary").ClickAsync();

        cut.Find(".status-message.status-danger").Should().NotBeNull();
        await exportJobService.DidNotReceive().EnqueueAsync(
            Arg.Any<string>(), Arg.Any<AuditLogFilterDto>(), Arg.Any<AuditLogExportFormat>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ClickingSendTilBaggrundseksport_ServiceThrowsInvalidOperationException_ShowsErrorMessage()
    {
        exportJobService.EnqueueAsync(Arg.Any<string>(), Arg.Any<AuditLogFilterDto>(), Arg.Any<AuditLogExportFormat>(), Arg.Any<CancellationToken>())
            .Returns<Task<AuditLogExportJobDto>>(_ => throw new InvalidOperationException("Du har allerede 3 igangværende eksporter."));

        var cut = RenderPanel();

        await cut.Find("button.btn-secondary").ClickAsync();

        cut.Find(".status-message.status-danger").TextContent.Should().Contain("igangværende eksporter");
    }

    [Fact]
    public async Task ClickingSendTilBaggrundseksport_Success_InvokesOnJobEnqueued()
    {
        exportJobService.EnqueueAsync(Arg.Any<string>(), Arg.Any<AuditLogFilterDto>(), Arg.Any<AuditLogExportFormat>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(AuditLogTestData.ExportJob(AuditLogExportJobStatus.Pending, rowCount: null, fileName: null)));
        var invoked = false;

        var cut = RenderPanel(EventCallback.Factory.Create(this, () => invoked = true));

        await cut.Find("button.btn-secondary").ClickAsync();

        invoked.Should().BeTrue();
    }
}