namespace MyGardenPlanner2026.Tests.Unit.Services.AuditLog.ExportJobs;

using FluentAssertions;
using MyGardenPlanner2026.Infrastructure.Services.AuditLog.ExportJobs;
using Xunit;

public sealed class SizeLimitedStreamTests
{
    private sealed class NonSeekableStream : MemoryStream
    {
        public override bool CanSeek => false;
    }

    private static (MemoryStream Inner, SizeLimitedStream Sut) Create(long limit)
    {
        var inner = new MemoryStream();
        return (inner, new SizeLimitedStream(inner, limit));
    }

    [Fact]
    public async Task WriteAsync_WithinLimit_PassesDataThrough()
    {
        var (inner, sut) = Create(10);

        await sut.WriteAsync(new byte[] { 1, 2, 3 }, TestContext.Current.CancellationToken);

        inner.ToArray().Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task WriteAsync_ExactlyAtLimit_Succeeds()
    {
        var (inner, sut) = Create(3);

        await sut.WriteAsync(new byte[3], TestContext.Current.CancellationToken);

        inner.Length.Should().Be(3);
    }

    [Fact]
    public async Task WriteAsync_OneByteOverLimit_ThrowsAndWritesNothing()
    {
        var (inner, sut) = Create(3);

        var act = async () => await sut.WriteAsync(new byte[4], TestContext.Current.CancellationToken);

        var ex = await act.Should().ThrowAsync<ExportSizeLimitExceededException>();
        ex.Which.LimitBytes.Should().Be(3);
        inner.Length.Should().Be(0);
    }

    [Fact]
    public async Task WriteAsync_SecondWriteCrossesLimit_KeepsFirstWriteAndRejectsSecond()
    {
        var (inner, sut) = Create(5);
        await sut.WriteAsync(new byte[3], TestContext.Current.CancellationToken);

        var act = async () => await sut.WriteAsync(new byte[3], TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ExportSizeLimitExceededException>();
        inner.Length.Should().Be(3);
    }

    [Fact]
    public async Task WriteAsync_ArrayOverload_OverLimit_Throws()
    {
        var (_, sut) = Create(3);

#pragma warning disable CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
        var act = async () => await sut.WriteAsync(new byte[4], 0, 4, TestContext.Current.CancellationToken);
#pragma warning restore CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'

        await act.Should().ThrowAsync<ExportSizeLimitExceededException>();
    }

    [Fact]
    public void Write_ArrayOverLimit_Throws()
    {
        var (_, sut) = Create(3);

        var act = () => sut.Write(new byte[4], 0, 4);

        act.Should().Throw<ExportSizeLimitExceededException>();
    }

    [Fact]
    public void Write_SpanOverLimit_Throws()
    {
        var (_, sut) = Create(3);

        var act = () => sut.Write(new ReadOnlySpan<byte>(new byte[4]));

        act.Should().Throw<ExportSizeLimitExceededException>();
    }

    [Fact]
    public void WriteByte_OverLimit_Throws()
    {
        var (_, sut) = Create(1);
        sut.WriteByte(1);

        var act = () => sut.WriteByte(2);

        act.Should().Throw<ExportSizeLimitExceededException>();
    }

    [Fact]
    public async Task WriteAsync_RewritingAfterSeek_DoesNotCountBytesTwice()
    {
        var (inner, sut) = Create(4);
        await sut.WriteAsync(new byte[4], TestContext.Current.CancellationToken);

        sut.Position = 0;
        await sut.WriteAsync("\t\t\t\t"u8.ToArray(), TestContext.Current.CancellationToken);

        inner.ToArray().Should().Equal("\t\t\t\t"u8.ToArray());
    }

    [Fact]
    public void SetLength_AboveLimit_Throws()
    {
        var (_, sut) = Create(10);

        var act = () => sut.SetLength(11);

        act.Should().Throw<ExportSizeLimitExceededException>();
    }

    [Fact]
    public void Properties_AreForwardedFromInnerStream()
    {
        var (inner, sut) = Create(10);
        inner.Write([1, 2]);

        sut.CanRead.Should().BeTrue();
        sut.CanSeek.Should().BeTrue();
        sut.CanWrite.Should().BeTrue();
        sut.Length.Should().Be(2);
        sut.Position.Should().Be(2);
    }

    [Fact]
    public void Read_ReturnsDataFromInnerStream()
    {
        var (inner, sut) = Create(10);
        inner.Write([7, 8]);
        sut.Position = 0;
        var buffer = new byte[2];

        var read = sut.Read(buffer, 0, 2);

        read.Should().Be(2);
        buffer.Should().Equal(7, 8);
    }

    [Fact]
    public void Constructor_NonSeekableStream_ThrowsArgumentException()
    {
        var act = () => new SizeLimitedStream(new NonSeekableStream(), 10);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_NonPositiveLimit_ThrowsArgumentOutOfRangeException(long limit)
    {
        var act = () => new SizeLimitedStream(new MemoryStream(), limit);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Dispose_DoesNotDisposeInnerStream()
    {
        var inner = new MemoryStream();
        var sut = new SizeLimitedStream(inner, 10);

        sut.Dispose();

        inner.CanWrite.Should().BeTrue();
    }
}