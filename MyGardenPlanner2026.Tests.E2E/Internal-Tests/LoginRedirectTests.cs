namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;

public sealed class LoginRedirectTests
{
    [Fact]
    public void ReturnPath_RelativReturnUrl_GiverStien()
    {
        LoginRedirect.ReturnPath("http://127.0.0.1:5000/Account/Login?ReturnUrl=%2Fadmin%2Fsubscriptions")
            .Should().Be("/admin/subscriptions");
    }

    [Fact]
    public void ReturnPath_AbsolutReturnUrlMedSmaaBogstaver_GiverStien()
    {
        LoginRedirect.ReturnPath(
                "http://127.0.0.1:5000/Account/Login?returnUrl=http%3A%2F%2F127.0.0.1%3A5000%2Fadmin%2Faudit-log")
            .Should().Be("/admin/audit-log");
    }

    [Fact]
    public void ReturnPath_ReturnUrlMedQuery_FjernerQuery()
    {
        LoginRedirect.ReturnPath("http://127.0.0.1:5000/Account/Login?ReturnUrl=%2Fadmin%2Fx%3Fa%3D1")
            .Should().Be("/admin/x");
    }

    [Fact]
    public void ReturnPath_UdenReturnUrl_GiverNull()
    {
        LoginRedirect.ReturnPath("http://127.0.0.1:5000/Account/Login").Should().BeNull();
    }

    [Fact]
    public void ReturnPath_UgyldigUrl_GiverNull()
    {
        LoginRedirect.ReturnPath("ikke en url").Should().BeNull();
    }
}