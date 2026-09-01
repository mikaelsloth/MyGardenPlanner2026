// LoginRateLimitPolicyDto.cs
namespace MyGardenPlanner2026.Core.Contracts.Admin;

public sealed record LoginRateLimitPolicyDto(int PermitLimit, int WindowSeconds);