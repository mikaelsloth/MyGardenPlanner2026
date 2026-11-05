namespace MyGardenPlanner2026.Infrastructure.Services.AuditLog.ExportJobs;

/// <summary>
/// Omslutter en søgbar stream og kaster <see cref="ExportSizeLimitExceededException"/>, FØR en
/// skrivning (eller SetLength) ville få indholdet over <c>maxBytes</c>. Størrelsen er den
/// resulterende længde, ikke summen af skrevne bytes, så skrivning efter Seek (fx ZIP/xlsx)
/// ikke tælles to gange. Disposer IKKE den omsluttede stream — ejeren gør det.
/// </summary>
public sealed class SizeLimitedStream : Stream
{
    private readonly Stream inner;
    private readonly long maxBytes;

    public SizeLimitedStream(Stream inner, long maxBytes)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxBytes);

        if (!inner.CanSeek)
        {
            throw new ArgumentException("Streamen skal understøtte Seek.", nameof(inner));
        }

        this.inner = inner;
        this.maxBytes = maxBytes;
    }

    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => inner.CanSeek;
    public override bool CanWrite => inner.CanWrite;
    public override long Length => inner.Length;

    public override long Position
    {
        get => inner.Position;
        set => inner.Position = value;
    }

    public override void Flush() => inner.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken) => inner.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

    public override int Read(Span<byte> buffer) => inner.Read(buffer);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        inner.ReadAsync(buffer, cancellationToken);

    public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

    public override void SetLength(long value)
    {
        EnsureWithinLimit(value);
        inner.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        EnsureCanAppend(count);
        inner.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        EnsureCanAppend(buffer.Length);
        inner.Write(buffer);
    }

    public override void WriteByte(byte value)
    {
        EnsureCanAppend(1);
        inner.WriteByte(value);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        EnsureCanAppend(buffer.Length);
        await inner.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    private void EnsureCanAppend(int count) =>
        EnsureWithinLimit(Math.Max(inner.Length, inner.Position + count));

    private void EnsureWithinLimit(long resultingSize)
    {
        if (resultingSize > maxBytes)
        {
            throw new ExportSizeLimitExceededException(maxBytes);
        }
    }
}