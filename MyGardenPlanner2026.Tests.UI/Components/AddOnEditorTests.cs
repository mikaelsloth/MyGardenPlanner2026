namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Admin;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class AddOnEditorTests : BunitContext
{
    private static readonly Guid AddOn1Id = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static readonly SubscriptionAddOnDto AddOn1 = new(
        AddOn1Id, AddOnType.BedforslagNiveau2, "Bedforslag (Niveau 2)", "Pakke med 2 bedforslag", 180m, 15m, 450m);

    private static Task<AuthenticationState> CreateAuthStateAsync()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")], authenticationType: "Test");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    private ISubscriptionAddOnAdminService RegisterFake(bool reAuthSucceeds = true)
    {
        var service = Substitute.For<ISubscriptionAddOnAdminService>();
        service.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SubscriptionAddOnDto>>([AddOn1]));
        Services.AddSingleton(service);

        var authorizationService = Substitute.For<IAuthorizationService>();
        authorizationService.AuthorizeAsync(
                Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Is(AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy))
            .Returns(Task.FromResult(reAuthSucceeds ? AuthorizationResult.Success() : AuthorizationResult.Failed()));
        Services.AddSingleton(authorizationService);

        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(null));
        Services.AddSingleton(userManager);

        Services.AddSingleton(Substitute.For<IReAuthenticationService>());
        Services.AddSingleton(Substitute.For<IReAuthFailureTracker>());
        Services.AddSingleton(Substitute.For<ICurrentUserAccessor>());

        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        rateLimiter.TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        Services.AddSingleton(rateLimiter);

        return service;
    }

    [Fact]
    public void AddOnEditor_RendersExistingAddOns()
    {
        RegisterFake();

        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.FindAll("tbody tr").Should().HaveCount(1);
        cut.Markup.Should().Contain("Bedforslag (Niveau 2)");
    }

    [Fact]
    public void ReAuthValid_ClickingGem_CallsSaveAsyncWithSameTypeAndEditedName()
    {
        var service = RegisterFake(reAuthSucceeds: true);
        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.Find($"#name-{AddOn1Id}").Change("Bedforslag (Niveau 2) - opdateret");
        cut.Find("button.btn-primary.btn-sm").Click();

        _ = service.Received().SaveAsync(
            Arg.Is<SubscriptionAddOnUpsertDto>(d =>
                d.Id == AddOn1Id && d.Type == AddOnType.BedforslagNiveau2 && d.Name == "Bedforslag (Niveau 2) - opdateret"),
            Arg.Any<CancellationToken>());
        cut.FindAll(".confirm-dialog").Should().BeEmpty();
    }

    [Fact]
    public void ReAuthValid_AddingNewAddOn_CallsSaveAsyncWithNullId()
    {
        var service = RegisterFake(reAuthSucceeds: true);
        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.Find("#new-name").Change("Artefaktpakke C");
        cut.Find("button.btn-primary:not(.btn-sm)").Click();

        _ = service.Received().SaveAsync(
            Arg.Is<SubscriptionAddOnUpsertDto>(d => d.Id == null && d.Name == "Artefaktpakke C"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_ClickingGem_OpensStepUpModal_WithoutSaving()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.Find("button.btn-primary.btn-sm").Click();

        cut.FindAll(".confirm-dialog").Should().HaveCount(1);
        _ = service.DidNotReceive().SaveAsync(Arg.Any<SubscriptionAddOnUpsertDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_AddingNewAddOn_OpensStepUpModal_WithoutSaving()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.Find("#new-name").Change("Artefaktpakke C");
        cut.Find("button.btn-primary:not(.btn-sm)").Click();

        cut.FindAll(".confirm-dialog").Should().HaveCount(1);
        _ = service.DidNotReceive().SaveAsync(Arg.Any<SubscriptionAddOnUpsertDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_ClickingSlet_OpensStepUpModal_WithoutDeleting()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.Find("button.btn-danger.btn-sm").Click();

        cut.FindAll(".confirm-dialog").Should().HaveCount(1);
        _ = service.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_ConfirmingReset_OpensStepUpModal_WithoutResetting()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.Find(".danger-zone button.btn-danger").Click();
        cut.Find(".inline-confirm button.btn-danger").Click();

        cut.FindAll(".confirm-dialog").Should().HaveCount(1);
        _ = service.DidNotReceive().ResetToDefaultAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ReAuthExpired_CancellingStepUpModal_ClosesModal_WithoutSaving()
    {
        var service = RegisterFake(reAuthSucceeds: false);
        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(CreateAuthStateAsync()));

        cut.Find("button.btn-danger.btn-sm").Click();
        cut.Find(".confirm-dialog button.btn-secondary").Click();

        cut.FindAll(".confirm-dialog").Should().BeEmpty();
        _ = service.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void RateLimited_ClickingGem_DoesNotCallUpdateTierAsync_AndShowsErrorMessage()
    {
        var service = RegisterFake(reAuthSucceeds: true);
        var rateLimiter = Substitute.For<IAdminActionRateLimiter>();
        rateLimiter.TryAcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        Services.AddSingleton(rateLimiter); // overskriver den permitterende fake fra RegisterFakes

        var cut = Render<AddOnEditor>(p => p.AddCascadingValue(CreateAuthStateAsync()));
        cut.Find("button.btn-primary").Click();

        _ = service.DidNotReceive().SaveAsync(Arg.Any<SubscriptionAddOnUpsertDto>(), Arg.Any<CancellationToken>());
        cut.Markup.Should().Contain("For mange handlinger");
    }
}