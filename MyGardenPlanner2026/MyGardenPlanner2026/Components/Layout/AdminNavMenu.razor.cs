namespace MyGardenPlanner2026.Components.Layout;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Viser kun links til admin-sider, som den aktuelle bruger har adgang til.
/// Adgang afgøres pr. side ved at spørge den relevante AuthorizationPolicy
/// direkte (samme policies som sidernes egne [Authorize]-attributter), så
/// synligheden automatisk følger både direkte rolle-medlemskab og aktiv
/// JIT-eskalering uden at duplikere den logik her (se JitRoleAuthorizationHandler
/// / AnyAdminRoleAuthorizationHandler).
/// </summary>
public partial class AdminNavMenu
{
    private sealed record AdminNavItem(string Href, string Label, string PolicyName);

    private static readonly AdminNavItem[] AllItems =
    [
        new("admin/subscriptions", "Abonnementer", AuthorizationServicesExtensions.RequireGlobalAdminPolicy),
        new("admin/security-policies", "Sikkerhedspolicies", AuthorizationServicesExtensions.RequirePolicyAdminPolicy),
        new("admin/jit-requests", "JIT-adgang", AuthorizationServicesExtensions.RequireAnyAdminRolePolicy),
        new("admin/audit-log", "AuditLog", AuthorizationServicesExtensions.RequireAuditViewerPolicy)
    ];

    private const string AuditLogHref = "admin/audit-log";

    [Inject]
    private IAuditLogExportJobService ExportJobService { get; set; } = default!;

    private int unseenExportJobCount;

    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private IReadOnlyList<AdminNavItem> visibleItems = [];

    protected override async Task OnInitializedAsync()
    {
        visibleItems = await ResolveVisibleItemsAsync();

        if (visibleItems.Any(i => i.Href == AuditLogHref))
        {
            var userId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);
            if (userId is not null)
            {
                unseenExportJobCount = await ExportJobService.CountUnseenCompletedAsync(userId);
            }
        }
    }

    private async Task<IReadOnlyList<AdminNavItem>> ResolveVisibleItemsAsync()
    {
        if (AuthenticationStateTask is null)
        {
            return [];
        }

        var authState = await AuthenticationStateTask;
        var visible = new List<AdminNavItem>();

        foreach (var item in AllItems)
        {
            var result = await AuthorizationService.AuthorizeAsync(authState.User, resource: null, item.PolicyName);
            if (result.Succeeded)
            {
                visible.Add(item);
            }
        }

        return visible;
    }
}