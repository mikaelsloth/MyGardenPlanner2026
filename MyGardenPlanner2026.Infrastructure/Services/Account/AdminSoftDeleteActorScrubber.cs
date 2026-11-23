namespace MyGardenPlanner2026.Infrastructure.Services.Account;

using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Infrastructure.Data;
using System.Data.Common;

/// <summary>
/// Standardimplementering af IAdminSoftDeleteActorScrubber. Bruger IAdminDbContextFactory, fordi
/// admin-schema kun kan opdateres af den privilegerede admin-databasebruger (samme mønster som
/// ReAuthFailureTracker). Temporale historiktabeller røres ikke.
/// </summary>
public sealed partial class AdminSoftDeleteActorScrubber(
    IAdminDbContextFactory contextFactory,
    ILogger<AdminSoftDeleteActorScrubber> logger) : IAdminSoftDeleteActorScrubber
{
    [LoggerMessage(EventId = 1170, Level = LogLevel.Warning, Message = "Clearing soft-delete actor e-mails in the admin schema failed for user '{UserId}'; the e-mails remain on the affected rows.")]
    static partial void AdminActorEmailClearFailed(ILogger logger, Exception exception, string UserId);

    public async Task<int> ClearActorEmailAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await SoftDeleteActorEmailScrubbing.ClearAsync(context, userId, adminSchema: true, cancellationToken);
        }
        catch (Exception ex) when (ex is DbException or InvalidOperationException)
        {
            AdminActorEmailClearFailed(logger, ex, userId);
            return 0;
        }
    }
}