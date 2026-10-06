namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;
using Microsoft.AspNetCore.Identity;

public sealed class SmokeTestDataSeederEnsureSucceededTests
{
    [Fact]
    public void EnsureSucceeded_VedSucces_KasterIkke()
    {
        var act = () => SmokeTestDataSeeder.EnsureSucceeded(IdentityResult.Success, "Kontekst");

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureSucceeded_VedFejl_KasterMedKontekstOgAlleFejlbeskrivelser()
    {
        var result = IdentityResult.Failed(
            new IdentityError { Description = "Fejl A" },
            new IdentityError { Description = "Fejl B" });

        var act = () => SmokeTestDataSeeder.EnsureSucceeded(result, "Kunne ikke gøre noget");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Kunne ikke gøre noget: Fejl A, Fejl B");
    }
}