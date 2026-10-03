namespace MyGardenPlanner2026.Tests.UI;

using Bunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities;
using NSubstitute;
using System.Security.Claims;

/// <summary>
/// Fælles test-doubles og service-registreringer for admin-editorer og sikkerhedskomponenter,
/// der kræver step-up re-autentificering, rate limiting og brugerhåndtering.
/// </summary>
public static class AdminSecurityTestDoubles
{
    /// <summary>
    /// Registrerer standard fakes for IAuthorizationService (RequireRecentAuthenticationPolicy),
    /// UserManager, IReAuthenticationService, IReAuthFailureTracker, ICurrentUserAccessor
    /// og IAdminActionRateLimiter på bUnit context Services.
    /// </summary>
    public static (IAuthorizationService AuthorizationService, IAdminActionRateLimiter RateLimiter) AddAdminSecurityServices(
        this BunitContext context,
        bool reAuthSucceeds = true,
        bool rateLimitAllowed = true)
    {
        var authorizationService = Substitute.For<IAuthorizationService>();
        SetReAuthResult(authorizationService, reAuthSucceeds);
        context.Services.AddSingleton(authorizationService);

        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(null));
        context.Services.AddSingleton(userManager);

        context.Services.AddSingleton(Substitute.For<IReAuthenticationService>());
        context.Services.AddSingleton(Substitute.For<IReAuthFailureTracker>());
        context.Services.AddSingleton(Substitute.For<ICurrentUserAccessor>());

        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        rateLimiter.TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(rateLimitAllowed));
        context.Services.AddSingleton(rateLimiter);

        return (authorizationService, rateLimiter);
    }

    /// <summary>
    /// Konfigurerer om IAuthorizationService godkender eller afviser RequireRecentAuthenticationPolicy.
    /// </summary>
    public static void SetReAuthResult(this IAuthorizationService authorizationService, bool succeeds)
    {
        authorizationService.AuthorizeAsync(
                Arg.Any<ClaimsPrincipal>(),
                Arg.Any<object?>(),
                Arg.Is(AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy))
            .Returns(Task.FromResult(succeeds ? AuthorizationResult.Success() : AuthorizationResult.Failed()));

        authorizationService.AuthorizeAsync(
                Arg.Any<ClaimsPrincipal>(),
                Arg.Any<object?>(),
                Arg.Any<string>())
            .Returns(Task.FromResult(succeeds ? AuthorizationResult.Success() : AuthorizationResult.Failed()));
    }
}