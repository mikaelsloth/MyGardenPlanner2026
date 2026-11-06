namespace MyGardenPlanner2026.Components.Foundation;

/// <summary>
/// Hjælper til ARIA-attributter med boolsk værdi (aria-expanded, aria-selected, aria-pressed).
/// Blazor renderer bool.ToString() som "True"/"False" og en bool-binding som tom/udeladt attribut.
/// ARIA kræver strengene "true"/"false".
/// </summary>
public static class AriaExtensions
{
    public static string ToAriaValue(this bool value) => value ? "true" : "false";
}