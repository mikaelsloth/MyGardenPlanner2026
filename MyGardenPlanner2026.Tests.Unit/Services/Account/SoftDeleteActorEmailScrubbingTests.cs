namespace MyGardenPlanner2026.Tests.Unit.Services.Account;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Interceptors;
using MyGardenPlanner2026.Infrastructure.Services.Account;
using Xunit;

public class SoftDeleteActorEmailScrubbingTests : TestDbContext
{
    private static ICurrentUserAccessor User(string id) =>
        FakeCurrentUser.Create(id, $"{id}@test.dk", "127.0.0.1");

    private async Task<Guid> SoftDeleteAddOnAsync(ICurrentUserAccessor user)
    {
        using var context = CreateDbContextWithInterceptors(new SoftDeleteInterceptor(user));
        var addOn = TestEntities.AddOn();
        await context.SubscriptionAddOns.AddAsync(addOn, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.SubscriptionAddOns.Remove(addOn);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        return addOn.Id;
    }

    private async Task<Guid> SoftDeleteGardenAsync(ICurrentUserAccessor user)
    {
        using var context = CreateDbContextWithInterceptors(new SoftDeleteInterceptor(user));
        var garden = new Garden { Name = "Testhave" };
        await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.Gardens.Remove(garden);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        return garden.Id;
    }

    private async Task<(string? UserId, string? Email)> ReadAddOnActorAsync(Guid id)
    {
        using var context = CreateDbContext();
        var row = await context.SubscriptionAddOns.IgnoreQueryFilters()
            .SingleAsync(a => a.Id == id, TestContext.Current.CancellationToken);
        return (row.DeletedByUserId, row.DeletedByEmail);
    }

    private async Task<(string? UserId, string? Email)> ReadGardenActorAsync(Guid id)
    {
        using var context = CreateDbContext();
        var row = await context.Gardens.IgnoreQueryFilters()
            .SingleAsync(g => g.Id == id, TestContext.Current.CancellationToken);
        return (row.DeletedByUserId, row.DeletedByEmail);
    }

    [Fact]
    public async Task ClearAsync_AdminPass_ClearsEmailOnlyForGivenUser()
    {
        var own = await SoftDeleteAddOnAsync(User("user-1"));
        var other = await SoftDeleteAddOnAsync(User("user-2"));

        using var context = CreateDbContext();
        var cleared = await SoftDeleteActorEmailScrubbing.ClearAsync(
            context, "user-1", adminSchema: true, TestContext.Current.CancellationToken);

        cleared.Should().Be(1);
        (await ReadAddOnActorAsync(own)).Should().Be(("user-1", null));
        (await ReadAddOnActorAsync(other)).Should().Be(("user-2", "user-2@test.dk"));
    }

    [Fact]
    public async Task ClearAsync_AdminPass_DoesNotTouchDboRows()
    {
        var garden = await SoftDeleteGardenAsync(User("user-1"));

        using var context = CreateDbContext();
        var cleared = await SoftDeleteActorEmailScrubbing.ClearAsync(
            context, "user-1", adminSchema: true, TestContext.Current.CancellationToken);

        cleared.Should().Be(0);
        (await ReadGardenActorAsync(garden)).Should().Be(("user-1", "user-1@test.dk"));
    }

    [Fact]
    public async Task ClearAsync_DboPass_ClearsGardenRows_ButNotAdminRows()
    {
        var garden = await SoftDeleteGardenAsync(User("user-1"));
        var addOn = await SoftDeleteAddOnAsync(User("user-1"));

        using var context = CreateDbContext();
        var cleared = await SoftDeleteActorEmailScrubbing.ClearAsync(
            context, "user-1", adminSchema: false, TestContext.Current.CancellationToken);

        cleared.Should().Be(1);
        (await ReadGardenActorAsync(garden)).Should().Be(("user-1", null));
        (await ReadAddOnActorAsync(addOn)).Should().Be(("user-1", "user-1@test.dk"));
    }

    [Fact]
    public async Task ClearAsync_IsIdempotent()
    {
        await SoftDeleteAddOnAsync(User("user-1"));

        using var context = CreateDbContext();
        await SoftDeleteActorEmailScrubbing.ClearAsync(
            context, "user-1", adminSchema: true, TestContext.Current.CancellationToken);
        var second = await SoftDeleteActorEmailScrubbing.ClearAsync(
            context, "user-1", adminSchema: true, TestContext.Current.CancellationToken);

        second.Should().Be(0);
    }

    [Fact]
    public async Task ClearAsync_WithBlankUserId_Throws()
    {
        using var context = CreateDbContext();

        var act = () => SoftDeleteActorEmailScrubbing.ClearAsync(
            context, " ", adminSchema: true, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void SoftDeleteEntityTypes_PartitionEveryISoftDeleteEntity_BetweenAdminAndDbo()
    {
        using var context = CreateDbContext();
        var all = context.Model.GetEntityTypes()
            .Select(t => t.ClrType)
            .Where(t => typeof(ISoftDelete).IsAssignableFrom(t))
            .ToList();

        var admin = SoftDeleteActorEmailScrubbing.SoftDeleteEntityTypes(context.Model, adminSchema: true);
        var dbo = SoftDeleteActorEmailScrubbing.SoftDeleteEntityTypes(context.Model, adminSchema: false);

        admin.Concat(dbo).Should().OnlyHaveUniqueItems().And.HaveCount(all.Count);
        all.Should().OnlyContain(t => admin.Contains(t) || dbo.Contains(t));
        admin.Should().Contain(typeof(SubscriptionAddOn));
        dbo.Should().Contain(typeof(Garden));
    }

    [Fact]
    public void AuditLog_IsNotSoftDeletable_BecauseItIsAppendOnly() =>
        // DENY UPDATE/DELETE på admin.AuditLogs: en ISoftDelete-AuditLog ville få rensningen til at fejle.
        typeof(ISoftDelete).IsAssignableFrom(typeof(AuditLog)).Should().BeFalse();
}