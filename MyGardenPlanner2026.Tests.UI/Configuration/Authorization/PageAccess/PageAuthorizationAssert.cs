namespace MyGardenPlanner2026.Tests.UI.Configuration.Authorization.PageAccess;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Fælles assert for, at en side er beskyttet af en bestemt authorization policy.
/// </summary>
public static class PageAuthorizationAssert
{
    /// <summary>
    /// Asserter, at <typeparamref name="TPage"/> har præcis én <see cref="AuthorizeAttribute"/> (inkl. nedarvede)
    /// med policyen <paramref name="expectedPolicy"/>.
    /// </summary>
    public static void RequiresPolicy<TPage>(string expectedPolicy)
    {
        var attribute = typeof(TPage)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        attribute.Should().NotBeNull();
        attribute!.Policy.Should().Be(expectedPolicy);
    }
}