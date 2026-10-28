namespace MyGardenPlanner2026.Components.Account;

using Microsoft.AspNetCore.Identity;
using MyGardenPlanner2026.Core.Entities;
using System.Text.Json;

/// <summary>
/// Bygger indholdet af PersonalData.json: kun felter markeret med [PersonalData] samt
/// eksterne login-nøgler. Authenticator-nøglen (en hemmelighed) eksporteres bevidst IKKE.
/// </summary>
internal sealed class PersonalDataExporter(UserManager<ApplicationUser> userManager)
{
    public async Task<byte[]> ExportAsync(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var personalData = new Dictionary<string, string>();

        var personalDataProps = typeof(ApplicationUser).GetProperties().Where(
            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
        foreach (var p in personalDataProps)
        {
            personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
        }

        var logins = await userManager.GetLoginsAsync(user);
        foreach (var l in logins)
        {
            personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
        }

        return JsonSerializer.SerializeToUtf8Bytes(personalData);
    }
}