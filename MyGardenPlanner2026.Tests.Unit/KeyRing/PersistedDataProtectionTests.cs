namespace MyGardenPlanner2026.Tests.Unit.KeyRing;

using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.KeyRing;
using MyGardenPlanner2026.Infrastructure.Services.Account;
using Xunit;

public class PersistedDataProtectionTests : TestDbContext
{
    private sealed class ContextFactory(Func<PlannerDbContext> create) : IDbContextFactory<PlannerDbContext>
    {
        public PlannerDbContext CreateDbContext() => create();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ServiceProvider BuildProvider(KeyProtectionOptions? options = null, bool requireCertificate = false)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAdminDbContextFactory>(new FakeAdminDbContextFactory(CreateDbContext));
        services.AddPersistedDataProtection(options ?? new KeyProtectionOptions(), requireCertificate);
        return services.BuildServiceProvider();
    }

    private string StoredKeyXml()
    {
        using var context = CreateDbContext();
        return string.Concat(context.DataProtectionKeys.Select(k => k.Xml).ToList());
    }

    private int StoredKeyCount()
    {
        using var context = CreateDbContext();
        return context.DataProtectionKeys.Count();
    }

    private static string Protect(ServiceProvider provider, string value) =>
        provider.GetRequiredService<IDataProtectionProvider>().CreateProtector("test").Protect(value);

    private static string Unprotect(ServiceProvider provider, string payload) =>
        provider.GetRequiredService<IDataProtectionProvider>().CreateProtector("test").Unprotect(payload);

    [Fact]
    public void ApplicationName_IsFixed() =>
        PersistedDataProtectionExtensions.ApplicationName.Should().Be("MyGardenPlanner");

    [Fact]
    public void Protect_ThenUnprotectWithNewProvider_Succeeds_AndNoNewKeyIsCreated()
    {
        string payload;
        using (var first = BuildProvider())
        {
            payload = Protect(first, "hemmelig");
        }

        var keysAfterFirst = StoredKeyCount();
        keysAfterFirst.Should().BeGreaterThan(0);

        using var second = BuildProvider();

        Unprotect(second, payload).Should().Be("hemmelig");
        StoredKeyCount().Should().Be(keysAfterFirst);
    }

    [Fact]
    public async Task AccountBackup_CreatedBeforeRestart_VerifiesAfterRestart()
    {
        byte[] file;
        using (var first = BuildProvider())
        {
            file = await CreateBackupService(first).CreateAsync(
                "user-1", "bruger@example.dk", new Dictionary<string, string> { ["Email"] = "bruger@example.dk" }, Ct);
        }

        using var second = BuildProvider();

        CreateBackupService(second).Verify(file, "bruger@example.dk").Status.Should().Be(AccountBackupStatus.Valid);
    }

    [Fact]
    public void WithoutCertificate_KeysAreStoredUnencrypted()
    {
        using var provider = BuildProvider();
        Protect(provider, "x");

        StoredKeyXml().Should().NotContain("EncryptedData");
    }

    [Fact]
    public void WithCertificate_KeysAreEncryptedAtRest_AndUsableAfterRestart()
    {
        var path = TestCertificates.CreatePfxFile("pw");
        try
        {
            var options = new KeyProtectionOptions { CertificatePath = path, CertificatePassword = "pw" };

            string payload;
            using (var first = BuildProvider(options))
            {
                payload = Protect(first, "hemmelig");
            }

            StoredKeyXml().Should().Contain("EncryptedData");

            using var second = BuildProvider(options);
            Unprotect(second, payload).Should().Be("hemmelig");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void WhenCertificateRequiredButMissing_RegistrationThrows()
    {
        var act = () => BuildProvider(new KeyProtectionOptions(), requireCertificate: true);

        act.Should().Throw<InvalidOperationException>();
    }

    private AccountBackupService CreateBackupService(ServiceProvider provider) =>
        new(new ContextFactory(CreateDbContext), provider.GetRequiredService<IDataProtectionProvider>(), TimeProvider.System);
}