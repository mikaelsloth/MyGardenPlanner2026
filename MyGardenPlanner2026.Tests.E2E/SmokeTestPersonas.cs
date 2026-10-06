namespace MyGardenPlanner2026.Tests.E2E;

/// <summary>Nøgler til de 7 smoke-test-brugere i <see cref="PlaywrightAppFixture.SmokeTestUsers"/>.</summary>
public static class SmokeTestPersonas
{
    public const string Admin = "Admin";
    public const string DataAdmin = "DataAdmin";
    public const string PolicyAdmin = "PolicyAdmin";
    public const string Auditor = "Auditor";
    public const string NoMfa = "NoMfa";
    public const string Requester = "Requester";
    public const string Plain = "Plain";
}