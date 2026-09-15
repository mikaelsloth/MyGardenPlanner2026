namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Layout;
using Xunit;

public sealed class FormSectionTests : BunitContext
{
    [Fact]
    public void FormSection_WithTitle_RendersHeader()
    {
        var cut = Render<FormSection>(p => p
            .Add(f => f.Title, "Kontaktoplysninger")
            .AddChildContent("<p>Indhold</p>"));

        cut.Find(".form-section-header h3").TextContent.Should().Be("Kontaktoplysninger");
    }

    [Fact]
    public void FormSection_TwoColumns_RendersFormGrid2Class()
    {
        var cut = Render<FormSection>(p => p.Add(f => f.Columns, 2));

        cut.Find(".form-grid").ClassList.Should().Contain("form-grid-2");
    }
}