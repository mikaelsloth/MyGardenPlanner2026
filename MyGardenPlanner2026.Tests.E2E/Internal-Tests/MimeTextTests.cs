namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;
using MyGardenPlanner2026.Tests.E2E.Smtp;
using System.Text;

public sealed class MimeTextTests
{
    private static string Base64(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

    [Fact]
    public void DecodeHeader_UdenEncodedWord_ErUaendret()
    {
        MimeText.DecodeHeader("Sikkerhedsalarm").Should().Be("Sikkerhedsalarm");
    }

    [Fact]
    public void DecodeHeader_BEncodedWord_Dekodes()
    {
        MimeText.DecodeHeader($"=?utf-8?B?{Base64("forsøg")}?=").Should().Be("forsøg");
    }

    [Fact]
    public void DecodeHeader_QEncodedWord_DekodesMedUnderscoreSomMellemrum()
    {
        MimeText.DecodeHeader("=?utf-8?Q?fors=C3=B8g_nu?=").Should().Be("forsøg nu");
    }

    [Fact]
    public void DecodeHeader_FlereOrdSkiltAfMellemrum_SamlesUdenMellemrum()
    {
        var header = $"=?utf-8?B?{Base64("for")}?= =?utf-8?B?{Base64("søg")}?=";

        MimeText.DecodeHeader(header).Should().Be("forsøg");
    }

    [Fact]
    public void DecodeBody_Base64_Dekodes()
    {
        MimeText.DecodeBody([Base64("forsøg")], "base64", Encoding.UTF8).Should().Be("forsøg");
    }

    [Fact]
    public void DecodeBody_QuotedPrintableMedSoftBreakOgHex_Dekodes()
    {
        MimeText.DecodeBody(["fors=C3=B8g to=", "gether"], "quoted-printable", Encoding.UTF8)
            .Should().Be("forsøg together");
    }

    [Fact]
    public void DecodeBody_SyvBit_SamlesMedLinjeskift()
    {
        MimeText.DecodeBody(["a", "b"], null, Encoding.UTF8).Should().Be("a\r\nb");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("text/plain")]
    [InlineData("text/plain; charset=ukendt-charset")]
    public void CharsetFromContentType_ManglerEllerUkendt_BrugerUtf8(string? contentType)
    {
        MimeText.CharsetFromContentType(contentType).Should().Be(Encoding.UTF8);
    }

    [Fact]
    public void CharsetFromContentType_AngivetCharset_Bruges()
    {
        MimeText.CharsetFromContentType("text/plain; charset=us-ascii").Should().Be(Encoding.ASCII);
    }
}