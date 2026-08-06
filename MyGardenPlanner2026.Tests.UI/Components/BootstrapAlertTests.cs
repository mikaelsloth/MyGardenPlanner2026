namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using MyGardenPlanner2026.Client.Components;
using Xunit;

public class BootstrapAlertTests : BunitContext
{
    [Fact]
    public void BootstrapAlert_RendersCorrectBootstrap5ClassesAndAttributes()
    {
        // Arrange & Act: Render BootstrapAlert komponenten med Success-type og dismiss-knap
        var cut = Render<BootstrapAlert>(parameters => parameters
            .Add(p => p.Type, BootstrapAlert.AlertType.Success)
            .Add(p => p.Dismissible, true)
            .Add(p => p.ChildContent, "Velkommen til MyGardenPlanner2026!"));

        // Assert 1: Verificer hoved-div har de rigtige Bootstrap-klasser
        var alertElement = cut.Find(".alert");
        Assert.True(alertElement.ClassList.Contains("alert-success"), "Mangler 'alert-success' klassen");
        Assert.True(alertElement.ClassList.Contains("alert-dismissible"), "Mangler 'alert-dismissible' klassen");
        Assert.Equal("alert", alertElement.GetAttribute("role"));

        // Assert 2: Verificer Bootstrap 5 lukke-knap (btn-close) og data-bs-dismiss attribut
        var closeButton = cut.Find("button");
        Assert.True(closeButton.ClassList.Contains("btn-close"), "Mangler Bootstrap 5 'btn-close' klassen");
        Assert.Equal("alert", closeButton.GetAttribute("data-bs-dismiss"));

        // Assert 3: Verificer den samlede HTML-struktur svarer præcist til Bootstrap 5 standarden
        cut.MarkupMatches(@"
            <div class=""alert alert-success alert-dismissible fade show"" role=""alert"">
                Velkommen til MyGardenPlanner2026!
                <button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>
            </div>");
    }

    [Fact]
    public void BootstrapAlert_WhenDismissibleIsFalse_DoesNotRenderCloseButton()
    {
        // Arrange & Act
        var cut = Render<BootstrapAlert>(parameters => parameters
            .Add(p => p.Type, BootstrapAlert.AlertType.Warning)
            .Add(p => p.Dismissible, false)
            .Add(p => p.ChildContent, "Advarsel uden lukkeknap"));

        // Assert
        var alertElement = cut.Find(".alert");
        Assert.True(alertElement.ClassList.Contains("alert-warning"));
        Assert.Empty(cut.FindAll("button")); // Verificer at ingen Bootstrap lukkeknap renders
    }
}