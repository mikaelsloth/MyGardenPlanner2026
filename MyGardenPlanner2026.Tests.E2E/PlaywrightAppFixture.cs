namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Smtp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// Starter appen som en RIGTIG separat proces (dotnet MyGardenPlanner2026.dll) på en
/// tilfældig ledig port, mod en engangs-SQL-database. Undgår WebApplicationFactory helt,
/// da dens Server-property er hårdkodet til TestServer. Lokalt bruges Trusted_Connection
/// mod .\SQLEXPRESS til alt; i CI (E2ESqlEnvironment.IsCi) provisioneres database,
/// admin-schema og de to begrænsede databasebrugere først (se CiSqlProvisioner), og
/// appen startes med de reelle mgp_app_user/mgp_admin_user-forbindelser.
/// </summary>
public sealed class PlaywrightAppFixture : IAsyncLifetime
{
    private E2ETestDatabase _database = default!;
    private Process _appProcess = default!;
    private IPlaywright _playwright = default!;
    private const int MaxCapturedLines = 200;
    private readonly ConcurrentQueue<string> _appOutput = new();
    private AppLogFile? _appLog;
    private string? _lastLoggedTest;

    public string RootUri { get; private set; } = default!;
    public const string SecurityAlertRecipient = "sikkerhedsalarm@e2e.test";
    public const string SecurityAlertSubject =
    "[MyGardenPlanner] Sikkerhedsalarm: Gentagne fejlede login-/re-auth-forsøg";
    /// <summary>E2E-appens re-auth-vindue. 1 minut gør, at udløbstesten (Tjekliste D3) kun venter ca. ét minut.</summary>
    public const int ReAuthMaxAgeMinutes = 1;
    /// <summary>Standard-timeout for Expect. CI-runnere er langsomme til at forbinde Blazor-circuits.</summary>
    public const float ExpectTimeoutMs = 15_000;

    public TestSmtpServer SmtpServer { get; private set; } = default!;
    public string AppConnectionString => _database.AppConnectionString;
    public IBrowser Browser { get; private set; } = default!;
    public IReadOnlyDictionary<string, SmokeTestUser> SmokeTestUsers { get; private set; } = default!;

