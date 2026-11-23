namespace MyGardenPlanner2026.Tests.Unit.KeyRing;

using FluentAssertions;
using MyGardenPlanner2026.Infrastructure.KeyRing;
using System.Security.Cryptography;
using Xunit;

public class KeyRingCertificateLoaderTests
{
    [Fact]
    public void Load_WithPfxPath_ReturnsCertificateWithPrivateKey()
    {
        var path = TestCertificates.CreatePfxFile("pw");
        try
        {
            using var certificate = KeyRingCertificateLoader.Load(
                new KeyProtectionOptions { CertificatePath = path, CertificatePassword = "pw" }, required: true);

            certificate.Should().NotBeNull();
            certificate!.HasPrivateKey.Should().BeTrue();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_WithWrongPassword_Throws()
    {
        var path = TestCertificates.CreatePfxFile("pw");
        try
        {
            var act = () => KeyRingCertificateLoader.Load(
                new KeyProtectionOptions { CertificatePath = path, CertificatePassword = "forkert" }, required: true);

            act.Should().Throw<CryptographicException>();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_WithoutConfiguration_WhenRequired_Throws()
    {
        var act = () => KeyRingCertificateLoader.Load(new KeyProtectionOptions(), required: true);

        act.Should().Throw<InvalidOperationException>().WithMessage("*DataProtection:CertificatePath*");
    }

    [Fact]
    public void Load_WithoutConfiguration_WhenNotRequired_ReturnsNull() =>
        KeyRingCertificateLoader.Load(new KeyProtectionOptions(), required: false).Should().BeNull();

    [Fact]
    public void Load_NullOptions_Throws()
    {
        var act = () => KeyRingCertificateLoader.Load(null!, required: false);

        act.Should().Throw<ArgumentNullException>();
    }
}