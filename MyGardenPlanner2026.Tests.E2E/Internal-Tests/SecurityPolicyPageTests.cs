namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;

public sealed class SecurityPolicyPageTests
{
    [Fact]
    public void AllTabs_HarSeksFaneMedUnikkeNavneOgKomponentnavne()
    {
        SecurityPolicyPage.AllTabs.Should().HaveCount(6);
        SecurityPolicyPage.AllTabs.Select(t => t.Name).Should().OnlyHaveUniqueItems();
        SecurityPolicyPage.AllTabs.Select(t => t.Component).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AllTabs_KomponentnavneErGyldigeFilnavnedeleUdenMellemrum()
    {
        SecurityPolicyPage.AllTabs.Should().OnlyContain(t =>
            !string.IsNullOrWhiteSpace(t.Component) && t.Component.All(char.IsLetterOrDigit));
    }

    [Fact]
    public void AllTabs_ErISidensRaekkefoelge()
    {
        SecurityPolicyPage.AllTabs.Select(t => t.Name).Should().Equal(
            "JIT-eskalering", "Step-up re-auth", "Fejl-tracker",
            "Admin-API rate limit", "Login rate limit", "AuditLog-eksport");
    }

    [Fact]
    public void AllTabs_HarUnikkePolicynavneOgSuccesbeskeder()
    {
        SecurityPolicyPage.AllTabs.Select(t => t.PolicyName).Should().OnlyHaveUniqueItems().And.OnlyContain(n => !string.IsNullOrWhiteSpace(n));
        SecurityPolicyPage.AllTabs.Select(t => t.SavedText).Should().OnlyHaveUniqueItems().And.OnlyContain(n => !string.IsNullOrWhiteSpace(n));
    }
}