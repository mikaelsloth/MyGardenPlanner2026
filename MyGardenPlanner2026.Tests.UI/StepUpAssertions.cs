namespace MyGardenPlanner2026.Tests.UI;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Fælles asserts for step-up modal og rate limit-besked i admin-komponenter.
/// </summary>
public static class StepUpAssertions
{
    public const string RateLimitMessage = "For mange handlinger";

    private const string StepUpModalSelector = ".confirm-dialog";

    public static void ShouldShowStepUpModal<TComponent>(this IRenderedComponent<TComponent> cut)
        where TComponent : IComponent
    {
        cut.FindAll(StepUpModalSelector).Should().HaveCount(1);
    }

    public static void ShouldNotShowStepUpModal<TComponent>(this IRenderedComponent<TComponent> cut)
        where TComponent : IComponent
    {
        cut.FindAll(StepUpModalSelector).Should().BeEmpty();
    }

    public static void ShouldShowRateLimitMessage<TComponent>(this IRenderedComponent<TComponent> cut)
        where TComponent : IComponent
    {
        cut.Markup.Should().Contain(RateLimitMessage);
    }
}