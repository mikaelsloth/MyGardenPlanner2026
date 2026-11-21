namespace MyGardenPlanner2026.Tests.Unit.Services.Account;

using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.Services.Account;
using System.Text;
using System.Text.Json.Nodes;
using Xunit;

public sealed class AccountBackupServiceTests : IDisposable
{
    private const string UserId = "user-1";
    private const string OtherUserId = "user-2";
    private const string UserEmail = "bruger@example.dk";

    private static readonly DateTimeOffset Now = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

    private static readonly Dictionary<string, string> PersonalData = new()
    {
        ["Email"] = UserEmail,
        ["PhoneNumber"] = "12345678"
    };

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<PlannerDbContext> _options;
    private readonly EphemeralDataProtectionProvider _protection = new();
    private readonly AccountBackupService _sut;

    public AccountBackupServiceTests()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<PlannerDbContext>().UseSqlite(_connection).Options;

        using (var context = new PlannerDbContext(_options))
        {
            context.Database.EnsureCreated();
        }

        _sut = CreateService(_protection);
    }

    public void Dispose() => _connection.Dispose();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private AccountBackupService CreateService(IDataProtectionProvider protection) =>
        new(new TestFactory(_options), protection, new FixedTimeProvider(Now));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class TestFactory(DbContextOptions<PlannerDbContext> options) : IDbContextFactory<PlannerDbContext>
    {
        public PlannerDbContext CreateDbContext() => new(options);
    }

    // ---------- Seed ----------

    private async Task<Garden> SeedGardenAsync(
        string name,
        string? description = null,
        bool archived = false,
        bool deleted = false,
        params (string UserId, bool IsOwner, GardenAccessLevel Layer, AccessCategory Category)[] members)
    {
        await using var context = new PlannerDbContext(_options);
        var garden = new Garden
        {
            Name = name,
            Description = description,
            Archived = archived,
            IsDeleted = deleted,
            CreatedAtUtc = Now.AddDays(-10)
        };
        context.Gardens.Add(garden);

        foreach (var (userId, isOwner, layer, category) in members)
        {
            context.GardenMemberships.Add(new GardenMembership
            {
                GardenId = garden.Id,
                UserId = userId,
                IsOwner = isOwner,
                Layer = layer,
                Category = category,
                JoinedAtUtc = Now.AddDays(-9)
            });
        }

        await context.SaveChangesAsync(Ct);
        return garden;
    }

    private async Task SeedEntitlementAsync(Guid gardenId, string userId)
    {
        await using var context = new PlannerDbContext(_options);
        context.UserEntitlements.Add(new UserEntitlement
        {
            GardenId = gardenId,
            UserId = userId,
            Layer = GardenAccessLevel.Planlaegger,
            Category = AccessCategory.Administrator,
            BillingCycle = BillingCycle.Annual,
            ValidToUtc = Now.AddYears(1),
            IsTrial = false,
            ExtraBedProposalsCount = 2,
            ExtraPlannedBedsCount = 3,
            ExtraCategoryAArtifactsCount = 4,
            ExtraCategoryBArtifactsCount = 5
        });
        await context.SaveChangesAsync(Ct);
    }

    private static (string UserId, bool IsOwner, GardenAccessLevel Layer, AccessCategory Category) Owner(string userId = UserId) =>
        (userId, true, GardenAccessLevel.Planlaegger, AccessCategory.Administrator);

    private static (string UserId, bool IsOwner, GardenAccessLevel Layer, AccessCategory Category) Member(string userId) =>
        (userId, false, GardenAccessLevel.BedDesigner, AccessCategory.Editor);

    private static byte[] Tamper(byte[] file, Action<JsonNode> change)
    {
        var node = JsonNode.Parse(file)!;
        change(node);
        return Encoding.UTF8.GetBytes(node.ToJsonString());
    }

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_WritesReadableEnvelopeWithFormatVersionTimeEmailHashAndSignature()
    {
        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);

        var node = JsonNode.Parse(file)!;
        ((string?)node["format"]).Should().Be("MyGardenPlanner.AccountBackup");
        ((int?)node["version"]).Should().Be(1);
        DateTimeOffset.Parse((string)node["createdUtc"]!).Should().Be(Now);
        ((string?)node["emailHash"]).Should().Be(RestoreEmailHasher.Hash(UserEmail));
        ((string?)node["signature"]).Should().NotBeNullOrWhiteSpace();
        ((string?)node["payload"]!["personalData"]!["Email"]).Should().Be(UserEmail);
    }

    [Fact]
    public async Task CreateAsync_IncludesOwnGardensWithMembershipAndEntitlement()
    {
        var garden = await SeedGardenAsync("Min have", "Beskrivelse", archived: true, members: Owner());
        await SeedEntitlementAsync(garden.Id, UserId);

        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);

        var payload = _sut.Verify(file, UserEmail).Payload!;
        var backedUp = payload.Gardens.Should().ContainSingle().Subject;
        backedUp.GardenId.Should().Be(garden.Id);
        backedUp.Name.Should().Be("Min have");
        backedUp.Description.Should().Be("Beskrivelse");
        backedUp.Archived.Should().BeTrue();
        backedUp.CreatedAtUtc.Should().Be(Now.AddDays(-10));
        backedUp.IsOwner.Should().BeTrue();
        backedUp.Layer.Should().Be(GardenAccessLevel.Planlaegger);
        backedUp.Category.Should().Be(AccessCategory.Administrator);
        backedUp.JoinedAtUtc.Should().Be(Now.AddDays(-9));

        var entitlement = backedUp.Entitlement!;
        entitlement.Layer.Should().Be(GardenAccessLevel.Planlaegger);
        entitlement.Category.Should().Be(AccessCategory.Administrator);
        entitlement.BillingCycle.Should().Be(BillingCycle.Annual);
        entitlement.ValidToUtc.Should().Be(Now.AddYears(1));
        entitlement.IsTrial.Should().BeFalse();
        entitlement.ExtraBedProposalsCount.Should().Be(2);
        entitlement.ExtraPlannedBedsCount.Should().Be(3);
        entitlement.ExtraCategoryAArtifactsCount.Should().Be(4);
        entitlement.ExtraCategoryBArtifactsCount.Should().Be(5);
    }

    [Fact]
    public async Task CreateAsync_GardenWithoutEntitlement_HasNullEntitlement()
    {
        await SeedGardenAsync("Uden entitlement", members: Owner());

        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);

        _sut.Verify(file, UserEmail).Payload!.Gardens.Should().ContainSingle().Which.Entitlement.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ExcludesOtherUsersGardensSoftDeletedGardensAndOtherMembers()
    {
        await SeedGardenAsync("Fælles have", members: [Owner(), Member(OtherUserId)]);
        await SeedGardenAsync("Slettet have", deleted: true, members: Owner());
        await SeedGardenAsync("Andens have", members: Owner(OtherUserId));

        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);

        var payload = _sut.Verify(file, UserEmail).Payload!;
        payload.Gardens.Select(g => g.Name).Should().Equal("Fælles have");
        Encoding.UTF8.GetString(file).Should().NotContain(OtherUserId).And.NotContain("Slettet have").And.NotContain("Andens have");
    }

    [Fact]
    public async Task CreateAsync_MultipleGardens_AreOrderedByGardenId()
    {
        await SeedGardenAsync("A", members: Owner());
        await SeedGardenAsync("B", members: Owner());
        await SeedGardenAsync("C", members: Owner());

        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);

        _sut.Verify(file, UserEmail).Payload!.Gardens.Select(g => g.GardenId).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task CreateAsync_NoGardens_ProducesValidFileWithEmptyGardens()
    {
        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);

        var result = _sut.Verify(file, UserEmail);

        result.IsValid.Should().BeTrue();
        result.Payload!.Gardens.Should().BeEmpty();
        result.Payload.PersonalData.Should().BeEquivalentTo(PersonalData);
    }

    [Theory]
    [InlineData("", UserEmail)]
    [InlineData("  ", UserEmail)]
    [InlineData(UserId, "")]
    [InlineData(UserId, "  ")]
    public async Task CreateAsync_BlankUserOrEmail_Throws(string userId, string email)
    {
        var act = () => _sut.CreateAsync(userId, email, PersonalData, Ct);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_NullPersonalData_Throws()
    {
        var act = () => _sut.CreateAsync(UserId, UserEmail, null!, Ct);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ---------- Verify ----------

    [Fact]
    public async Task Verify_UntouchedFile_IsValidAndReturnsPayload()
    {
        var garden = await SeedGardenAsync("Min have", members: Owner());
        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);

        var result = _sut.Verify(file, UserEmail);

        result.Status.Should().Be(AccountBackupStatus.Valid);
        result.IsValid.Should().BeTrue();
        result.Payload!.PersonalData.Should().BeEquivalentTo(PersonalData);
        result.Payload.Gardens.Should().ContainSingle().Which.GardenId.Should().Be(garden.Id);
    }

    [Theory]
    [InlineData(" Bruger@Example.DK ")]
    [InlineData("BRUGER@EXAMPLE.DK")]
    public async Task Verify_EmailIsCaseAndWhitespaceInsensitive(string email)
    {
        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);

        _sut.Verify(file, email).Status.Should().Be(AccountBackupStatus.Valid);
    }

    [Fact]
    public async Task Verify_OtherEmail_ReturnsEmailMismatchWithoutPayload()
    {
        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);

        var result = _sut.Verify(file, "anden@example.dk");

        result.Status.Should().Be(AccountBackupStatus.EmailMismatch);
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task Verify_ReformattedFile_IsStillValid()
    {
        await SeedGardenAsync("Min have", "Beskrivelse", members: Owner());
        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);
        var compact = Tamper(file, _ => { });

        compact.Length.Should().BeLessThan(file.Length);
        _sut.Verify(compact, UserEmail).Status.Should().Be(AccountBackupStatus.Valid);
    }

    [Fact]
    public async Task Verify_TamperedGardenName_ReturnsInvalidSignature()
    {
        await SeedGardenAsync("Min have", members: Owner());
        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);
        var tampered = Tamper(file, n => n["payload"]!["gardens"]![0]!["name"] = "Andet navn");

        _sut.Verify(tampered, UserEmail).Status.Should().Be(AccountBackupStatus.InvalidSignature);
    }

    [Fact]
    public async Task Verify_TamperedEntitlement_ReturnsInvalidSignature()
    {
        var garden = await SeedGardenAsync("Min have", members: Owner());
        await SeedEntitlementAsync(garden.Id, UserId);
        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);
        var tampered = Tamper(file, n => n["payload"]!["gardens"]![0]!["entitlement"]!["extraBedProposalsCount"] = 999);

        _sut.Verify(tampered, UserEmail).Status.Should().Be(AccountBackupStatus.InvalidSignature);
    }

    [Fact]
    public async Task Verify_TamperedPersonalData_ReturnsInvalidSignature()
    {
        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);
        var tampered = Tamper(file, n => n["payload"]!["personalData"]!["PhoneNumber"] = "00000000");

        _sut.Verify(tampered, UserEmail).Status.Should().Be(AccountBackupStatus.InvalidSignature);
    }

    [Fact]
    public async Task Verify_TamperedCreatedUtc_ReturnsInvalidSignature()
    {
        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);
        var tampered = Tamper(file, n => n["createdUtc"] = Now.AddDays(1).ToString("O"));

        _sut.Verify(tampered, UserEmail).Status.Should().Be(AccountBackupStatus.InvalidSignature);
    }

    [Fact]
    public async Task Verify_EmailHashReplacedWithOtherEmailsHash_ReturnsInvalidSignatureEvenForThatEmail()
    {
        // Sikkerhedsegenskab: filen kan ikke "omadresseres" til en anden e-mail.
        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);
        var retargeted = Tamper(file, n => n["emailHash"] = RestoreEmailHasher.Hash("anden@example.dk"));

        _sut.Verify(retargeted, "anden@example.dk").Status.Should().Be(AccountBackupStatus.InvalidSignature);
    }

    [Fact]
    public async Task Verify_FileSignedWithOtherKeyRing_ReturnsInvalidSignature()
    {
        var otherInstallation = CreateService(new EphemeralDataProtectionProvider());
        var file = await otherInstallation.CreateAsync(UserId, UserEmail, PersonalData, Ct);

        _sut.Verify(file, UserEmail).Status.Should().Be(AccountBackupStatus.InvalidSignature);
    }

    [Fact]
    public async Task Verify_SignatureGarbage_ReturnsInvalidSignature()
    {
        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);
        var tampered = Tamper(file, n => n["signature"] = "ikke-en-gyldig-signatur");

        _sut.Verify(tampered, UserEmail).Status.Should().Be(AccountBackupStatus.InvalidSignature);
    }

    [Fact]
    public async Task Verify_WrongFormat_ReturnsInvalidFormat()
    {
        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);
        var tampered = Tamper(file, n => n["format"] = "NoetAndet");

        _sut.Verify(tampered, UserEmail).Status.Should().Be(AccountBackupStatus.InvalidFormat);
    }

    [Fact]
    public async Task Verify_UnsupportedVersion_ReturnsUnsupportedVersion()
    {
        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);
        var tampered = Tamper(file, n => n["version"] = 2);

        _sut.Verify(tampered, UserEmail).Status.Should().Be(AccountBackupStatus.UnsupportedVersion);
    }

    [Fact]
    public async Task Verify_MissingSignature_ReturnsInvalidFormat()
    {
        var file = await _sut.CreateAsync(UserId, UserEmail, PersonalData, Ct);
        var tampered = Tamper(file, n => ((JsonObject)n).Remove("signature"));

        _sut.Verify(tampered, UserEmail).Status.Should().Be(AccountBackupStatus.InvalidFormat);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ikke json")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("123")]
    public void Verify_NotABackupFile_ReturnsInvalidFormat(string content) => _sut.Verify(Encoding.UTF8.GetBytes(content), UserEmail).Status.Should().Be(AccountBackupStatus.InvalidFormat);

    [Fact]
    public void Verify_NullFile_Throws()
    {
        var act = () => _sut.Verify(null!, UserEmail);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Verify_BlankEmail_Throws(string email)
    {
        var act = () => _sut.Verify([1, 2, 3], email);

        act.Should().Throw<ArgumentException>();
    }
}