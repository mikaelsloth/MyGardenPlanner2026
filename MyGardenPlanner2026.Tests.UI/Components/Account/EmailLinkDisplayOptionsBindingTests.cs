namespace MyGardenPlanner2026.Tests.UI.Components.Account;

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Components.Account;
using MyGardenPlanner2026.Configuration.Extensions;
using Xunit;

public class EmailLinkDisplayOptionsBindingTests
{
    private static bool ResolveShowEmailLinks(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddIdentityServices();

        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<EmailLinkDisplayOptions>>().Value.ShowEmailLinks;
    }

    [Fact]
    public void ShowEmailLinks_NotConfigured_DefaultsToFalse() =>
        ResolveShowEmailLinks([]).Should().BeFalse();

    [Fact]
    public void ShowEmailLinks_ConfiguredTrue_IsBound() =>
        ResolveShowEmailLinks(new() { ["Identity:ShowEmailLinks"] = "true" }).Should().BeTrue();
}