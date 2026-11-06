namespace MyGardenPlanner2026.Tests.UI.Components.Foundation;

using FluentAssertions;
using MyGardenPlanner2026.Components.Foundation;
using Xunit;

public class AriaExtensionsTests
{
    [Fact]
    public void ToAriaValue_True_ReturnsLowercaseTrue() =>
        true.ToAriaValue().Should().Be("true");

    [Fact]
    public void ToAriaValue_False_ReturnsLowercaseFalse() =>
        false.ToAriaValue().Should().Be("false");
}