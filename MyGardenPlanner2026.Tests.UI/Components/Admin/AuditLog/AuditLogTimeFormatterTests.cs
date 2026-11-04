namespace MyGardenPlanner2026.Tests.UI.Components.Admin.AuditLog;

using FluentAssertions;
using MyGardenPlanner2026.Components.Domain.Admin;
using System.Globalization;
using Xunit;

public class AuditLogTimeFormatterTests
{
    private static readonly CultureInfo Danish = new("da-DK");

    private static string Expected(int year, int month, int day, int hour, int minute) =>
        $"{new DateTime(year, month, day, hour, minute, 0).ToString("g", Danish)} UTC";

    [Fact]
    public void Format_UtcValue_ShowsSameTimeWithUtcSuffix()
    {
        var timestamp = new DateTimeOffset(2026, 10, 15, 23, 30, 0, TimeSpan.Zero);

        AuditLogTimeFormatter.Format(timestamp).Should().Be(Expected(2026, 10, 15, 23, 30));
    }

    [Fact]
    public void Format_ValueWithOffset_IsConvertedToUtc()
    {
        var timestamp = new DateTimeOffset(2026, 10, 16, 1, 30, 0, TimeSpan.FromHours(2));

        AuditLogTimeFormatter.Format(timestamp).Should().Be(Expected(2026, 10, 15, 23, 30));
    }
}