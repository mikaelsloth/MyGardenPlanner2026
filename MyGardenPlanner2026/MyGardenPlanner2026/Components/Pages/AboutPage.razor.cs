namespace MyGardenPlanner2026.Components.Pages;

public partial class AboutPage
{
    private static readonly IReadOnlyList<(string Title, string Description)> Features =
    [
        ("Projektstyring", "Hold styr på status og deadlines for alle dine haveprojekter."),
        ("Smart Budget", "Få et \"worst-case\" prisoverslag på tværs af plantealternativer, så du aldrig bliver overrasket."),
        ("Botanisk Opslag", "Slå planter op via integration med Perenual API og få korrekte, opdaterede botaniske data."),
        ("Billedarkiv", "Gem dine egne billeder og følg havens udvikling år for år.")
    ];
}