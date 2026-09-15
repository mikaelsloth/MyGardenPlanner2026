namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Domain.Gardens;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using Xunit;

public sealed class InvitationLinkCardTests : BunitContext
{
    public InvitationLinkCardTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void RendersInviteUrl_ContainingRawToken()
    {
        var result = new CreateInvitationResultDto(Guid.NewGuid(), "raw-token-abc", DateTimeOffset.UtcNow.AddDays(7), false);

        var cut = Render<InvitationLinkCard>(p => p
            .Add(c => c.Result, result)
            .Add(c => c.GardenName, "Testhave")
            .Add(c => c.InvitedEmail, "invited@example.com"));

        cut.Markup.Should().Contain("raw-token-abc");
    }

    [Fact]
    public void RendersPrintOnlyBlock_WithGardenNameAndInvitedEmail()
    {
        var result = new CreateInvitationResultDto(Guid.NewGuid(), "raw-token-abc", DateTimeOffset.UtcNow.AddDays(7), false);

        var cut = Render<InvitationLinkCard>(p => p
            .Add(c => c.Result, result)
            .Add(c => c.GardenName, "Testhave")
            .Add(c => c.InvitedEmail, "invited@example.com"));

        var printBlock = cut.Find(".print-only");
        printBlock.TextContent.Should().Contain("Testhave");
        printBlock.TextContent.Should().Contain("invited@example.com");
    }

    [Fact]
    public void ClickingCopyLink_ShowsCopiedConfirmation()
    {
        var result = new CreateInvitationResultDto(Guid.NewGuid(), "raw-token-abc", DateTimeOffset.UtcNow.AddDays(7), false);

        var cut = Render<InvitationLinkCard>(p => p
            .Add(c => c.Result, result)
            .Add(c => c.GardenName, "Testhave"));

        cut.FindAll(".card-actions .btn")[0].Click();

        cut.Markup.Should().Contain("Linket er kopieret.");
    }
}