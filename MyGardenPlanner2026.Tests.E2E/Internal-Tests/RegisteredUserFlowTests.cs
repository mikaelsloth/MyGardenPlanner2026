namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;
using System.ComponentModel.DataAnnotations;

public sealed class RegisteredUserFlowTests
{
    [Fact]
    public void NewEmail_ErGyldigEmailIDomaenetSomResetScriptetRydder()
    {
        var email = RegisteredUserFlow.NewEmail();

        new EmailAddressAttribute().IsValid(email).Should().BeTrue();
        email.Should().EndWith("@test.dk");
    }

    [Fact]
    public void NewEmail_GiverNyAdresseHverGang()
    {
        RegisteredUserFlow.NewEmail().Should().NotBe(RegisteredUserFlow.NewEmail());
    }
}