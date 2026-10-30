namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;

public sealed class AppLogFileTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"mgp-applog-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }

    [Fact]
    public void WriteLine_SkriverTidsstempelOgLinje()
    {
        var path = Path.Combine(_dir, "sub", "app.log");
        var now = new DateTime(2026, 10, 30, 12, 34, 56, 789, DateTimeKind.Utc);

        using (var log = new AppLogFile(path, () => now))
        {
            log.WriteLine("hej");
            log.WriteLine("farvel");
        }

        File.ReadAllLines(path).Should().Equal("12:34:56.789 hej", "12:34:56.789 farvel");
    }

    [Fact]
    public void WriteLine_EfterDispose_KasterIkkeOgSkriverIkke()
    {
        var path = Path.Combine(_dir, "app.log");
        var log = new AppLogFile(path);
        log.Dispose();

        var act = () => log.WriteLine("for sent");

        act.Should().NotThrow();
        File.ReadAllText(path).Should().BeEmpty();
    }

    [Fact]
    public void WriteLine_FraFlereTraade_TabInterIngenLinjer()
    {
        var path = Path.Combine(_dir, "app.log");

        using (var log = new AppLogFile(path))
        {
            Parallel.For(0, 200, i => log.WriteLine($"linje {i}"));
        }

        File.ReadAllLines(path).Should().HaveCount(200);
    }
}