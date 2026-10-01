namespace MyGardenPlanner2026.Tests.Unit;

using MyGardenPlanner2026.Core.Contracts.Common;
using NSubstitute;

/// <summary>
/// Opretter en ICurrentUserAccessor-substitut, der altid returnerer den angivne bruger.
/// Bruges af interceptor- og persistens-tests (SoftDelete, AuditLogging).
/// </summary>
internal static class FakeCurrentUser
{
    public static ICurrentUserAccessor Create(string userId, string email, string? ipAddress)
    {
        var accessor = Substitute.For<ICurrentUserAccessor>();
        accessor.GetCurrent().Returns(new CurrentUserInfo(userId, email, ipAddress));
        return accessor;
    }
}