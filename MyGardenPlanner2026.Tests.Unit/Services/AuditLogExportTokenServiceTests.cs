namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public sealed class AuditLogExportTokenServiceTests
{
    private readonly TestTimeProvider timeProvider = new(DateTimeOffset.Now);
    private readonly TestOptionsMonitor<ReAuthenticationPolicyOptions> policyOptionsMonitor =
        new(new ReAuthenticationPolicyOptions { MaxAgeMinutes = 15 });

    private AuditLogExportTokenService CreateSut(string applicationDiscriminator = "AuditLogExportTokenServiceTests")
    {
        var services = new ServiceCollection();
        services.AddDataProtection().SetApplicationName(applicationDiscriminator);
        var provider = services.BuildServiceProvider().GetRequiredService<IDataProtectionProvider>();

        return new AuditLogExportTokenService(provider, policyOptionsMonitor, timeProvider);
    }

    [Fact]
    public void IssueToken_ThenValidate_SameUserWithinMaxAge_Succeeds()
    {
        var sut = CreateSut();
        var token = sut.IssueToken("user-1");

        var isValid = sut.TryValidateToken(token, "user-1", out var failureReason);

        isValid.Should().BeTrue();
        failureReason.Should().BeNull();
    }

    [Fact]
    public void TryValidateToken_DifferentUser_Fails()
    {
        var sut = CreateSut();
        var token = sut.IssueToken("user-1");

        var isValid = sut.TryValidateToken(token, "user-2", out var failureReason);

        isValid.Should().BeFalse();
        failureReason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TryValidateToken_TamperedToken_Fails()
    {
        var sut = CreateSut();
        var token = sut.IssueToken("user-1");
        var tampered = token[..^1] + (token[^1] == 'A' ? 'B' : 'A');

        var isValid = sut.TryValidateToken(tampered, "user-1", out _);

        isValid.Should().BeFalse();
    }

    [Fact]
    public void TryValidateToken_EmptyToken_Fails()
    {
        var sut = CreateSut();

        var isValid = sut.TryValidateToken(string.Empty, "user-1", out var failureReason);

        isValid.Should().BeFalse();
        failureReason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TryValidateToken_TokenFromDifferentProtector_Fails()
    {
        var sut = CreateSut("AppA");

        var otherServices = new ServiceCollection();
        otherServices.AddDataProtection().SetApplicationName("AppB");
        var otherProvider = otherServices.BuildServiceProvider().GetRequiredService<IDataProtectionProvider>();
        var otherSut = new AuditLogExportTokenService(otherProvider, policyOptionsMonitor, timeProvider);

        var token = otherSut.IssueToken("user-1");
        var isValid = sut.TryValidateToken(token, "user-1", out _);

        isValid.Should().BeFalse();
    }

    [Fact]
    public void TryValidateToken_TokenOlderThanConfiguredMaxAge_Fails()
    {
        var sut = CreateSut();
        var token = sut.IssueToken("user-1");

        timeProvider.Advance(TimeSpan.FromMinutes(16));

        var isValid = sut.TryValidateToken(token, "user-1", out var failureReason);

        isValid.Should().BeFalse();
        failureReason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TryValidateToken_TokenExactlyAtMaxAge_Succeeds()
    {
        var sut = CreateSut();
        var token = sut.IssueToken("user-1");

        timeProvider.Advance(TimeSpan.FromMinutes(15));

        var isValid = sut.TryValidateToken(token, "user-1", out _);

        isValid.Should().BeTrue();
    }
}