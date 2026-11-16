namespace MyGardenPlanner2026.Core.Contracts.Account;

public sealed record AccountDeletionResult
{
    /// <summary>Haver, hvor brugeren er eneste ejer, og der er andre medlemmer, og som ikke er løst.</summary>
    public IReadOnlyList<BlockingGardenDto> BlockingGardens { get; init; } = [];

    /// <summary>Navne på de blokerende haver.</summary>
    public IReadOnlyList<string> BlockingGardenNames => [.. BlockingGardens.Select(g => g.Name)];

    public bool IsBlocked => BlockingGardens.Count > 0;

    public static AccountDeletionResult Completed() => new();

    public static AccountDeletionResult Blocked(IReadOnlyList<BlockingGardenDto> gardens) =>
        new() { BlockingGardens = gardens };
}