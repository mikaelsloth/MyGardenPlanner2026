namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;

/// <summary>
/// WebAuthn virtual authenticator i Chromium via CDP. Gennemfører create/get uden UI
/// (automaticPresenceSimulation) og gemmer discoverable credentials (resident key).
/// Skal tilføjes FØR passkey-handlinger og bevarer credentials, så længe siden lever
/// (også efter ClearCookiesAsync). WebAuthn kræver et domænenavn som host (fx localhost), ikke en IP.
/// </summary>
public sealed class VirtualAuthenticator
{
    private readonly ICDPSession _session;
    private readonly string _authenticatorId;

    private VirtualAuthenticator(ICDPSession session, string authenticatorId)
    {
        _session = session;
        _authenticatorId = authenticatorId;
    }

    public static async Task<VirtualAuthenticator> AddAsync(IPage page)
    {
        var session = await page.Context.NewCDPSessionAsync(page);
        await session.SendAsync("WebAuthn.enable");

        var response = await session.SendAsync("WebAuthn.addVirtualAuthenticator", new Dictionary<string, object>
        {
            ["options"] = new Dictionary<string, object>
            {
                ["protocol"] = "ctap2",
                ["transport"] = "internal",
                ["hasResidentKey"] = true,
                ["hasUserVerification"] = true,
                ["isUserVerified"] = true,
                ["automaticPresenceSimulation"] = true,
            },
        });

        var authenticatorId = response?.GetProperty("authenticatorId").GetString()
            ?? throw new InvalidOperationException("WebAuthn.addVirtualAuthenticator gav intet authenticatorId.");

        return new VirtualAuthenticator(session, authenticatorId);
    }

    public async Task<int> CountCredentialsAsync()
    {
        var response = await _session.SendAsync("WebAuthn.getCredentials", new Dictionary<string, object>
        {
            ["authenticatorId"] = _authenticatorId,
        });

        return response?.GetProperty("credentials").GetArrayLength() ?? 0;
    }
}