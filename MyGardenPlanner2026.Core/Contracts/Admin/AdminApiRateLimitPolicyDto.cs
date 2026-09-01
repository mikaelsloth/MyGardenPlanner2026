// AdminApiRateLimitPolicyDto.cs
namespace MyGardenPlanner2026.Core.Contracts.Admin;

public sealed record AdminApiRateLimitPolicyDto(int PermitLimit, int WindowSeconds, int SegmentsPerWindow);