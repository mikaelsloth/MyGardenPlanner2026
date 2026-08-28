namespace MyGardenPlanner2026.Tests.UI.Identity;

using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Account;
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