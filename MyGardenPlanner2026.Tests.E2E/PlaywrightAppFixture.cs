namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;
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

    public string RootUri { get; private set; } = default!;
    public IBrowser Browser { get; private set; } = default!;
    public IReadOnlyDictionary<string, SmokeTestUser> SmokeTestUsers { get; private set; } = default!;

    public async ValueTask InitializeAsync()
    {
        _database = await E2ETestDatabase.CreateAsync();

        SmokeTestUsers = await SmokeTestDataSeeder.SeedAsync(_database.AppConnectionString);

        var port = GetFreeTcpPort();
        RootUri = $"http://127.0.0.1:{port}";
        _appProcess = StartAppProcess(port, _database.AppConnectionString, _database.AdminConnectionString);

        await WaitUntilReadyAsync(TimeSpan.FromSeconds(60));

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    private readonly List<IBrowserContext> _contexts = [];

    public async ValueTask DisposeAsync()
    {
        foreach (var context in _contexts)
        {
            await context.CloseAsync();
        }

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

        if (_database is not null)
        {
            await _database.DisposeAsync();
        }
    }

    public async Task<IPage> NewPageAsync()
    {
        var context = await Browser.NewContextAsync();
        _contexts.Add(context);
        return await context.NewPageAsync();
    }

    /// <summary>Åbner en ny side med viewport fra <see cref="Screenshots.Viewports"/>.</summary>
    public async Task<IPage> NewPageAsync(int viewportWidth)
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = Screenshots.Viewports.For(viewportWidth),
        });
        _contexts.Add(context);
        return await context.NewPageAsync();
    }

    /// <summary>Åbner en ny side på <paramref name="path"/> og logger ind som den angivne smoke-test-persona.</summary>
    public async Task<IPage> LoginAndGotoAsync(string persona, string path)
    {
        var page = await NewPageAsync();
        await page.GotoAsync($"{RootUri}{path}");
        await LoginFlow.LoginAsync(page, SmokeTestUsers[persona]);
        return page;
    }

    private static Process StartAppProcess(int port, string appConnectionString, string adminConnectionString)
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

        // Neutraliserer Program.cs' #if DEBUG SeedSmokeTestUsersAsync(): uden dette
        // overskriver appens egen (evt. user-secrets-konfigurerede) smoke-test-seeder
        // vores fixture-seedede brugeres 2FA-nøgler ved opstart, da e-mails er identiske.
        startInfo.EnvironmentVariables["SmokeTestUsers__Password"] = "";
        startInfo.EnvironmentVariables["SmokeTestUsers__AuthenticatorKey"] = "";

        // Login-rate-limiten (default 5 forsøg/60 sek., §4.1) er pr.-IP og deles af alle
        // tests i kollektionen. Hæves markant for E2E.
        startInfo.EnvironmentVariables["LoginRateLimit__PermitLimit"] = "1000";
        startInfo.EnvironmentVariables["LoginRateLimit__WindowSeconds"] = "60";

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException("Kunne ikke starte MyGardenPlanner2026.dll.");
    }

    private async Task WaitUntilReadyAsync(TimeSpan timeout)
    {
        using var client = new HttpClient();
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            if (_appProcess.HasExited)
            {
                var stdOut = await _appProcess.StandardOutput.ReadToEndAsync();
                var stdErr = await _appProcess.StandardError.ReadToEndAsync();
                throw new InvalidOperationException(
                    $"Appen afsluttede uventet (exit code {_appProcess.ExitCode}) under opstart.\n" +
                    $"StdOut:\n{stdOut}\nStdErr:\n{stdErr}");
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

    private static int GetFreeTcpPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}