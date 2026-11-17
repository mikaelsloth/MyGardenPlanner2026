namespace MyGardenPlanner2026.Components.Account.Pages.Manage;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Core.Entities.Common;
using System.ComponentModel.DataAnnotations;

public partial class DeletePersonalData
{
    private const string TransferPrefix = "transfer:";
    private const string DeleteChoice = "delete";

    private readonly Dictionary<string, string> candidateEmails = [];

    private string? message;

    private ApplicationUser? user;

    private bool requirePassword;

    private IReadOnlyList<BlockingGardenDto> blockingGardens = [];

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = default!;

    private int RestoreDays => RestoreOptions.CurrentValue.RetentionDays;

    [LoggerMessage(EventId = 1019, Level = LogLevel.Information, Message = "User with ID '{UserId}' deleted themselves.")]
    static partial void UserSelfDeleted(ILogger logger, string UserId);

    [LoggerMessage(EventId = 1083, Level = LogLevel.Warning, Message = "Deleting user with ID '{UserId}' failed: {Errors}.")]
    static partial void UserDeleteFailed(ILogger logger, string UserId, string Errors);

    protected override async Task OnInitializedAsync()
    {
        Input ??= new();

        user = await UserManager.GetUserAsync(HttpContext.User);
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        requirePassword = await UserManager.HasPasswordAsync(user);
        await LoadBlockingGardensAsync();
    }

    private async Task OnValidSubmitAsync()
    {
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        if (requirePassword && !await UserManager.CheckPasswordAsync(user, Input.Password))
        {
            message = AccountMessages.IncorrectPasswords;
            return;
        }

        if (!TryBuildResolutions(out var resolutions))
        {
            return;
        }

        var userId = await UserManager.GetUserIdAsync(user);
        var email = await UserManager.GetEmailAsync(user);

        var dataResult = await AccountDeletionService.DeleteUserDataAsync(userId, email, resolutions, CancellationToken.None);
        if (dataResult.IsBlocked)
        {
            message = AccountMessages.AccountDeletionBlocked(dataResult.BlockingGardenNames);
            await LoadBlockingGardensAsync();
            return;
        }

        var result = await UserManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            UserDeleteFailed(Logger, userId, string.Join(", ", result.Errors.Select(e => e.Code)));
            message = AccountMessages.AccountNotDeleted;
            return;
        }

        await SignInManager.SignOutAsync();

        UserSelfDeleted(Logger, userId);

        // D4-2: efter sletning sendes brugeren til forsiden.
        RedirectManager.RedirectTo("");
    }

    /// <summary>
    /// Indlæser blokerende haver og sikrer, at Input.Gardens har præcis ét element pr. have i samme
    /// rækkefølge (elementer matches på GardenId og bevarer brugerens valg efter et fejlet submit).
    /// </summary>
    private async Task LoadBlockingGardensAsync()
    {
        var userId = await UserManager.GetUserIdAsync(user!);
        blockingGardens = await AccountDeletionService.GetBlockingGardensAsync(userId, CancellationToken.None);

        foreach (var candidate in blockingGardens.SelectMany(g => g.Candidates).DistinctBy(c => c.UserId))
        {
            if (candidateEmails.ContainsKey(candidate.UserId))
            {
                continue;
            }

            var candidateUser = await UserManager.FindByIdAsync(candidate.UserId);
            if (candidateUser?.Email is { } candidateEmail)
            {
                candidateEmails[candidate.UserId] = candidateEmail;
            }
        }

        Input.Gardens =
        [
            .. blockingGardens.Select(g =>
                Input.Gardens.FirstOrDefault(i => i.GardenId == g.GardenId)
                ?? new GardenChoiceInput { GardenId = g.GardenId })
        ];
    }

    private string CandidateLabel(GardenTransferCandidateDto candidate)
    {
        var email = candidateEmails.GetValueOrDefault(candidate.UserId, "Ukendt medlem");
        return $"{email} ({candidate.Category.ToDisplayName()}, {candidate.Layer.ToDisplayName()})";
    }

    /// <summary>
    /// Oversætter brugerens valg til resolutions. Sætter <see cref="message"/> og returnerer false,
    /// hvis et valg mangler, eller navnebekræftelsen ved "slet for alle" ikke passer.
    /// </summary>
    private bool TryBuildResolutions(out Dictionary<Guid, GardenOwnershipResolution> resolutions)
    {
        resolutions = [];

        foreach (var garden in blockingGardens)
        {
            var input = Input.Gardens.FirstOrDefault(i => i.GardenId == garden.GardenId);
            var choice = input?.Choice;

            if (choice == DeleteChoice)
            {
                if (!string.Equals(input!.ConfirmName?.Trim(), garden.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    message = AccountMessages.GardenDeleteNameMismatch(garden.Name);
                    return false;
                }

                resolutions[garden.GardenId] = GardenOwnershipResolution.DeleteForAll();
            }
            else if (choice is not null
                && choice.StartsWith(TransferPrefix, StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(choice[TransferPrefix.Length..]))
            {
                resolutions[garden.GardenId] = GardenOwnershipResolution.TransferTo(choice[TransferPrefix.Length..]);
            }
            else
            {
                message = AccountMessages.GardenChoiceMissing(garden.Name);
                return false;
            }
        }

        return true;
    }

    private sealed class InputModel
    {
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public List<GardenChoiceInput> Gardens { get; set; } = [];
    }

    private sealed class GardenChoiceInput
    {
        public Guid GardenId { get; set; }

        public string Choice { get; set; } = "";

        public string ConfirmName { get; set; } = "";
    }
}