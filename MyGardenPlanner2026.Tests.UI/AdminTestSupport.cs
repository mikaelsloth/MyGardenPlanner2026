namespace MyGardenPlanner2026.Tests.UI;

using Bunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities;
using NSubstitute;
using System.Security.Claims;

/// <summary>
/// De registrerede fakes, som admin-tests kan justere undervejs (fx skifte re-auth eller rate limit midt i en test).
/// </summary>
public sealed record AdminStepUpFakes(IAuthorizationService AuthorizationService, IAdminActionRateLimiter RateLimiter);

/// <summary>
/// Fælles bUnit-opsætning for admin-komponenter med step-up re-authentication og rate limiting.
/// </summary>
public static class AdminTestSupport
{
    /// <summary>
    /// Registrerer IAuthorizationService (RequireRecentAuthentication-policy), UserManager (GetUserAsync → null),
    /// IAdminActionRateLimiter samt re-auth-fakes og ILogger&lt;<typeparamref name="TComponent"/>&gt;.
    /// Skal kaldes før første render/GetRequiredService.
    /// </summary>
    /// <param name="reAuthSucceeds">Om RequireRecentAuthentication-policyen lykkes (ellers åbnes step-up-modalen).</param>
    /// <param name="rateLimiterPermits">Om rate limiteren giver tilladelse.</param>
    public static AdminStepUpFakes RegisterAdminStepUpFakes<TComponent>(
        this BunitContext context, bool reAuthSucceeds = true, bool rateLimiterPermits = true)
    {
        var authorizationService = Substitute.For<IAuthorizationService>();
        authorizationService.AuthorizeAsync(
                Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Is(AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy))
            .Returns(Task.FromResult(reAuthSucceeds ? AuthorizationResult.Success() : AuthorizationResult.Failed()));

        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(null));

        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        rateLimiter.TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(rateLimiterPermits));

        context.Services.AddSingleton(authorizationService);
        context.Services.AddSingleton(userManager);
        context.Services.AddSingleton(rateLimiter);
        context.RegisterReAuthFakes<TComponent>();

        return new AdminStepUpFakes(authorizationService, rateLimiter);
    }
}