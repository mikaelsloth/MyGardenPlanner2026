namespace MyGardenPlanner2026.Tests.E2E.Smtp;

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// Minimal SMTP-server til E2E: lytter på en ledig loopback-port, svarer nok på protokollen
/// til System.Net.Mail.SmtpClient (ingen TLS, ingen AUTH) og gemmer modtagne mails. Appen
/// peges hertil via Smtp__*-miljøvariabler i PlaywrightAppFixture.
/// </summary>
public sealed class TestSmtpServer : IAsyncDisposable
{
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _acceptLoop;
    private readonly ConcurrentQueue<ReceivedMail> _messages = new();

    private TestSmtpServer()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _acceptLoop = Task.Run(() => AcceptLoopAsync(_cts.Token), _cts.Token);
    }

    public int Port { get; }

    public IReadOnlyList<ReceivedMail> Messages => [.. _messages];

    public static TestSmtpServer Start() => new();

    public async Task<ReceivedMail> WaitForMessageAsync(Func<ReceivedMail, bool> predicate, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(10));

        while (true)
        {
            var match = _messages.FirstOrDefault(predicate);
            if (match is not null)
            {
                return match;
            }

            if (DateTime.UtcNow >= deadline)
            {
                throw new TimeoutException(
                    "Ingen modtaget mail matchede inden for tidsgrænsen. " +
                    $"Modtagne emner: [{string.Join(" | ", _messages.Select(m => m.Subject))}]");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50), _cts.Token);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        _listener.Stop();

        try
        {
            await _acceptLoop;
        }
        catch (OperationCanceledException)
        {
            // forventet ved nedlukning
        }

        _cts.Dispose();
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await _listener.AcceptTcpClientAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException or SocketException)
            {
                return;
            }

            _ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            using (client)
            {
                var stream = client.GetStream();
                var utf8 = new UTF8Encoding(false);
                using var reader = new StreamReader(stream, utf8);
                await using var writer = new StreamWriter(stream, utf8) { NewLine = "\r\n", AutoFlush = true };

                await RunSessionAsync(reader, writer, cancellationToken);
            }
        }
        catch (Exception ex) when (ex is IOException or SocketException or OperationCanceledException or ObjectDisposedException)
        {
            // klienten lukkede forbindelsen eller sinken blev lukket ned
        }
    }

    private async Task RunSessionAsync(StreamReader reader, StreamWriter writer, CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync("220 localhost ESMTP test-sink");

        var from = string.Empty;
        var recipients = new List<string>();

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            switch (line.Split(' ', 2)[0].ToUpperInvariant())
            {
                case "EHLO":
                case "HELO":
                    await writer.WriteLineAsync("250 localhost");
                    break;
                case "MAIL":
                    from = ExtractAddress(line);
                    await writer.WriteLineAsync("250 OK");
                    break;
                case "RCPT":
                    recipients.Add(ExtractAddress(line));
                    await writer.WriteLineAsync("250 OK");
                    break;
                case "DATA":
                    await writer.WriteLineAsync("354 End data with <CR><LF>.<CR><LF>");
                    var data = await ReadDataAsync(reader, cancellationToken);
                    _messages.Enqueue(ParseMessage(from, [.. recipients], data));
                    from = string.Empty;
                    recipients.Clear();
                    await writer.WriteLineAsync("250 OK queued");
                    break;
                case "RSET":
                    from = string.Empty;
                    recipients.Clear();
                    await writer.WriteLineAsync("250 OK");
                    break;
                case "NOOP":
                    await writer.WriteLineAsync("250 OK");
                    break;
                case "QUIT":
                    await writer.WriteLineAsync("221 Bye");
                    return;
                default:
                    await writer.WriteLineAsync("502 Command not implemented");
                    break;
            }
        }
    }

    private static async Task<List<string>> ReadDataAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        var lines = new List<string>();

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (line == ".")
            {
                break;
            }

            // Fjern SMTP dot-stuffing.
            lines.Add(line.StartsWith("..", StringComparison.Ordinal) ? line[1..] : line);
        }

        return lines;
    }

    private static string ExtractAddress(string line)
    {
        var start = line.IndexOf('<');
        var end = line.IndexOf('>', start + 1);
        return start >= 0 && end > start ? line[(start + 1)..end] : line;
    }

    private static ReceivedMail ParseMessage(string from, IReadOnlyList<string> recipients, List<string> dataLines)
    {
        var headerEnd = dataLines.IndexOf(string.Empty);
        var headerLines = headerEnd < 0 ? dataLines : [.. dataLines.Take(headerEnd)];
        var bodyLines = headerEnd < 0 ? [] : dataLines.Skip(headerEnd + 1).ToList();

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? currentName = null;

        foreach (var line in headerLines)
        {
            if ((line.StartsWith(' ') || line.StartsWith('\t')) && currentName is not null)
            {
                headers[currentName] = $"{headers[currentName]} {line.Trim()}";
                continue;
            }

            var separator = line.IndexOf(':');
            if (separator <= 0)
            {
                continue;
            }

            currentName = line[..separator];
            headers[currentName] = line[(separator + 1)..].Trim();
        }

        var subject = MimeText.DecodeHeader(headers.GetValueOrDefault("Subject", string.Empty));
        var charset = MimeText.CharsetFromContentType(headers.GetValueOrDefault("Content-Type"));
        var body = MimeText.DecodeBody(bodyLines, headers.GetValueOrDefault("Content-Transfer-Encoding"), charset);

        return new ReceivedMail(from, recipients, subject, body);
    }
}