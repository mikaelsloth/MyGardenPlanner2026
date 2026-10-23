# 2. Verifikationsproces pr. modul

## I. Adgangskontrol / navigation

- [x] `AdminNavMenu` viser kun links brugeren reelt har adgang til (test med #2, #3, #4 hver for sig)
- [x] Direkte URL til admin-side uden rolle → `AccessDenied` eller redirect til login

### Forudsætninger

- Personaer: #1 Admin, #2 DataAdmin, #3 PolicyAdmin, #4 Auditor (SmokeTestPersonas).
- Forventet adgang er hardcodet i `AdminAccessMatrix` (specifikation, ikke udledt af koden).
- Ingen policyændringer i denne tjekliste, så intet skal gendannes i `finally`.

### I1. `AdminNavMenu` viser kun links brugeren reelt har adgang til

- Route: personaens startside (`/admin/subscriptions`, `/admin/jit-requests`, `/admin/security-policies`, `/admin/audit-log`)
- Bruger: #2 DataAdmin, #3 PolicyAdmin og #4 Auditor hver for sig (+ #1 Admin som kontrol). I1d: en frisk AuditViewer + Admin (#1) som godkender.
- Handling:
  a. Log ind som personaen og åbn startsiden.
  b. Afvent siden (titel, én h1, ingen skeletons) og at personaens egne links er synlige.
  c. Tjek at øvrige links ikke findes, og at antallet af links matcher matricen.
  d. Tjek at linket til den aktuelle side har klassen `active`.
  e. Klik på hvert øvrigt synligt link: siden åbner uden AccessDenied.
  f. I1d: frisk AuditViewer ser kun JIT-adgang og AuditLog. Efter anmodning om SystemAdmin og godkendelse (step-up) og en ny sideindlæsning ses også Abonnementer.
- Forventet:
  - #1 Admin: Abonnementer, JIT-adgang.
  - #2 DataAdmin: JIT-adgang.
  - #3 PolicyAdmin: Sikkerhedspolicies, JIT-adgang.
  - #4 Auditor: JIT-adgang, AuditLog.
  - Efter godkendt JIT SystemAdmin: Abonnementer, JIT-adgang, AuditLog.

- Skærmbilleder (768, 1280): `Adgangskontrol_I1_<Persona>_<bredde>`, `Adgangskontrol_I1d_<bredde>`
- Filer: AdminNavMenu.razor(.cs), AdminLayout.razor, AuthorizationServicesExtensions.cs, AnyAdminRoleAuthorizationHandler.cs, JitRoleAuthorizationHandler.cs

### I2. Direkte URL til admin-side uden rolle → `AccessDenied` / login

- Route: `/admin/subscriptions`, `/admin/security-policies`, `/admin/jit-requests`, `/admin/audit-log`
- Bruger: anonym (I2a), #1–#4 på sider uden for deres adgang (I2b).
- Handling:
  a. Anonym: åbn hver rute direkte. Tjek URL (`/Account/Login`), `ReturnUrl` og at sidens h1 og navigation ikke vises.
  b. Krydsadgang: log ind som personaen via den afviste side (login-redirect og `ReturnUrl`). Tjek `/Account/AccessDenied` og at sidens h1 og navigation ikke vises. Gentag for personaens øvrige afviste sider.
  c. Kontrol: personaen kan åbne sin egen side.
- Forventet:
  - Anonym: redirect til login med `ReturnUrl` til den forespurgte side.
  - #1–#4 på afviste sider: AccessDenied ("Du har ikke adgang til denne ressource.").
  - Personaens egen side åbner stadig.

- Skærmbilleder (768, 1280): `Adgangskontrol_I2_Anonym_<Side>_<bredde>`, `Adgangskontrol_I2_AccessDenied_<Persona>_<Side>_<bredde>`
- Filer: Routes.razor(.cs), RedirectToLogin.razor(.cs), RedirectToAccessDenied.razor(.cs), AccessDenied.razor(.cs), AdminAccessMatrix.cs