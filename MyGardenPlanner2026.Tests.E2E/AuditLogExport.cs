namespace MyGardenPlanner2026.Tests.E2E;

using ClosedXML.Excel;
using Microsoft.Playwright;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

/// <summary>Svaret fra /admin/audit-log/export.</summary>
public sealed partial record ExportResponse(int Status, string? ContentType, string? ContentDisposition, byte[] Body)
{
    /// <summary>Kroppen som tekst uden BOM (CSV skrives med UTF-8 BOM).</summary>
    public string Text => Encoding.UTF8.GetString(Body).TrimStart('\uFEFF');

    public string? FileName =>
        ContentDisposition is null ? null : FileNamePattern().Match(ContentDisposition) is { Success: true } m ? m.Groups[1].Value : null;

    [GeneratedRegex("filename=\"?([^\";]+)\"?", RegexOptions.IgnoreCase, "da-DK")]
    private static partial Regex FileNamePattern();
}

/// <summary>
/// Eksporten udløses med window.open(url, "_blank"). Et init-script erstatter window.open og gemmer
/// URL'erne, og testen henter dem derefter med browserens cookies (Context.APIRequest). Det verificerer
/// parametre, status, filnavn og indhold, men ikke at browseren selv viser en download.
/// </summary>
public static class AuditLogExport
{
    private const string CaptureScript =
        "window.__mgpOpened = []; window.open = (url) => { window.__mgpOpened.push(String(url)); return null; };";

    public static Task InstallCaptureAsync(IPage page) => page.AddInitScriptAsync(CaptureScript);

    public static async Task<IReadOnlyList<string>> OpenedUrlsAsync(IPage page) =>
        await page.EvaluateAsync<string[]>("() => window.__mgpOpened ?? []");

    /// <summary>Venter på, at der er åbnet flere URL'er end <paramref name="knownCount"/>, og returnerer den nye.</summary>
    public static async Task<string> WaitForNewUrlAsync(IPage page, int knownCount, TimeSpan? timeout = null)
    {
        string? url = null;
        var waitTimeout = timeout ?? TimeSpan.FromSeconds(10);

        var found = await BlazorInteraction.WaitUntilAsync(async () =>
        {
            var urls = await OpenedUrlsAsync(page);
            if (urls.Count <= knownCount)
            {
                return false;
            }

            url = urls[knownCount];
            return true;
        }, waitTimeout);

        return found
            ? url!
            : throw new TimeoutException($"Ingen eksport-URL blev åbnet inden for {waitTimeout}.");
    }

    public static async Task<ExportResponse> FetchAsync(IPage page, string url)
    {
        var response = await page.Context.APIRequest.GetAsync(url);
        await using (response.ConfigureAwait(false))
        {
            response.Headers.TryGetValue("content-type", out var contentType);
            response.Headers.TryGetValue("content-disposition", out var disposition);

            return new ExportResponse(response.Status, contentType, disposition, await response.BodyAsync());
        }
    }

    /// <summary>Henter eksporten med et tilfældigt token (forventet 403).</summary>
    public static Task<ExportResponse> FetchWithInvalidTokenAsync(IPage page, string rootUri, string format = "Csv") =>
        FetchAsync(page, $"{rootUri}/admin/audit-log/export?format={format}&token=ugyldigt");

    public static IReadOnlyList<IReadOnlyList<string>> ParseCsv(string text)
    {
        var rows = new List<IReadOnlyList<string>>();
        List<string> fields = [];
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (inQuotes)
            {
                if (c != '"')
                {
                    field.Append(c);
                }
                else if (i + 1 < text.Length && text[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = false;
                }

                continue;
            }

            switch (c)
            {
                case '"':
                    inQuotes = true;
                    break;
                case ',':
                    fields.Add(field.ToString());
                    field.Clear();
                    break;
                case '\r':
                    break;
                case '\n':
                    fields.Add(field.ToString());
                    field.Clear();
                    rows.Add(fields);
                    fields = [];
                    break;
                default:
                    field.Append(c);
                    break;
            }
        }

        if (field.Length > 0 || fields.Count > 0)
        {
            fields.Add(field.ToString());
            rows.Add(fields);
        }

        return rows;
    }

    public static int JsonRowCount(byte[] body)
    {
        using var document = JsonDocument.Parse(body);
        return document.RootElement.GetArrayLength();
    }

    public static bool IsZip(byte[] body) => body.Length > 3 && body[0] == (byte)'P' && body[1] == (byte)'K';

    /// <summary>Antal datarækker (uden overskriftsrækken) i arket "AuditLog".</summary>
    public static int XlsxRowCount(byte[] body)
    {
        using var stream = new MemoryStream(body);
        using var workbook = new XLWorkbook(stream);
        return workbook.Worksheet("AuditLog").RowsUsed().Count() - 1;
    }
}