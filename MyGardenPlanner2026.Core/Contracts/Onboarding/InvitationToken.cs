namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

/// <summary>
/// RawToken må KUN eksponeres til brugeren i invitationslinket på oprettelsestidspunktet
/// og må aldrig logges eller persisteres. TokenHash er den eneste værdi der gemmes i
/// databasen (se GardenInvitation.TokenHash).
/// </summary>
public sealed record InvitationToken(string RawToken, string TokenHash);