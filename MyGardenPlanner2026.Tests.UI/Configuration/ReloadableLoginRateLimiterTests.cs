namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Configuration.RateLimiting;
using MyGardenPlanner2026.Infrastructure.Services;
using NSubstitute;
using System.Net;
using Xunit;

public class ReloadableLoginRateLimiterTests
{
    private static DefaultHttpContext CreateProtectedLoginRequest()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/Account/Login";
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
        return context;
    }

    private static (IOptionsMonitor<LoginRateLimitOptions> Monitor, Action<LoginRateLimitOptions> TriggerChange) CreateMonitor(
        LoginRateLimitOptions initial)
    {
        var current = initial;
        Action<LoginRateLimitOptions, string?>? callback = null;

        var monitor = Substitute.For<IOptionsMonitor<LoginRateLimitOptions>>();
        monitor.CurrentValue.Returns(_ => current);
        monitor.OnChange(Arg.Do<Action<LoginRateLimitOptions, string?>>(cb => callback = cb))
            .Returns(Substitute.For<IDisposable>());

        void Trigger(LoginRateLimitOptions updated)
        {
            current = updated;
            callback?.Invoke(updated, null);
        }

        return (monitor, Trigger);
    }

    [Fact]
    public void AttemptAcquire_UnprotectedPath_AlwaysSucceeds()
    {
        var (monitor, _) = CreateMonitor(new LoginRateLimitOptions { PermitLimit = 1, WindowSeconds = 60 });
        using var limiter = new ReloadableLoginRateLimiter(monitor);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/pricing";

        var first = limiter.AttemptAcquire(context);
        var second = limiter.AttemptAcquire(context);

        first.IsAcquired.Should().BeTrue();
        second.IsAcquired.Should().BeTrue();
    }

    [Fact]
    public void AttemptAcquire_ProtectedPath_ExceedsPermitLimit_RejectsOverflow()
    {
        var (monitor, _) = CreateMonitor(new LoginRateLimitOptions { PermitLimit = 1, WindowSeconds = 60 });
        using var limiter = new ReloadableLoginRateLimiter(monitor);
        var context = CreateProtectedLoginRequest();

        var first = limiter.AttemptAcquire(context);
        var second = limiter.AttemptAcquire(context);

        first.IsAcquired.Should().BeTrue();
        second.IsAcquired.Should().BeFalse();
    }

    [Fact]
    public void AttemptAcquire_AfterOptionsChanged_AppliesNewPermitLimitImmediately()
    {
        var (monitor, triggerChange) = CreateMonitor(new LoginRateLimitOptions { PermitLimit = 1, WindowSeconds = 60 });
        using var limiter = new ReloadableLoginRateLimiter(monitor);
        var context = CreateProtectedLoginRequest();

        limiter.AttemptAcquire(context).IsAcquired.Should().BeTrue();
        limiter.AttemptAcquire(context).IsAcquired.Should().BeFalse(); // kvote opbrugt under gammel grænse

        triggerChange(new LoginRateLimitOptions { PermitLimit = 5, WindowSeconds = 60 });

        limiter.AttemptAcquire(context).IsAcquired.Should().BeTrue(); // ny limiter, frisk kvote
    }
}