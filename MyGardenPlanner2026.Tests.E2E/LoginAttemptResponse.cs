namespace MyGardenPlanner2026.Tests.E2E;

/// <summary>Svaret på POST'en i ét loginforsøg. Body læses kun ved status 400 eller derover.</summary>
public sealed record LoginAttemptResponse(int Status, string Body, string? RetryAfter);