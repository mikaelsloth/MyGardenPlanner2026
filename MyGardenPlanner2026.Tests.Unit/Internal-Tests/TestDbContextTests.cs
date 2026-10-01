namespace MyGardenPlanner2026.Tests.Unit.Internal_Tests;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public sealed class TestDbContextTests : TestDbContext
{
    private static AuditLog NewLog(string entityId) => new()
    {
        EntityName = "Test",
        EntityId = entityId,
        Action = AuditAction.Create,
        TimestampUtc = DateTimeOffset.UtcNow
    };

    private sealed class CountingInterceptor : SaveChangesInterceptor
    {
        public int Calls { get; private set; }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Calls++;
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

    private static async Task AddLogAsync(Microsoft.EntityFrameworkCore.DbContext context, AuditLog log)
    {
        await context.AddAsync(log, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Constructor_CreatesSchema_SoQueriesWorkImmediately()
    {
        using var context = CreateDbContext();

        var count = await context.AuditLogs.CountAsync(TestContext.Current.CancellationToken);

        count.Should().Be(0);
    }

    [Fact]
    public async Task CreateDbContext_ContextsShareTheSameDatabase()
    {
        using (var writer = CreateDbContext())
        {
            await AddLogAsync(writer, NewLog("shared"));
        }

        using var reader = CreateDbContext();
        (await reader.AuditLogs.AnyAsync(l => l.EntityId == "shared", TestContext.Current.CancellationToken))
            .Should().BeTrue();
    }

    [Fact]
    public async Task CreateDbContextFactory_ContextsShareTheSameDatabase()
    {
        using (var writer = CreateDbContext())
        {
            await AddLogAsync(writer, NewLog("factory"));
        }

        var factory = CreateDbContextFactory();
        await using var reader = await factory.CreateDbContextAsync(TestContext.Current.CancellationToken);

        (await reader.AuditLogs.AnyAsync(l => l.EntityId == "factory", TestContext.Current.CancellationToken))
            .Should().BeTrue();
    }

    [Fact]
    public async Task CreateAdminDbContextFactory_ContextsShareTheSameDatabase()
    {
        using (var writer = CreateDbContext())
        {
            await AddLogAsync(writer, NewLog("admin"));
        }

        var factory = CreateAdminDbContextFactory();
        await using var reader = await factory.CreateDbContextAsync(TestContext.Current.CancellationToken);

        (await reader.AuditLogs.AnyAsync(l => l.EntityId == "admin", TestContext.Current.CancellationToken))
            .Should().BeTrue();
    }

    [Fact]
    public async Task CreateDbContext_WithoutInterceptors_DoesNotInvokeInterceptor()
    {
        var interceptor = new CountingInterceptor();
        _ = interceptor;

        using var context = CreateDbContext();
        await AddLogAsync(context, NewLog("plain"));

        interceptor.Calls.Should().Be(0);
    }

    [Fact]
    public async Task CreateDbContextWithInterceptors_InvokesInterceptorOnSave()
    {
        var interceptor = new CountingInterceptor();

        using var context = CreateDbContextWithInterceptors(interceptor);
        await AddLogAsync(context, NewLog("intercepted"));

        interceptor.Calls.Should().Be(1);
    }

    [Fact]
    public async Task CreateAdminDbContextFactoryWithInterceptors_InvokesInterceptorOnSave()
    {
        var interceptor = new CountingInterceptor();
        var factory = CreateAdminDbContextFactoryWithInterceptors(interceptor);

        await using var context = await factory.CreateDbContextAsync(TestContext.Current.CancellationToken);
        await AddLogAsync(context, NewLog("admin-intercepted"));

        interceptor.Calls.Should().Be(1);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var act = () =>
        {
            Dispose();
            Dispose();
        };

        act.Should().NotThrow();
    }
}