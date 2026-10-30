namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;

public sealed class PerTestResourcesTests
{
    private readonly List<string> _closed = [];
    private string? _testId = "t1";

    private PerTestResources<string> Create() =>
        new(() => _testId, item => { _closed.Add(item); return Task.CompletedTask; });

    [Fact]
    public async Task SammeTest_LukkerIkkeEksisterendeRessourcer()
    {
        var resources = Create();

        await resources.ReleaseIfNewTestAsync();
        resources.Add("a");
        await resources.ReleaseIfNewTestAsync();
        resources.Add("b");
        await resources.ReleaseIfNewTestAsync();

        _closed.Should().BeEmpty();
        resources.Count.Should().Be(2);
    }

    [Fact]
    public async Task NyTest_LukkerRessourcerFraForrigeTest()
    {
        var resources = Create();

        await resources.ReleaseIfNewTestAsync();
        resources.Add("a");
        resources.Add("b");

        _testId = "t2";
        await resources.ReleaseIfNewTestAsync();

        _closed.Should().Equal("a", "b");
        resources.Count.Should().Be(0);
    }

    [Fact]
    public async Task UkendtTestId_LukkerIkke()
    {
        var resources = Create();
        await resources.ReleaseIfNewTestAsync();
        resources.Add("a");

        _testId = null;
        await resources.ReleaseIfNewTestAsync();

        _closed.Should().BeEmpty();
    }

    [Fact]
    public async Task CloseAllAsync_LukkerAltOgTommerListen()
    {
        var resources = Create();
        resources.Add("a");
        resources.Add("b");

        await resources.CloseAllAsync();

        _closed.Should().Equal("a", "b");
        resources.Count.Should().Be(0);
    }
}