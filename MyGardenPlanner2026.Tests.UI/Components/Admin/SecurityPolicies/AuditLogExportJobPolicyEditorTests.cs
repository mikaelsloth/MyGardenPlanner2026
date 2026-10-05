namespace MyGardenPlanner2026.Tests.UI.Components.Admin.SecurityPolicies;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class AuditLogExportJobPolicyEditorTests : BunitContext
{
    private readonly IAuditLogExportJobPolicyAdminService adminService = Substitute.For<IAuditLogExportJobPolicyAdminService>();
    private readonly IAuthorizationService authorizationService;
    private readonly IAdminActionRateLimiter rateLimiter;

    public AuditLogExportJobPolicyEditorTests()
    {
        Services.AddSingleton(adminService);

        var fakes = this.RegisterAdminStepUpFakes<AuditLogExportJobPolicyEditor>();
        authorizationService = fakes.AuthorizationService;
        rateLimiter = fakes.RateLimiter;

        adminService.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AuditLogExportJobPolicyDto(24, 3)));
    }

    private IRenderedComponent<AuditLogExportJobPolicyEditor> RenderEditor() =>
        Render<AuditLogExportJobPolicyEditor>(p => p.AddCascadingValue(TestPrincipals.CreateAuthStateAsync()));

    [Fact]
    public void AuditLogExportJobPolicyEditor_RendersSeededValues()
    {
        var cut = RenderEditor();

        cut.Find("#exportjob-retention").GetAttribute("value").Should().Be("24");
        cut.Find("#exportjob-maxactive").GetAttribute("value").Should().Be("3");
    }

    [Fact]
    public async Task ReAuthValid_ClickingGem_CallsUpdateAsyncWithEditedValues_WithoutOpeningModal()
    {
        var cut = RenderEditor();

        await cut.Find("#exportjob-retention").ChangeAsync("72");
        await cut.Find("#exportjob-maxactive").ChangeAsync("10");
        await cut.Find("button.btn-primary").ClickAsync();

        cut.ShouldNotShowStepUpModal();
        await adminService.Received().UpdateAsync(
            Arg.Is<AuditLogExportJobPolicyDto>(d => d.RetentionHours == 72 && d.MaxActiveJobsPerUser == 10),
            "user-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReAuthValid_ClickingGem_InvokesOnStatusMessage()
    {
        adminService.UpdateAsync(Arg.Any<AuditLogExportJobPolicyDto>(), "user-1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AuditLogExportJobPolicyDto(72, 10)));
        string? statusMessage = null;

        var cut = Render<AuditLogExportJobPolicyEditor>(p => p
            .AddCascadingValue(TestPrincipals.CreateAuthStateAsync())
            .Add(x => x.OnStatusMessage, EventCallback.Factory.Create<string>(this, m => statusMessage = m)));

        await cut.Find("button.btn-primary").ClickAsync();

        statusMessage.Should().NotBeNull();
    }

    [Fact]
    public async Task ReAuthExpired_ClickingGem_OpensStepUpModal_WithoutSaving()
    {
        authorizationService.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object?>(), Arg.Any<string>())
            .Returns(Task.FromResult(AuthorizationResult.Failed()));

        var cut = RenderEditor();
        await cut.Find("button.btn-primary").ClickAsync();

        cut.ShouldShowStepUpModal();
        await adminService.DidNotReceive().UpdateAsync(
            Arg.Any<AuditLogExportJobPolicyDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReAuthExpired_CancellingStepUpModal_ClosesModal_WithoutSaving()
    {
        authorizationService.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object?>(), Arg.Any<string>())
            .Returns(Task.FromResult(AuthorizationResult.Failed()));

        var cut = RenderEditor();
        await cut.Find("button.btn-primary").ClickAsync();
        await cut.Find(".confirm-dialog-actions button.btn-secondary").ClickAsync();

        cut.ShouldNotShowStepUpModal();
        await adminService.DidNotReceive().UpdateAsync(
            Arg.Any<AuditLogExportJobPolicyDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RateLimited_ClickingGem_DoesNotCallUpdateAsync_AndShowsErrorMessage()
    {
        rateLimiter.TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));

        var cut = RenderEditor();
        await cut.Find("button.btn-primary").ClickAsync();

        cut.Find(".status-message.status-danger").Should().NotBeNull();
        await adminService.DidNotReceive().UpdateAsync(
            Arg.Any<AuditLogExportJobPolicyDto>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ThrowsArgumentOutOfRangeException_ShowsErrorMessage()
    {
        adminService.UpdateAsync(Arg.Any<AuditLogExportJobPolicyDto>(), "user-1", Arg.Any<CancellationToken>())
            .Returns<Task<AuditLogExportJobPolicyDto>>(_ => throw new ArgumentOutOfRangeException(null, "RetentionHours skal være mindst 1."));

        var cut = RenderEditor();
        await cut.Find("button.btn-primary").ClickAsync();

        cut.Find(".status-message.status-danger").TextContent.Should().Contain("RetentionHours");
    }
}