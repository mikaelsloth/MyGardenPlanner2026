# 2. Verifikationsproces pr. modul

## I. Adgangskontrol / navigation

- [ ] `AdminNavMenu` viser kun links brugeren reelt har adgang til (test med #2, #3, #4 hver for sig)
- [ ] Direkte URL til admin-side uden rolle → `AccessDenied` eller redirect til login

### Forudsætninger

- Alle policyændringer gendannes i `finally`.

### I1. `AdminNavMenu` viser kun links brugeren reelt har adgang til

- Route: /admin/subscriptions, /admin/security-policies, /admin/jit-requests og /admin/audit-log. Hver persona åbner sin egen startside.
- Bruger: #2 DataAdmin, #3 PolicyAdmin, #4 Auditor, hver for sig. Evt. #1 Admin som kontrol, da det er den eneste, der ser “Abonnementer”.
- Handling:
  a. fixture.LoginAndGotoAsync(persona, startside, bredde).
  b. Afvent SmokePageAssertions.ExpectPageAsync.
  c. Positiv kontrol først: afvent, at personaens eget link er synligt. Nav’en renderes først ved prerender og igen ved circuit, så en negativ assertion kan ellers bestå for tidligt (samme læring som godkendelseskøen i E2).
  d. Tæl links i nav.admin-nav-menu li og sammenlign med forventet antal.
  e. For hvert af de fire links: synligt ⇔ forventet, valgt via href-suffiks (a[href$='admin/audit-log'] osv.).
  f. Klik på hvert synligt link og tjek, at målsiden åbner (h1) og ikke giver AccessDenied.
  g. Skærmbillede.
- Forventet:
  a. Præcis de forventede links, hverken flere eller færre.
  b. Hvert synligt link fører til en side brugeren kan åbne.
  c. Det aktuelle sides link har klassen active.
- Skærmbilleder (768, 1280): Adgangskontrol_I1_<Persona>_<bredde>, taget efter positiv kontrol.
- Filer: AdminNavMenu.razor(.cs/.css), AdminLayout.razor(.css), Pages/Admin/_Imports.razor, de fire admin-sider, AccessDenied.razor(.cs), Routes.razor(.cs)

### I2. Direkte URL til admin-side uden rolle → `AccessDenied`

- Route: alle fire admin-ruter via E2ERoutes.
- Bruger:
  I2a anonym: ingen login.
  I2b krydsadgang: #2, #3 og #4 på de sider, de ikke har adgang til.
  Allerede dækket og genbruges ikke: Requester, NoMfa og Plain.
- Handling:
  a. Anonym: fixture.NewPageAsync(bredde), derefter GotoAsync(rootUri + rute). Tjek URL.
  b. Krydsadgang: fixture.LoginAndGotoAsync(persona, rute, bredde). Tjek AccessDenied med AccessAssertions.ExpectAccessDeniedAsync.
  c. Den forbudte side leverer ikke sit h1 (fx “Administrer abonnementer”). Tjek h1-antal 0 for det.
  d. Skærmbillede.
- Forventet:
  a. Anonym: redirect til login (URL indeholder /Account/Login og ReturnUrl).
  b. Krydsadgang: /Account/AccessDenied, titel og h1 “Adgang nægtet”, teksten “Du har ikke adgang til denne ressource.”
  c. Sidens indhold vises ikke.
  d. Kontrol: den samme persona kan åbne sin egen side. Dermed er fejlen rolle, ikke login.
Skærmbilleder (768, 1280): Adgangskontrol_I2_Anonym_<bredde>, Adgangskontrol_I2_AccessDenied_<Persona>_<bredde>
Filer: AdminNavMenu.razor(.cs/.css), AdminLayout.razor(.css), Pages/Admin/_Imports.razor, de fire admin-sider, AccessDenied.razor(.cs), Routes.razor(.cs)