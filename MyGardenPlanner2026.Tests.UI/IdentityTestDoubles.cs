namespace MyGardenPlanner2026.Tests.UI;

using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Components.Account;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities;
using NSubstitute;

/// <summary>
/// Fælles test-infrastruktur for Account/Pages-tests. Indkapsler NSubstitute-opsætning
/// af UserManager/SignInManager (mange konstruktørparametre) samt adgang til den interne
/// IdentityRedirectManager (kræver InternalsVisibleTo, se hovedprojektets .csproj).
/// </summary>
public static class IdentityTestDoubles
{
    public static UserManager<ApplicationUser> CreateUserManager() =>
        Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

    public static SignInManager<ApplicationUser> CreateSignInManager(UserManager<ApplicationUser> userManager) =>
        Substitute.For<SignInManager<ApplicationUser>>(
            userManager,
            Substitute.For<IHttpContextAccessor>(),
            Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null, null, null, null);

    /// <summary>
    /// Opretter og registrerer UserManager og SignInManager (substitutter) i bUnit's Services.
    /// </summary>
    public static (UserManager<ApplicationUser> UserManager, SignInManager<ApplicationUser> SignInManager)
        RegisterIdentityFakes(this BunitContext context)
    {
        var userManager = CreateUserManager();
        var signInManager = CreateSignInManager(userManager);

        context.Services.AddSingleton(userManager);
        context.Services.AddSingleton(signInManager);

        return (userManager, signInManager);
    }

    /// <summary>
    /// Registrerer de fakes, som re-auth-flowet i login- og step-up-komponenter kræver:
    /// IReAuthenticationService, IReAuthFailureTracker, ICurrentUserAccessor (127.0.0.1) og ILogger&lt;T&gt;.
    /// </summary>
    /// <typeparam name="TComponent">Komponenten, hvis logger-kategori registreres.</typeparam>
    /// <returns>IReAuthenticationService-fakeen, så tests kan verificere kald.</returns>
    public static IReAuthenticationService RegisterReAuthFakes<TComponent>(this BunitContext context)
    {
        var reAuthenticationService = Substitute.For<IReAuthenticationService>();

        var currentUserAccessor = Substitute.For<ICurrentUserAccessor>();
        currentUserAccessor.GetCurrent().Returns(new CurrentUserInfo(null, null, "127.0.0.1"));

        context.Services.AddSingleton(reAuthenticationService);
        context.Services.AddSingleton(Substitute.For<IReAuthFailureTracker>());
        context.Services.AddSingleton(currentUserAccessor);
        context.Services.AddSingleton(Substitute.For<ILogger<TComponent>>());

        return reAuthenticationService;
    }

    /// <summary>
    /// Opsætter en bruger, der afventer tofaktor-login: SignInManager.GetTwoFactorAuthenticationUserAsync
    /// returnerer brugeren, og UserManager.GetUserIdAsync returnerer <paramref name="userId"/>.
    /// </summary>
    public static ApplicationUser SetupTwoFactorLoginUser(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        string userId = "user-1")
    {
        var user = new ApplicationUser { Id = userId };
        userManager.GetUserIdAsync(user).Returns(Task.FromResult(userId));
        signInManager.GetTwoFactorAuthenticationUserAsync().Returns(Task.FromResult<ApplicationUser?>(user));

        return user;
    }

    /// <summary>
    /// HttpContext med en substitueret IAuthenticationService i RequestServices.
    /// Nødvendig for sider (fx Login), der kalder HttpContext.SignOutAsync(...).
    /// </summary>
    public static DefaultHttpContext CreateHttpContextWithAuthService(string httpMethod = "GET")
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IAuthenticationService>());

        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        httpContext.Request.Method = httpMethod;
        return httpContext;
    }

    /// <summary>
    /// Registrerer en reel IdentityRedirectManager bundet til bUnit's fake NavigationManager,
    /// så navigation fra RedirectManager kan verificeres via den returnerede instans.
    /// </summary>
    public static BunitNavigationManager UseIdentityRedirectManager(this BunitContext context)
    {
        // AddSingleton FØR enhver GetRequiredService — bUnit låser containeren
        // for yderligere registreringer, så snart første service er hentet.
        context.Services.AddSingleton<IdentityRedirectManager>(sp =>
            new IdentityRedirectManager(sp.GetRequiredService<NavigationManager>()));

        return context.Services.GetRequiredService<BunitNavigationManager>();
    }
}