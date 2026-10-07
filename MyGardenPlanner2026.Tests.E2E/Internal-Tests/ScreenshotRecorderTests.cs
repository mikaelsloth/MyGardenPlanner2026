namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

public sealed class ScreenshotRecorderTests
{
    private static readonly string Root = Path.Combine(Path.GetTempPath(), "root");

    [Fact]
    public void BuildPath_OpbyggerKapitelTjeklisteOgFilnavn()
    {
        var path = ScreenshotRecorder.BuildPath(Root, "2", "Tjekliste A", "Loadside", "LandingPage", 375);

        path.Should().Be(Path.Combine(Root, "2", "Tjekliste A", "Loadside_LandingPage_375.png"));
    }

    [Fact]
    public void BuildPath_ErstatterUgyldigeTegnMedUnderscore()
    {
        var path = ScreenshotRecorder.BuildPath(Root, "2", "Tjekliste A", "A/B:C", "Komp", 768);

        Path.GetFileName(path).Should().Be("A_B_C_Komp_768.png");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("..")]
    public void BuildPath_TomtEllerKunPunktummer_Kaster(string component)
    {
        var act = () => ScreenshotRecorder.BuildPath(Root, "2", "Tjekliste A", "Test", component, 375);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DefaultOutputRoot_LiggerIRepoRodenUnderTestResults()
    {
        var root = ScreenshotRecorder.DefaultOutputRoot();

        root.Should().Be(Path.Combine(RepoPaths.FindRepoRoot(), "TestResults", "Screenshots"));
    }
}