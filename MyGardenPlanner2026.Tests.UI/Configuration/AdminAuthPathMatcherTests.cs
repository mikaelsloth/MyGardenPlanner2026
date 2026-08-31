namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using MyGardenPlanner2026.Configuration.RateLimiting;
using Xunit;

public class AdminAuthPathMatcherTests
{
    [Theory]
    [InlineData("/Account/Login")]
    [InlineData("/account/login")]
    [InlineData("/Account/LoginWith2fa")]
    [InlineData("/Account/LoginWithRecoveryCode")]
    public void IsProtectedAuthRequest_PostToLoginEndpoint_ReturnsTrue(string path)
    {
        AdminAuthPathMatcher.IsProtectedAuthRequest("POST", path).Should().BeTrue();
    }

    [Fact]
    public void IsProtectedAuthRequest_GetToLoginEndpoint_ReturnsFalse()
    {
        AdminAuthPathMatcher.IsProtectedAuthRequest("GET", "/Account/Login").Should().BeFalse();
    }

    [Fact]
    public void IsProtectedAuthRequest_PostToUnrelatedPath_ReturnsFalse()
    {
        AdminAuthPathMatcher.IsProtectedAuthRequest("POST", "/pricing").Should().BeFalse();
    }

    [Fact]
    public void IsProtectedAuthRequest_PostToRegister_ReturnsFalse()
    {
        // Registrering er bevidst IKKE omfattet — kun login-forsøg.
        AdminAuthPathMatcher.IsProtectedAuthRequest("POST", "/Account/Register").Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsProtectedAuthRequest_EmptyOrNullPath_ReturnsFalse(string? path)
    {
        AdminAuthPathMatcher.IsProtectedAuthRequest("POST", path!).Should().BeFalse();
    }
}