namespace MyGardenPlanner2026.Tests.Unit.Common;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Common;
using Xunit;

public class SystemActorContextTests
{
    [Fact]
    public void Current_WithoutScope_IsNull() =>
        SystemActorContext.Current.Should().BeNull();

    [Fact]
    public void Begin_Generic_UsesTypeAndCallerMemberName()
    {
        using var scope = SystemActorContext.Begin<SystemActorContextTests>();

        SystemActorContext.Current.Should().Be(
            $"system-{nameof(SystemActorContextTests)}.{nameof(Begin_Generic_UsesTypeAndCallerMemberName)}");
    }

    [Fact]
    public void Begin_Explicit_UsesGivenNames()
    {
        using var scope = SystemActorContext.Begin("Seeder", "SeedAsync");

        SystemActorContext.Current.Should().Be("system-Seeder.SeedAsync");
    }

    [Fact]
    public void Dispose_RestoresPreviousValue()
    {
        using var outer = SystemActorContext.Begin("Outer", "Run");

        using (SystemActorContext.Begin("Inner", "Run"))
        {
            SystemActorContext.Current.Should().Be("system-Inner.Run");
        }

        SystemActorContext.Current.Should().Be("system-Outer.Run");
    }

    [Fact]
    public void Dispose_Twice_DoesNotAffectOuterScope()
    {
        var outer = SystemActorContext.Begin("Outer", "Run");
        var inner = SystemActorContext.Begin("Inner", "Run");

        inner.Dispose();
        inner.Dispose();

        SystemActorContext.Current.Should().Be("system-Outer.Run");

        outer.Dispose();
        SystemActorContext.Current.Should().BeNull();
    }

    [Theory]
    [InlineData("", "Run")]
    [InlineData("Class", " ")]
    public void Begin_WithBlankNames_Throws(string className, string method)
    {
        var act = () => SystemActorContext.Begin(className, method);

        act.Should().Throw<ArgumentException>();
    }
}