namespace MyGardenPlanner2026.Components.Account;

using Microsoft.AspNetCore.Identity;
using MyGardenPlanner2026.Core.Contracts.Email;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Infrastructure.Services.Email;

/// <summary>
/// Identity-adapter, der sender Identitys mails (bekræftelse, nulstilling) via IUserEmailSender
/// med danske skabeloner. Linkene er allerede HTML-encodede af kalderne. Bruges, når
/// Email:Provider = Smtp; ellers bruges <see cref="IdentityNoOpEmailSender"/>.
/// </summary>
internal sealed class IdentityEmailSender(IUserEmailSender emailSender) : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        emailSender.SendAsync(email, EmailTemplates.ConfirmEmail(confirmationLink));

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        emailSender.SendAsync(email, EmailTemplates.ResetPassword(resetLink));

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        emailSender.SendAsync(email, EmailTemplates.ResetPasswordCode(resetCode));
}