namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public class SmtpOptionsTests
{
    [Fact]
    public void DefaultOptions_HasPort25AndSslEnabled()
    {
        var options = new SmtpOptions();

        options.Port.Should().Be(25);
        options.EnableSsl.Should().BeTrue();
    }

    [Fact]
    public void DefaultOptions_HasEmptyAdminSecurityEmailsList()
    {
        var options = new SmtpOptions();

        options.AdminSecurityEmails.Should().BeEmpty();
    }
}