# 2. Verifikationsproces pr. modul
## E. JIT-eskalering (ny AuditViewer anmoder; Admin #1 og Auditor #4 godkender)
- [ ] Anmodning uden for `JitElevationPolicy`-grænser afvises
- [ ] Ansøger kan **ikke** se egen anmodning i godkendelseskøen
- [ ] Godkendelse → adgang til den beskyttede side virker straks
- [ ] Afvisning → adgang nægtes
- [ ] Sæt `SweepIntervalMinutes` lavt midlertidigt → verificér at udløbne anmodninger får status `Expired`
- [ ] Anmodning afvises, hvis ansøger allerede er medlem af rollen eller har en afventende/aktiv anmodning
- [ ] Anmodning og godkendelse sender sikkerhedsalarm til admin-listen

### Forudsætninger
- A er en frisk AuditViewer; B i E2 er en frisk PolicyAdmin”. Erstat linjen “Rolleberettigelse ... tjekkes ikke ...” med: “Servicen afviser anmodning om en rolle, ansøger allerede har (E6). Godkend/Afvis kræver direkte medlemskab af rollen.
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
  a. Fejl (`status-danger`, role=alert): "Error: 29 minutter ligger uden for grænsen. Antal minutter skal være mellem 30 og 90." Ingen række i "Mine anmodninger", heller ikke med "Vis afviste/udløbne".
  b. Fejl: "Error: 91 minutter ligger uden for grænsen. Antal minutter skal være mellem 30 og 90". Ingen række.
  c. Success besked vises: ’Anmodning om ‘<rolle>‘ er sendt.’. Rækken vises med status "Afventer" (beviser, at a/b blev afvist på grund af grænserne).
- Skærmbillede: ja, fejlen efter handling b (375, 768, 1280)
- Filer: `JitRequestsPage.cs` (ny), `ManualSmoke/TjeklisteE/AnmodningUdenForGraenserTests.cs`; produktion: `AdminJitRequestsPage.razor`, `JitElevationRequestForm.razor`, `JitElevationService.cs`, `JitElevationPolicyOptions.cs`

### E2. Ansøger kan **ikke** se egen anmodning i godkendelseskøen
- Route: `/admin/jit-requests` (fanen "Godkend anmodninger")
- Bruger: ny `AuditViewer` A og B, samt Auditor (#4)
- Handling:
  a. A (AuditViewer) anmoder om SystemAdmin; B (PolicyAdmin) anmoder om AuditViewer. 
  b. A åbner “Godkend anmodninger”. 
  c. Auditor (#4) åbner den. 
  d. Admin (#1) åbner den.
- Forventet:
  a. B’s række vises med B’s e-mail (positiv kontrol), A’s egen ikke. A’s række står under “Mine anmodninger” som “Afventer”. 
  b. Auditor ser B’s række, ikke A’s. 
  c. Admin ser A’s række, ikke B’s.
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
  a. A anmoder om SystemAdmin (afvises) og om PolicyAdmin (kontrol, forbliver afventende).
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
  d. A's liste kontrolleres gentagne gange (op til 3 minutter) med "Vis afviste/udløbne" valgt. Skærmbillederne tages bagefter på nye sider for A i hver bredde.  
  e. Til sidst gendanner PolicyAdmin de oprindelige værdier (`finally`).
- Forventet:
  a. "JIT-eskaleringspolicyen er opdateret og trådt i kraft."
  d. A's række skifter fra "Godkendt" til "Udløbet" senest ca. 60 sekunder efter udløb. Kun godkendte anmodninger udløber.
  e. Min/Max/Sweep er tilbage på 30/90/5, så E1 ikke påvirkes.
- Skærmbillede: ja, A's liste med "Udløbet" (375, 768, 1280)
- Filer: `JitRequestsPage.cs`, `SecurityPolicyPage.cs` (JIT-selektorer), `ManualSmoke/TjeklisteE/SweepUdloebTests.cs`; produktion: `JitElevationPolicyEditor.razor`, `RoleElevationExpirySweepService.cs`, `JitElevationPolicyOptions.cs`

### E6 Anmodning afvises, hvis ansøger allerede er medlem af rollen eller har en afventende/aktiv anmodning
- route /admin/jit-requests
- bruger ny AuditViewer
- Handling:
  a. A (AuditViewer) anmoder om SystemAdmin; B (PolicyAdmin) anmoder om AuditViewer. 
  b. A åbner “Godkend anmodninger”. 
  c. Auditor (#4) åbner den. 
  d. Admin (#1) åbner den.
- Forventet:
  a. B’s række vises med B’s e-mail (positiv kontrol), A’s egen ikke. A’s række står under “Mine anmodninger” som “Afventer”. 
  b. Auditor ser B’s række, ikke A’s. 
  c. Admin ser A’s række, ikke B’s.
- skærmbillede ja (375, 768, 1280) 
- filer AnmodningAfvisesTests.cs

### E7 Anmodning og godkendelse sender sikkerhedsalarm til admin-listen
- route /admin/jit-requests
- bruger ny AuditViewer (A) og Admin (#1)
- handling 
  a. A anmoder om SystemAdmin, 30 minutter
  b. Admin godkender (step-up)
- forventet 
  a. mail med emne “...JIT-adgang anmodet” og Bruger-ID, Rolle, Minutter
  b. mail med emne “...tildelt/aktiveret” og Bruger-ID, Godkender-ID, Rolle, Minutter
- skærmbillede nej
- filer SikkerhedsalarmTests.cs, SecurityAlerts.cs