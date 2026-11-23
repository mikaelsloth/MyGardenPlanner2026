namespace MyGardenPlanner2026.Tests.Unit.Services.Account;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.Interceptors;
using MyGardenPlanner2026.Infrastructure.Services.Account;
using Xunit;

public class AdminSoftDeleteActorScrubberTests : TestDbContext
{
    private sealed class FakeAdminDbContextFactory(Func<PlannerDbContext> create) : IAdminDbContextFactory
    {
        public PlannerDbContext CreateDbContext() => create();

        public Task<PlannerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(create());
    }

    private static AdminSoftDeleteActorScrubber CreateSut(Func<PlannerDbContext> create) =>
        new(new FakeAdminDbContextFactory(create), NullLogger<AdminSoftDeleteActorScrubber>.Instance);

    [Fact]
    public async Task ClearActorEmailAsync_ClearsAdminRowsForUser()
    {
        Guid id;
        using (var context = CreateDbContextWithInterceptors(
            new SoftDeleteInterceptor(FakeCurrentUser.Create("user-1", "user-1@test.dk", "127.0.0.1"))))
        {
            var addOn = TestEntities.AddOn();
            await context.SubscriptionAddOns.AddAsync(addOn, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            context.SubscriptionAddOns.Remove(addOn);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            id = addOn.Id;
        }

        var cleared = await CreateSut(CreateDbContext)
            .ClearActorEmailAsync("user-1", TestContext.Current.CancellationToken);

        cleared.Should().Be(1);
        using var verify = CreateDbContext();
        var row = await verify.SubscriptionAddOns.IgnoreQueryFilters()
            .SingleAsync(a => a.Id == id, TestContext.Current.CancellationToken);
        row.DeletedByUserId.Should().Be("user-1");
        row.DeletedByEmail.Should().BeNull();
    }

    [Fact]
    public async Task ClearActorEmailAsync_WhenAdminContextFails_ReturnsZeroWithoutThrowing()
    {
        var sut = CreateSut(() => throw new InvalidOperationException("boom"));

        var cleared = await sut.ClearActorEmailAsync("user-1", TestContext.Current.CancellationToken);

        cleared.Should().Be(0);
    }

    [Fact]
    public async Task ClearActorEmailAsync_WithBlankUserId_Throws()
    {
        var act = () => CreateSut(CreateDbContext).ClearActorEmailAsync(" ", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}