    public async ValueTask InitializeAsync()
    {
        _database = await E2ETestDatabase.CreateAsync();

        SmokeTestUsers = await SmokeTestDataSeeder.SeedAsync(_database.AppConnectionString);

        SmtpServer = TestSmtpServer.Start();

        var port = GetFreeTcpPort();
        RootUri = $"http://127.0.0.1:{port}";
        _appLog = new AppLogFile(Path.Combine(RepoPaths.FindRepoRoot(), "TestResults", "AppLogs", $"app-{port}.log"));
        _appProcess = StartAppProcess(
            port, _database.AppConnectionString, _database.AdminConnectionString, SmtpServer.Port, CaptureAppOutput);
        await WaitUntilReadyAsync(TimeSpan.FromSeconds(60));

        _playwright = await Playwright.CreateAsync();
        Assertions.SetDefaultExpectTimeout(ExpectTimeoutMs);
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    private readonly PerTestResources<IBrowserContext> _contexts =
        new(() => TestContext.Current.Test?.UniqueID, context => context.CloseAsync());

    public async ValueTask DisposeAsync()
    {
        await _contexts.CloseAllAsync();

        if (Browser is not null)
        {
            await Browser.DisposeAsync();
        }

        _playwright?.Dispose();

        if (_appProcess is not null && !_appProcess.HasExited)
        {
            _appProcess.Kill(entireProcessTree: true);
            await _appProcess.WaitForExitAsync();
        }

        _appProcess?.Dispose();
        _appLog?.Dispose();

        if (SmtpServer is not null)
        {
            await SmtpServer.DisposeAsync();
        }

        if (_database is not null)
        {
            await _database.DisposeAsync();
        }
    }

    public async Task<IPage> NewPageAsync()
    {
        LogTestStart();
        await _contexts.ReleaseIfNewTestAsync();
        var context = await Browser.NewContextAsync();
        _contexts.Add(context);
        return await context.NewPageAsync();
    }

    /// <summary>Åbner en ny side med viewport fra <see cref="Screenshots.Viewports"/>.</summary>
    public async Task<IPage> NewPageAsync(int viewportWidth)
    {
        LogTestStart();
        await _contexts.ReleaseIfNewTestAsync();
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = Screenshots.Viewports.For(viewportWidth),
        });
        _contexts.Add(context);
        return await context.NewPageAsync();
    }

    public Task<SmokeTestUser> SeedExtraUserAsync(string role) =>
        SmokeTestDataSeeder.SeedExtraUserAsync(AppConnectionString, role);

    /// <summary>Som persona-overloaden, men for en vilkårlig seedet bruger.</summary>
    public async Task<IPage> LoginAndGotoAsync(SmokeTestUser user, string path, int? viewportWidth)
    {
        var page = viewportWidth is null ? await NewPageAsync() : await NewPageAsync((int)viewportWidth);
        await page.GotoAsync($"{RootUri}{path}");
        await LoginFlow.LoginAsync(page, user);
        return page;
    }

    public Task<IPage> LoginAndGotoAsync(string persona, string path, int? viewportWidth = null) =>
    LoginAndGotoAsync(SmokeTestUsers[persona], path, viewportWidth);

    /// <summary>
    /// Appens stdout/stderr skal tømmes løbende: er de omdirigeret og ulæste, fylder røret op, og
    /// konsol-loggeren blokerer requests, der logger (fx DB-sider med EF-kommandoer). De seneste linjer
    /// gemmes til diagnostik, hvis appen crasher under opstart.
    /// </summary>
    private void CaptureAppOutput(string? line)
    {
        if (line is null)
        {
            return;
        }

        _appLog?.WriteLine(line);
        _appOutput.Enqueue(line);

        while (_appOutput.Count > MaxCapturedLines && _appOutput.TryDequeue(out _))
        {
        }
    }

    private static Process StartAppProcess(
        int port, string appConnectionString, string adminConnectionString, int smtpPort, Action<string?> onOutput)
    {
        var dllPath = Path.Combine(AppContext.BaseDirectory, "MyGardenPlanner2026.dll");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = AppContext.BaseDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add(dllPath);

        startInfo.EnvironmentVariables["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";
        startInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.EnvironmentVariables["DatabaseProvider"] = "SqlExpressConnection";
        startInfo.EnvironmentVariables["ConnectionStrings__SqlExpressConnection"] = appConnectionString;
        startInfo.EnvironmentVariables["ConnectionStrings__AdminSqlExpressConnection"] = adminConnectionString;

        // Links (RegisterConfirmation/Email) vises kun med dette flag (R2.1/R2.2). E2E bygger på dem.
        startInfo.EnvironmentVariables["Identity__ShowEmailLinks"] = "true";

        // Neutraliserer Program.cs' #if DEBUG SeedSmokeTestUsersAsync(): uden dette
        // overskriver appens egen (evt. user-secrets-konfigurerede) smoke-test-seeder
        // vores fixture-seedede brugeres 2FA-nøgler ved opstart, da e-mails er identiske.
        startInfo.EnvironmentVariables["SmokeTestUsers__Password"] = "";
        startInfo.EnvironmentVariables["SmokeTestUsers__AuthenticatorKey"] = "";

        // Login-rate-limiten (default 5 forsøg/60 sek., §4.1) er pr.-IP og deles af alle
        // tests i kollektionen. Hæves markant for E2E.
        startInfo.EnvironmentVariables["LoginRateLimit__PermitLimit"] = "1000";
        startInfo.EnvironmentVariables["LoginRateLimit__WindowSeconds"] = "60";

        // Step-up re-auth: kort vindue, så udløb kan testes (appsettings: 15 min).
        startInfo.EnvironmentVariables["ReAuthenticationPolicy__MaxAgeMinutes"] = $"{ReAuthMaxAgeMinutes}";

        // Peger sikkerhedsalarmer på TestSmtpServer. Uden dette bruger appen appsettings' Smtp:Host
        // (ikke tilgængelig i CI), og 5. forkerte login giver 500 (SmtpException). Tomme credentials
        // neutraliserer evt. user-secrets. Overstyrer index 0 i AdminSecurityEmails.
        startInfo.EnvironmentVariables["Smtp__Host"] = "127.0.0.1";
        startInfo.EnvironmentVariables["Smtp__Port"] = $"{smtpPort}";
        startInfo.EnvironmentVariables["Smtp__EnableSsl"] = "false";
        startInfo.EnvironmentVariables["Smtp__UserName"] = "";
        startInfo.EnvironmentVariables["Smtp__Password"] = "";
        startInfo.EnvironmentVariables["Smtp__AdminSecurityEmails__0"] = SecurityAlertRecipient;

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Kunne ikke starte MyGardenPlanner2026.dll.");

        process.OutputDataReceived += (_, e) => onOutput(e.Data);
        process.ErrorDataReceived += (_, e) => onOutput(e.Data);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }

    private async Task WaitUntilReadyAsync(TimeSpan timeout)
    {
        using var client = new HttpClient();
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            if (_appProcess.HasExited)
            {
                await _appProcess.WaitForExitAsync();

                throw new InvalidOperationException(
                    $"Appen afsluttede uventet (exit code {_appProcess.ExitCode}) under opstart.\n" +
                    $"Seneste output:\n{string.Join(Environment.NewLine, _appOutput)}");
            }

            try
            {
                var response = await client.GetAsync(RootUri);
                if ((int)response.StatusCode < 500)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // appen lytter endnu ikke — prøv igen
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250));
        }

        throw new TimeoutException($"Appen svarede ikke på {RootUri} inden for {timeout}.");
    }

    /// <summary>Markerer i app-loggen, hvornår en ny test går i gang (første sideoprettelse i testen).</summary>
    private void LogTestStart()
    {
        var name = TestContext.Current.Test?.TestDisplayName;
        if (name is null || name == _lastLoggedTest)
        {
            return;
        }

        _lastLoggedTest = name;
        _appLog?.WriteLine($"=== TEST START: {name}");
    }

    private static int GetFreeTcpPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}