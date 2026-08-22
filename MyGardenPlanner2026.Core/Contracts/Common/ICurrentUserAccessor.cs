namespace MyGardenPlanner2026.Core.Contracts.Common;

/// <summary>
/// Abstraktion der giver Infrastructure adgang til "hvem er den aktuelle bruger" uden at
/// kende ASP.NET Core HttpContext. Implementeres i Web-projektet.
/// OBS: I langvarige Blazor Server interactive circuits kan HttpContext være null —
/// implementeringen skal være null-sikker og returnere en tom CurrentUserInfo i stedet
/// for at kaste exception.
/// </summary>
public interface ICurrentUserAccessor
{
    CurrentUserInfo GetCurrent();
}