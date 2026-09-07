namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Pages.Admin;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class AdminAuditLogPageTests : BunitContext
{
    private readonly IAuditLogQueryService queryService = Substitute.For<IAuditLogQueryService>();
    private readonly IAuditLogViewerPreferenceService preferenceService = Substitute.For<IAuditLogViewerPreferenceService>();
    private readonly IAuditLogExportTokenService tokenService = Substitute.For<IAuditLogExportTokenService>();
    private readonly IAuthorizationService authorizationService = Substitute.For<IAuthorizationService>();
    private readonly IAdminActionRateLimiter rateLimiter = Substitute.For<IAdminActionRateLimiter>();
    private readonly IReAuthenticationService reAuthenticationService = Substitute.For<IReAuthenticationService>();
    private readonly IReAuthFailureTracker reAuthFailureTracker = Substitute.For<IReAuthFailureTracker>();
    private readonly ICurrentUserAccessor currentUserAccessor = Substitute.For<ICurrentUserAccessor>();

    private static AuditLogEntryDto Entry(long id = 1) => new(
        id, "user-1", "user1@example.com", "127.0.0.1", AuditAction.Update,
        "SubscriptionTier", "abc", null, null, new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));

    public AdminAuditLogPageTests()
    {
        Services.AddSingleton(queryService);
        Services.AddSingleton(preferenceService);
        Services.AddSingleton(tokenService);
        Services.AddSingleton(authorizationService);
        Services.AddSingleton(rateLimiter);
        Services.AddSingleton(IdentityTestDoubles.CreateUserManager());
        Services.AddSingleton(reAuthenticationService);
        Services.AddSingleton(reAuthFailureTracker);
        Services.AddSingleton(currentUserAccessor);

        queryService.GetDistinctEntityNamesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<string>>(["SubscriptionAddOn", "SubscriptionTier"]));

        preferenceService.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AuditLogViewerPreferenceDto(25, null)));

        authorizationService.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object?>(), Arg.Any<string>())
            .Returns(Task.FromResult(AuthorizationResult.Success()));
        rateLimiter.TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<AdminAuditLogPage> RenderPage()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "user-1")], "TestAuth"));
        var authState = Task.FromResult(new AuthenticationState(principal));

        return Render<AdminAuditLogPage>(p => p.AddCascadingValue(authState));
    }

    [Fact]
    public void OnInitialized_LoadsEntityNameOptions_AndPassesToFilterBar()
    {
        queryService.SearchAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AuditLogQueryResultDto([], 0, 1, 25)));

        var cut = RenderPage();

        cut.Markup.Should().Contain("SubscriptionAddOn");
        cut.Markup.Should().Contain("SubscriptionTier");
    }

    [Fact]
    public async Task OnInitialized_AppliesSavedPageSizePreference_ToInitialSearch()
    {
        preferenceService.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AuditLogViewerPreferenceDto(100, null)));
        queryService.SearchAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AuditLogQueryResultDto([], 0, 1, 100)));

        RenderPage();

        await queryService.Received().SearchAsync(
            Arg.Is<AuditLogFilterDto>(f => f.PageSize == 100), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OnInitialized_AppliesSavedLastFilter_ToInitialSearch()
    {
        var savedFilter = new AuditLogFilterDto("SubscriptionTier", null, null, null, null, null, null);
        preferenceService.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AuditLogViewerPreferenceDto(25, savedFilter)));
        queryService.SearchAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AuditLogQueryResultDto([], 0, 1, 25)));

        RenderPage();

        await queryService.Received().SearchAsync(
            Arg.Is<AuditLogFilterDto>(f => f.EntityName == "SubscriptionTier"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchingViaFilterBar_UpdatesResultsTable_AndSavesPreference()
    {
        queryService.SearchAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(new AuditLogQueryResultDto([], 0, 1, 25)),
                Task.FromResult(new AuditLogQueryResultDto([Entry()], 1, 1, 25)));

        var cut = RenderPage();

        await cut.Find(".filter-bar-actions button").ClickAsync();

        cut.FindAll("tbody tr").Should().HaveCount(1);
        await preferenceService.Received().SaveAsync(
            "user-1", Arg.Any<AuditLogViewerPreferenceDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ClickingNaesteSide_RequestsNextPage_WithSameFilterCriteria()
    {
        queryService.SearchAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AuditLogQueryResultDto([Entry()], 100, 1, 25)));

        var cut = RenderPage();
        await cut.FindAll(".audit-log-pagination button")[1].ClickAsync();

        await queryService.Received().SearchAsync(
            Arg.Is<AuditLogFilterDto>(f => f.PageNumber == 2), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ClickingDetaljer_OpensModalWithSelectedEntry()
    {
        var entry = Entry(9);
        queryService.SearchAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AuditLogQueryResultDto([entry], 1, 1, 25)));

        var cut = RenderPage();
        cut.Find("tbody tr button").Click();

        cut.Find("#audit-log-detail-title").TextContent.Should().Contain("abc");
    }

    [Fact]
    public void ClosingModal_HidesDetailModal()
    {
        var entry = Entry(9);
        queryService.SearchAsync(Arg.Any<AuditLogFilterDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AuditLogQueryResultDto([entry], 1, 1, 25)));

        var cut = RenderPage();
        cut.Find("tbody tr button").Click();
        cut.Find(".confirm-dialog-actions button").Click();

        cut.FindAll(".confirm-dialog-backdrop").Should().BeEmpty();
    }
}