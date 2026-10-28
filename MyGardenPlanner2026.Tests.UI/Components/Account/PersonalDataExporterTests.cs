namespace MyGardenPlanner2026.Tests.UI.Components.Account;

using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MyGardenPlanner2026.Components.Account;
using MyGardenPlanner2026.Core.Entities;
using NSubstitute;
using System.Text.Json;
using Xunit;

public class PersonalDataExporterTests
{
    private const string UserEmail = "bruger@example.dk";

    private static (PersonalDataExporter Exporter, UserManager<ApplicationUser> UserManager, ApplicationUser User) Create(
        IList<UserLoginInfo>? logins = null)
    {
        var user = new ApplicationUser { Id = "user-1", Email = UserEmail, UserName = UserEmail };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetLoginsAsync(user)
            .Returns(Task.FromResult(logins ?? []));

        return (new PersonalDataExporter(userManager), userManager, user);
    }

    private static Dictionary<string, string> Parse(byte[] bytes) =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(bytes)!;

    [Fact]
    public async Task ExportAsync_IncludesPersonalDataProperties()
    {
        var (exporter, _, user) = Create();

        var data = Parse(await exporter.ExportAsync(user));

        data.Should().ContainKey("Email").WhoseValue.Should().Be(UserEmail);
        data.Should().ContainKey("UserName").WhoseValue.Should().Be(UserEmail);
    }

    [Fact]
    public async Task ExportAsync_DoesNotIncludeAuthenticatorKey_AndDoesNotReadIt()
    {
        var (exporter, userManager, user) = Create();

        var data = Parse(await exporter.ExportAsync(user));

        data.Should().NotContainKey("Authenticator Key");
        await userManager.DidNotReceive().GetAuthenticatorKeyAsync(Arg.Any<ApplicationUser>());
    }

    [Fact]
    public async Task ExportAsync_IncludesExternalLoginProviderKeys()
    {
        var logins = new List<UserLoginInfo> { new("Google", "google-key-1", "Google") };
        var (exporter, _, user) = Create(logins);

        var data = Parse(await exporter.ExportAsync(user));

        data.Should().ContainKey("Google external login provider key").WhoseValue.Should().Be("google-key-1");
    }

    [Fact]
    public async Task ExportAsync_NullUser_Throws()
    {
        var (exporter, _, _) = Create();

        var act = () => exporter.ExportAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}