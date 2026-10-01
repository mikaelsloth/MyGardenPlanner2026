namespace MyGardenPlanner2026.Tests.Unit.Internal_Tests;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Common;
using Xunit;

public sealed class FakeCurrentUserTests
{
    [Fact]
    public void Create_ReturnsAccessorWithProvidedUser()
    {
        var accessor = FakeCurrentUser.Create("user-1", "a@example.dk", "10.0.0.1");

        accessor.GetCurrent().Should().BeEquivalentTo(new CurrentUserInfo("user-1", "a@example.dk", "10.0.0.1"));
    }

    [Fact]
    public void Create_NullIpAddress_IsPassedThrough()
    {
        var accessor = FakeCurrentUser.Create("system", "system@example.dk", null);

        accessor.GetCurrent().Should().BeEquivalentTo(new CurrentUserInfo("system", "system@example.dk", null));
    }

    [Fact]
    public void Create_RepeatedCalls_ReturnSameUser()
    {
        var accessor = FakeCurrentUser.Create("user-1", "a@example.dk", "10.0.0.1");

        accessor.GetCurrent().Should().BeEquivalentTo(accessor.GetCurrent());
    }

    [Fact]
    public void Create_TwoAccessors_AreIndependent()
    {
        var first = FakeCurrentUser.Create("user-1", "a@example.dk", "10.0.0.1");
        var second = FakeCurrentUser.Create("user-2", "b@example.dk", "10.0.0.2");

        first.GetCurrent().Should().NotBeEquivalentTo(second.GetCurrent());
    }
}