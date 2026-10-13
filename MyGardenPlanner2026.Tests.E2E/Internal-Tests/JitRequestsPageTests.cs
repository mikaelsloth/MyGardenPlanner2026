namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;

public sealed class JitRequestsPageTests
{
    [Fact]
    public void NewReason_GiverNyIkkeTomVaerdiHverGang()
    {
        var first = JitRequestsPage.NewReason();
        var second = JitRequestsPage.NewReason();

        first.Should().NotBeNullOrWhiteSpace();
        first.Should().NotBe(second);
    }
}