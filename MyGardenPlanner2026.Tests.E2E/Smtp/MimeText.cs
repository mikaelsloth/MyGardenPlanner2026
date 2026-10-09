namespace MyGardenPlanner2026.Tests.E2E.Smtp;

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Dekoder MIME-tekst, som System.Net.Mail sender den: RFC 2047 encoded-words i headere
/// (B/Q) og base64/quoted-printable i kroppen. Dækker kun enkeltdels plain-text.
/// </summary>
public static partial class MimeText
{
    [GeneratedRegex(@"(\?=)\s+(=\?)")]
    private static partial Regex WhitespaceBetweenEncodedWords { get; }

    [GeneratedRegex(@"=\?([^?]+)\?([bBqQ])\?([^?]*)\?=")]
    private static partial Regex EncodedWord { get; }

    [GeneratedRegex(@"charset=""?([^"";\s]+)""?", RegexOptions.IgnoreCase)]
    private static partial Regex Charset { get; }

    public static string DecodeHeader(string value)
    {
        // RFC 2047: mellemrum mellem to nabo-encoded-words er ikke en del af teksten.
        var joined = WhitespaceBetweenEncodedWords.Replace(value, "$1$2");

        return EncodedWord.Replace(joined, match =>
        {
            var encoding = GetEncoding(match.Groups[1].Value);
            var text = match.Groups[3].Value;
            var bytes = char.ToUpperInvariant(match.Groups[2].Value[0]) == 'B'
                ? Convert.FromBase64String(text)
                : DecodeQuotedPrintable(text.Replace('_', ' '));

            return encoding.GetString(bytes);
        });
    }

    public static string DecodeBody(IReadOnlyList<string> lines, string? transferEncoding, Encoding charset)
    {
        return (transferEncoding?.Trim().ToLowerInvariant()) switch
        {
            "base64" => charset.GetString(Convert.FromBase64String(string.Concat(lines))),
            "quoted-printable" => charset.GetString(DecodeQuotedPrintableLines(lines)),
            _ => string.Join("\r\n", lines),
        };
    }

    public static Encoding CharsetFromContentType(string? contentType)
    {
        var match = contentType is null ? null : Charset.Match(contentType);
        return match is { Success: true } ? GetEncoding(match.Groups[1].Value) : Encoding.UTF8;
    }

    private static Encoding GetEncoding(string? charset)
    {
        try
        {
            return string.IsNullOrWhiteSpace(charset) ? Encoding.UTF8 : Encoding.GetEncoding(charset);
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8;
        }
    }

    private static byte[] DecodeQuotedPrintableLines(IReadOnlyList<string> lines)
    {
        using var buffer = new MemoryStream();

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var softBreak = line.EndsWith('=');

            buffer.Write(DecodeQuotedPrintable(softBreak ? line[..^1] : line));

            if (!softBreak && i < lines.Count - 1)
            {
                buffer.Write("\r\n"u8);
            }
        }

        return buffer.ToArray();
    }

    private static byte[] DecodeQuotedPrintable(string text)
    {
        using var buffer = new MemoryStream();

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '=' && i + 2 < text.Length)
            {
                buffer.WriteByte(byte.Parse(text.AsSpan(i + 1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                i += 2;
            }
            else
            {
                buffer.WriteByte((byte)text[i]);
            }
        }

        return buffer.ToArray();
    }
}