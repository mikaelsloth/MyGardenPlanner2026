namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class AuditLogExportPanelTests : BunitContext
{
    private readonly IAuditLogQueryService queryService = Substitute.For<IAuditLogQueryService>();
    private readonly IAuditLogExportTokenService tokenService = Substitute.For<IAuditLogExportTokenService>();
    private readonly IAuthorizationService authorizationService = Substitute.For<IAuthorizationService>();
    private readonly IAdminActionRateLimiter rateLimiter = Substitute.For<IAdminActionRateLimiter>();
    private readonly IReAuthenticationService reAuthenticationService = Substitute.For<IReAuthenticationService>();
    private readonly IReAuthFailureTracker reAuthFailureTracker = Substitute.For<IReAuthFailureTracker>();
    private readonly ICurrentUserAccessor currentUserAccessor = Substitute.For<ICurrentUserAccessor>();

    private static AuditLogFilterDto EmptyFilter() => new(null, null, null, null, null, null, null);

    public AuditLogExportPanelTests()
    {
        Services.AddSingleton(queryService);
        Services.AddSingleton(tokenService);
        Services.AddSingleton(authorizationService);
        Services.AddSingleton(rateLimiter);
        Services.AddSingleton(IdentityTestDoubles.CreateUserManager());
        Services.AddSingleton(reAuthenticationService);
        Services.AddSingleton(reAuthFailureTracker);
        Services.AddSingleton(currentUserAccessor);
        Services.AddSingleton(Substitute.For<ILogger<AuditLogExportPanel>>());

        // Standard: reauth gyldig, permit ledig, 0 rækker at eksportere.
        rateLimiter.TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        authorizationService.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object?>(), Arg.Any<string>())
            .Returns(Task.FromResult(AuthorizationResult.Success()));
        queryService.CountAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));
        tokenService.IssueToken(Arg.Any<string>()).Returns("test-token");

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<AuditLogExportPanel> RenderPanel()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "user-1")], "TestAuth"));
        var authState = Task.FromResult(new AuthenticationState(principal));

        return Render<AuditLogExportPanel>(p => p
            .AddCascadingValue(authState)
            .Add(x => x.CurrentFilter, EmptyFilter()));
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

        cut.Find(".confirm-dialog-backdrop").Should().NotBeNull();
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
}