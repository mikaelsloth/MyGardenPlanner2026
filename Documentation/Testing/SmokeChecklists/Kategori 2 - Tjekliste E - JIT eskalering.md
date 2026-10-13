# 2. Verifikationsproces pr. modul
## E. JIT-eskalering (ny AuditViewer anmoder; Admin #1 og Auditor #4 godkender)
- [ ] Anmodning uden for `JitElevationPolicy`-grænser afvises
- [ ] Ansøger kan **ikke** se egen anmodning i godkendelseskøen
- [ ] Godkendelse → adgang til den beskyttede side virker straks
- [ ] Afvisning → adgang nægtes
- [ ] Sæt `SweepIntervalMinutes` lavt midlertidigt → verificér at udløbne anmodninger får status `Expired`

### Forudsætninger
- Anmoder (A) og peer (B) er friske brugere med rollen `AuditViewer` (`SeedExtraUserAsync`). #6 (Requester) har ingen admin-rolle og kan ikke åbne `/admin/jit-requests`.
- Godkender af `SystemAdmin`-anmodninger er Admin (#1). Godkender af `AuditViewer`-anmodninger er Auditor (#4).
- Hver anmodning har en unik årsag (GUID), fordi Admin og Auditor deles mellem tests.
- Step-up (password + TOTP) kræves kun ved Godkend/Afvis og ved Gem af policy, ikke ved selve anmodningen.
- Standardpolicy: Min 30, Max 90, Sweep 5 minutter.
- Rolleberettigelse (må ansøgeren anmode om rollen?) tjekkes ikke af servicen i dag. Det er på backloggen og ikke en del af E1.

### E1. Anmodning uden for `JitElevationPolicy`-grænser afvises
- Route: `/admin/jit-requests` (fanen "Anmod om adgang")
- Bruger: ny `AuditViewer` (A)
- Handling:
  a. Anmod om `SystemAdmin`, 29 minutter, unik årsag. Klik "Send anmodning".
  b. Anmod om `SystemAdmin`, 91 minutter, unik årsag.
  c. Kontrol: anmod om `SystemAdmin`, 30 minutter, unik årsag.
- Forventet:
  a. Fejl (`status-danger`, role=alert): "Error: RequestedMinutes skal være mellem 30 og 90." Ingen række i "Mine anmodninger", heller ikke med "Vis afviste/udløbne".
  b. Samme fejl. Ingen række.
  c. Ingen fejl. Rækken vises med status "Afventer" (beviser, at a/b blev afvist på grund af grænserne).
- Skærmbillede: ja, fejlen efter handling b (375, 768, 1280)
- Filer: `JitRequestsPage.cs` (ny), `ManualSmoke/TjeklisteE/AnmodningUdenForGraenserTests.cs`; produktion: `AdminJitRequestsPage.razor`, `JitElevationRequestForm.razor`, `JitElevationService.cs`, `JitElevationPolicyOptions.cs`

### E2. Ansøger kan **ikke** se egen anmodning i godkendelseskøen
- Route: `/admin/jit-requests` (fanen "Godkend anmodninger")
- Bruger: ny `AuditViewer` A og B, samt Auditor (#4)
- Handling:
  a. A og B anmoder hver om `AuditViewer`, 30 minutter, hver sin unikke årsag.
  b. A åbner "Godkend anmodninger".
  c. Auditor (#4) åbner "Godkend anmodninger".
- Forventet:
  b. B's række vises (positiv kontrol, testen venter på den). A's egen række vises ikke. Under "Anmod om adgang" → "Mine anmodninger" har A sin række med status "Afventer".
  c. Auditor ser både A's og B's række.
- Skærmbillede: ja, A's kø (375, 768, 939, 940, 1280)
- Filer: `JitRequestsPage.cs`, `ManualSmoke/TjeklisteE/EgenAnmodningIKoeTests.cs`; produktion: `JitElevationApprovalQueue.razor`, `JitElevationService.cs` (`GetPendingRequestsForApprovalAsync`)

### E3. Godkendelse → adgang til den beskyttede side virker straks
- Route: A: `/admin/subscriptions` og `/admin/jit-requests`; Admin: `/admin/jit-requests`
- Bruger: ny `AuditViewer` (A), Admin (#1)
- Handling:
  a. A åbner `/admin/subscriptions`.
  b. A anmoder om `SystemAdmin`, 30 minutter, unik årsag.
  c. Admin åbner "Godkend anmodninger", klikker "Godkend" på rækken og gennemfører step-up (password + TOTP).
  d. A åbner `/admin/subscriptions` igen (ny sideindlæsning, uden nyt login).
- Forventet:
  a. `/Account/AccessDenied`: "Du har ikke adgang til denne ressource."
  c. Statusbesked "Anmodning om 'SystemAdmin' er godkendt." Rækken forsvinder fra Admins kø.
  d. H1 "Administrer abonnementer". A's række på `/admin/jit-requests` har status "Godkendt".
- Skærmbillede: ja, før (AccessDenied) og efter (siden) (375, 768, 1280)
- Filer: `JitRequestsPage.cs`, `ManualSmoke/TjeklisteE/GodkendelseTests.cs`; produktion: `JitElevationApprovalQueue.razor`, `AnyAdminRoleAuthorizationHandler.cs`, `JitRoleAuthorizationHandler.cs`, `AuthorizationServicesExtensions.cs`

### E4. Afvisning → adgang nægtes
- Route/bruger: som E3
- Handling:
  a. A anmoder om `SystemAdmin`, 30 minutter, unik årsag.
  b. Admin klikker "Afvis" på rækken og gennemfører step-up.
  c. A åbner `/admin/subscriptions`.
  d. A åbner `/admin/jit-requests`, først uden og derefter med "Vis afviste/udløbne".
- Forventet:
  b. Statusbesked "Anmodning om 'SystemAdmin' er afvist."
  c. `/Account/AccessDenied`: "Du har ikke adgang til denne ressource."
  d. Rækken er skjult som standard. Med "Vis afviste/udløbne" vises den med status "Afvist".
- Skærmbillede: ja, AccessDenied efter afvisning (375, 768, 1280)
- Filer: `JitRequestsPage.cs`, `ManualSmoke/TjeklisteE/AfvisningTests.cs`

### E5. Sæt `SweepIntervalMinutes` lavt midlertidigt → verificér at udløbne anmodninger får status `Expired`
- Route: `/admin/security-policies` (fanen "JIT-eskalering", standardfane), `/admin/jit-requests`
- Bruger: ny `PolicyAdmin`, ny `AuditViewer` (A), Admin (#1)
- Handling:
  a. PolicyAdmin læser de nuværende værdier i `#jit-min` og `#jit-sweep`, sætter Min = 1 og Sweep = 1 og klikker "Gem" (step-up).
  b. A anmoder om `SystemAdmin`, 1 minut, unik årsag.
  c. Admin godkender (step-up).
  d. A's liste kontrolleres gentagne gange (op til ca. 150 sekunder) med "Vis afviste/udløbne" valgt.
  e. Til sidst gendanner PolicyAdmin de oprindelige værdier (`finally`).
- Forventet:
  a. "JIT-eskaleringspolicyen er opdateret og trådt i kraft."
  d. A's række skifter fra "Godkendt" til "Udløbet" senest ca. 60 sekunder efter udløb. Kun godkendte anmodninger udløber.
  e. Min/Max/Sweep er tilbage på 30/90/5, så E1 ikke påvirkes.
- Skærmbillede: ja, A's liste med "Udløbet" (375, 768, 1280)
- Filer: `JitRequestsPage.cs`, `SecurityPolicyPage.cs` (JIT-selektorer), `ManualSmoke/TjeklisteE/SweepUdloebTests.cs`; produktion: `JitElevationPolicyEditor.razor`, `RoleElevationExpirySweepService.cs`, `JitElevationPolicyOptions.cs`