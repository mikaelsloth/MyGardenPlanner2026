// ReAuthFailureTrackerPolicyDto.cs
namespace MyGardenPlanner2026.Core.Contracts.Admin;

public sealed record ReAuthFailureTrackerPolicyDto(int Threshold, int WindowDays);