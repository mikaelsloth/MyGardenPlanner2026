namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public sealed class AuditLogViewerPreferenceServiceTests : TestDbContext
{
    private readonly AuditLogViewerPreferenceService sut;

    public AuditLogViewerPreferenceServiceTests()
    {
        sut = new AuditLogViewerPreferenceService(CreateDbContextFactory());
    }

    [Fact]
    public async Task GetAsync_NoExistingPreference_ReturnsDefaultPageSizeAndNullFilter()
    {
        var result = await sut.GetAsync("user-1", TestContext.Current.CancellationToken);

        result.PageSize.Should().Be(25);
        result.LastFilter.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_NewUser_CreatesPreference()
    {
        await sut.SaveAsync("user-1", new AuditLogViewerPreferenceDto(50, null), TestContext.Current.CancellationToken);

        var result = await sut.GetAsync("user-1", TestContext.Current.CancellationToken);

        result.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task SaveAsync_ExistingUser_OverwritesPreviousValue()
    {
        await sut.SaveAsync("user-1", new AuditLogViewerPreferenceDto(25, null), TestContext.Current.CancellationToken);
        await sut.SaveAsync("user-1", new AuditLogViewerPreferenceDto(100, null), TestContext.Current.CancellationToken);

        var result = await sut.GetAsync("user-1", TestContext.Current.CancellationToken);

        result.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task SaveAsync_WithFilter_RoundTripsFilterFieldsCorrectly()
    {
        var filter = new AuditLogFilterDto(
            "SubscriptionTier", "abc-123", "user-9", "user9@example.com", AuditAction.Update,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero));

        await sut.SaveAsync("user-1", new AuditLogViewerPreferenceDto(50, filter), TestContext.Current.CancellationToken);

        var result = await sut.GetAsync("user-1", TestContext.Current.CancellationToken);

        result.LastFilter.Should().NotBeNull();
        result.LastFilter!.EntityName.Should().Be("SubscriptionTier");
        result.LastFilter.EntityId.Should().Be("abc-123");
        result.LastFilter.UserId.Should().Be("user-9");
        result.LastFilter.UserEmail.Should().Be("user9@example.com");
        result.LastFilter.Action.Should().Be(AuditAction.Update);
        result.LastFilter.FromUtc.Should().Be(filter.FromUtc);
        result.LastFilter.ToUtc.Should().Be(filter.ToUtc);
    }

    [Fact]
    public async Task SaveAsync_WithNullFilter_ClearsPreviouslySavedFilter()
    {
        var filter = new AuditLogFilterDto("SubscriptionTier", null, null, null, null, null, null);
        await sut.SaveAsync("user-1", new AuditLogViewerPreferenceDto(25, filter), TestContext.Current.CancellationToken);

        await sut.SaveAsync("user-1", new AuditLogViewerPreferenceDto(25, null), TestContext.Current.CancellationToken);

        var result = await sut.GetAsync("user-1", TestContext.Current.CancellationToken);

        result.LastFilter.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_DifferentUsers_AreStoredIndependently()
    {
        await sut.SaveAsync("user-1", new AuditLogViewerPreferenceDto(10, null), TestContext.Current.CancellationToken);
        await sut.SaveAsync("user-2", new AuditLogViewerPreferenceDto(100, null), TestContext.Current.CancellationToken);

        var result1 = await sut.GetAsync("user-1", TestContext.Current.CancellationToken);
        var result2 = await sut.GetAsync("user-2", TestContext.Current.CancellationToken);

        result1.PageSize.Should().Be(10);
        result2.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task SaveAsync_PageSizeLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        var act = async () => await sut.SaveAsync("user-1", new AuditLogViewerPreferenceDto(0, null));

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetAsync_MissingUserId_ThrowsArgumentException(string? userId)
    {
        var act = async () => await sut.GetAsync(userId!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SaveAsync_MissingUserId_ThrowsArgumentException(string? userId)
    {
        var act = async () => await sut.SaveAsync(userId!, new AuditLogViewerPreferenceDto(25, null));

        await act.Should().ThrowAsync<ArgumentException>();
    }
}