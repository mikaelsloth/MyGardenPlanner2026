namespace MyGardenPlanner2026.Tests.UI.Components.Account;

using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Account;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Entities;

public sealed class EmailProviderRegistrationTests
{
    [Theory]
    [InlineData(null, typeof(IdentityNoOpEmailSender))]
    [InlineData("NoOp", typeof(IdentityNoOpEmailSender))]
    [InlineData("Smtp", typeof(IdentityEmailSender))]
    public void AddIdentityServices_SelectsSenderFromEmailProvider(string? configuredProvider, Type expected)
    {
        var settings = new Dictionary<string, string?>();
        if (configuredProvider is not null)
        {
            settings["Email:Provider"] = configuredProvider;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddIdentityServices();

        using var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetRequiredService<IEmailSender<ApplicationUser>>().Should().BeOfType(expected);
    }
}