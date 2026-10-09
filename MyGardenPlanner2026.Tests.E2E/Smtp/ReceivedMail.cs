namespace MyGardenPlanner2026.Tests.E2E.Smtp;

/// <summary>En mail modtaget af <see cref="TestSmtpServer"/>, med emne og krop dekodet.</summary>
public sealed record ReceivedMail(
    string From,
    IReadOnlyList<string> Recipients,
    string Subject,
    string Body);