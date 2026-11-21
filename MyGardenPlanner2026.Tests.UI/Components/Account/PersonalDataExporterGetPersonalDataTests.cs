namespace MyGardenPlanner2026.Tests.UI.Components.Account;

using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MyGardenPlanner2026.Components.Account;
using MyGardenPlanner2026.Core.Entities;
using NSubstitute;
using Xunit;

public sealed class PersonalDataExporterGetPersonalDataTests
{
    [Fact]
    public async Task GetPersonalDataAsync_ReturnsPersonalDataFieldsAndExternalLoginKeys()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        var user = new ApplicationUser { Id = "user-1", Email = "bruger@example.dk" };
        userManager.GetLoginsAsync(user)
            .Returns(Task.FromResult<IList<UserLoginInfo>>([new UserLoginInfo("Google", "abc-123", "Google")]));
        var sut = new PersonalDataExporter(userManager);

        var result = await sut.GetPersonalDataAsync(user);

        result.Should().ContainKey("Email").WhoseValue.Should().Be("bruger@example.dk");
        result.Should().ContainKey("Google external login provider key").WhoseValue.Should().Be("abc-123");
    }

    [Fact]
    public async Task GetPersonalDataAsync_NullUser_Throws()
    {
        var sut = new PersonalDataExporter(IdentityTestDoubles.CreateUserManager());

        var act = () => sut.GetPersonalDataAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}