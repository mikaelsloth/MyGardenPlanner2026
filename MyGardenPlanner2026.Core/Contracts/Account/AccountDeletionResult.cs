namespace MyGardenPlanner2026.Core.Contracts.Account;

public sealed record AccountDeletionResult
{
    /// <summary>Navne på haver, hvor brugeren er eneste ejer, og der er andre medlemmer.</summary>
    public IReadOnlyList<string> BlockingGardenNames { get; init; } = [];

    public bool IsBlocked => BlockingGardenNames.Count > 0;

    public static AccountDeletionResult Completed() => new();

    public static AccountDeletionResult Blocked(IReadOnlyList<string> gardenNames) =>
        new() { BlockingGardenNames = gardenNames };
}