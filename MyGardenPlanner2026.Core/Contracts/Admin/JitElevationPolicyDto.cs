// JitElevationPolicyDto.cs
namespace MyGardenPlanner2026.Core.Contracts.Admin;

public sealed record JitElevationPolicyDto(int MinRequestedMinutes, int MaxRequestedMinutes, int SweepIntervalMinutes);