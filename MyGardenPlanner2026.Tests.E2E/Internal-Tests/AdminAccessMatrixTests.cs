namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;

public sealed class AdminAccessMatrixTests
{
    [Fact]
    public void Personas_ErDeFireAdminPersonaer() => AdminAccessMatrix.Personas.Should().Equal(
            SmokeTestPersonas.Admin, SmokeTestPersonas.DataAdmin,
            SmokeTestPersonas.PolicyAdmin, SmokeTestPersonas.Auditor);

    [Fact]
    public void For_AllePersonaer_HarStartsideBlandtTilladteOgAltidJitAdgang()
    {
        foreach (var persona in AdminAccessMatrix.Personas)
        {
            var access = AdminAccessMatrix.For(persona);

            access.Allowed.Should().Contain(access.Start);
            access.Allowed.Should().Contain(AdminAccessMatrix.JitRequests);
        }
    }

    [Fact]
    public void DeniedPages_ErKomplementetTilAllowed_OgDeSkaerIkke()
    {
        foreach (var persona in AdminAccessMatrix.Personas)
        {
            var allowed = AdminAccessMatrix.For(persona).Allowed;
            var denied = AdminAccessMatrix.DeniedPages(persona);

            allowed.Intersect(denied).Should().BeEmpty();
            allowed.Concat(denied).Should().BeEquivalentTo(AdminAccessMatrix.AllPages);
        }
    }

    [Fact]
    public void Matrix_KunAdminHarAbonnementer_OgDataAdminHarKunJit()
    {
        AdminAccessMatrix.For(SmokeTestPersonas.Admin).Allowed.Should().Contain(AdminAccessMatrix.Subscriptions);
        AdminAccessMatrix.For(SmokeTestPersonas.DataAdmin).Allowed.Should().Equal(AdminAccessMatrix.JitRequests);

        foreach (var persona in new[] { SmokeTestPersonas.DataAdmin, SmokeTestPersonas.PolicyAdmin, SmokeTestPersonas.Auditor })
        {
            AdminAccessMatrix.For(persona).Allowed.Should().NotContain(AdminAccessMatrix.Subscriptions);
        }
    }

    [Fact]
    public void For_UkendtPersona_Kaster()
    {
        var act = () => AdminAccessMatrix.For(SmokeTestPersonas.Plain);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}