namespace MyGardenPlanner2026.Core.Contracts.Email;

/// <summary>Emne og HTML-indhold i en mail til en bruger.</summary>
public sealed record EmailContent(string Subject, string HtmlBody);