namespace MyGardenPlanner2026.Tests.E2E;

/// <summary>Stier der bruges i E2E-tests.</summary>
public static class E2ERoutes
{
    public const string Landing = "/";
    public const string Pricing = "/pricing";
    public const string About = "/about";
    public const string Terms = "/terms";
    public const string Privacy = "/privacy";

    public const string AdminSubscriptions = "/admin/subscriptions";
    public const string AdminJitRequests = "/admin/jit-requests";
    public const string AdminSecurityPolicies = "/admin/security-policies";
    public const string AdminAuditLog = "/admin/audit-log";

    public const string EnableAuthenticator = "/Account/Manage/EnableAuthenticator";
    public const string AccessDenied = "/Account/AccessDenied";
    public const string Login = "/Account/Login";
    public const string Register = "/Account/Register";
    public const string RegisterConfirmation = "/Account/RegisterConfirmation";
    public const string Lockout = "/Account/Lockout";
}