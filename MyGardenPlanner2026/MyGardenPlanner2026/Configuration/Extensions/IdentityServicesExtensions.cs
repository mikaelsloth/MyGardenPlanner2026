namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Components.Account;
using MyGardenPlanner2026.Core.Contracts.Email;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.Services.Email;

public static class IdentityServicesExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddCascadingAuthenticationState();
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<PersonalDataExporter>();
        services.AddOptions<EmailLinkDisplayOptions>()
            .BindConfiguration(EmailLinkDisplayOptions.SectionName);
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
            .AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ReturnUrlParameter = "ReturnUrl";
        });

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<PlannerDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<DanishIdentityErrorDescriber>();
        ;

        services.AddOptions<EmailOptions>().BindConfiguration(EmailOptions.SectionName);
        services.AddSingleton<IUserEmailSender, SmtpUserEmailSender>();
        services.AddSingleton<IdentityNoOpEmailSender>();
        services.AddSingleton<IdentityEmailSender>();

        // Provideren vælges ved opstart. NoOp er standard (ShowLinks-logikken afhænger af typen).
        services.AddSingleton<IEmailSender<ApplicationUser>>(sp =>
            sp.GetRequiredService<IOptionsMonitor<EmailOptions>>().CurrentValue.Provider == EmailProvider.Smtp
                ? sp.GetRequiredService<IdentityEmailSender>()
                : sp.GetRequiredService<IdentityNoOpEmailSender>());

        return services;
    }
}