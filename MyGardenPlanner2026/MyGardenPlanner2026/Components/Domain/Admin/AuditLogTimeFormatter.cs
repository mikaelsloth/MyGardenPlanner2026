namespace MyGardenPlanner2026.Components.Domain.Admin;

using System.Globalization;

/// <summary>
/// Fælles visning af AuditLog-tidspunkter. Altid UTC med suffikset " UTC", så visningen
/// matcher filteret (datoer tolkes som UTC) og eksporten (TimestampUtc). ToLocalTime()
/// bruges bevidst ikke: i Blazor Server giver den serverens tidszone, ikke brugerens.
/// </summary>
internal static class AuditLogTimeFormatter
{
    private static readonly CultureInfo DanishCulture = new("da-DK");

    public static string Format(DateTimeOffset timestamp) =>
        $"{timestamp.UtcDateTime.ToString("g", DanishCulture)} UTC";
}