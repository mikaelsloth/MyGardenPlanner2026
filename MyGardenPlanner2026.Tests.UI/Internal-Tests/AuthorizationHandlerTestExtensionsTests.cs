namespace MyGardenPlanner2026.Tests.UI.Internal_Tests;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Tests.UI.Configuration.Authorization;
using Xunit;

public sealed class AuthorizationHandlerTestExtensionsTests
{
    private sealed class TestRequirement : IAuthorizationRequirement
    {
    }

    private sealed class SucceedWhenHandler(Func<AuthorizationHandlerContext, bool> predicate)
        : AuthorizationHandler<TestRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TestRequirement requirement)
        {
            if (predicate(context))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task EvaluateAsync_HandlerSucceedsRequirement_ReturnsTrue()
    {
        var handler = new SucceedWhenHandler(_ => true);

        var succeeded = await handler.EvaluateAsync(new TestRequirement(), TestPrincipals.Create());

        succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_HandlerDoesNotSucceedRequirement_ReturnsFalse()
    {
        var handler = new SucceedWhenHandler(_ => false);

        var succeeded = await handler.EvaluateAsync(new TestRequirement(), TestPrincipals.Create());

        succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_PassesUserAndResourceToHandler()
    {
        var handler = new SucceedWhenHandler(c =>
            c.Resource is string resource && resource == "garden-1" && c.User.IsInRole("SystemAdmin"));

        var succeeded = await handler.EvaluateAsync(
            new TestRequirement(), TestPrincipals.Create("user-1", "SystemAdmin"), "garden-1");

        succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_NoResource_PassesNullResourceToHandler()
    {
        var handler = new SucceedWhenHandler(c => c.Resource is null);

        var succeeded = await handler.EvaluateAsync(new TestRequirement(), TestPrincipals.Create());

        succeeded.Should().BeTrue();
    }
}