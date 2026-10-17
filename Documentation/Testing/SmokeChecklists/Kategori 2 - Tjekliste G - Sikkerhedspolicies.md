# 2. Verifikationsproces pr. modul

## G. Admin – Sikkerhedspolicies (`/admin/security-policies`, kræver PolicyAdmin)

- [ ] Alle 6 faner (JIT-eskalering, Step-up re-auth, Fejl-tracker, Admin-API rate limit, Login rate limit, AuditLog-eksport) loader seedede værdier
- [ ] Gem på hver fane kræver step-up og opdaterer `IOptionsMonitor`-værdien **uden genstart** (bekræftes ved at udløse den relaterede adfærd bagefter)

### Forudsætninger

- Policy-værdier gemmes i databasen. De seedes ved første opstart fra `IOptions<T>` (appsettings + miljøvariabler).
- E2E-appen overstyrer to værdier via `PlaywrightAppFixture`: ReAuth `MaxAgeMinutes` = 1 (appsettings: 15) og Login `PermitLimit` = 1000 (appsettings: 5).
- Step-up (re-auth) er pr. circuit: modalen vises kun i en frisk circuit eller efter udløb. Hvert G2-scenarie starter derfor i en ny circuit (ny side i samme browser-context, uden nyt login).
- Login-limiteren tæller kun POST til `/account/login`, `/account/loginwith2fa` og `/account/loginwithrecoverycode`, pr. IP, i et fast vindue.
- Admin-API-limiteren er pr. bruger og ikke HTTP: afvisningen er UI-fejlen "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen."
- Hver Gem sender en sikkerhedsalarm ("Sikkerhedspolicy ændret") med Bruger-ID og Policy-navn til SMTP-sinken.

### G1. Sikkerhedspolicy-faner loader seedede værdier

- Route: `/admin/security-policies`, alle 6 faner
- Bruger: PolicyAdmin A (2FA). Ingen godkender.
- Handling for hver fane: A læser policy-værdierne.
- Forventet (E2E-seed):

| Fane | Felter | Forventet |
| ------ | ------ | ------ |
| JIT-eskalering | `jit-min` / `jit-max` / `jit-sweep` | 30 / 90 / 5 |
| Step-up re-auth | `reauth-maxage` | 1 |
| Fejl-tracker | `failtrack-threshold` / `failtrack-windowdays` | 5 / 2 |
| Admin-API rate limit | `adminapi-permit` / `adminapi-window` / `adminapi-segments` | 100 / 60 / 6 |
| Login rate limit | `login-permit` / `login-window` | 1000 / 60 |
| AuditLog-eksport | `exportjob-retention` / `exportjob-maxactive` | 24 / 3 |

- Skærmbilleder (768, 1280): `Sikkerhedspolicies_G1_<Jit|ReAuth|FejlTracker|AdminApi|Login|AuditLogEksport>_<bredde>`
- Filer: AdminSecurityPolicyPage.razor(.cs), JitElevationPolicyEditor, ReAuthenticationPolicyEditor, ReAuthFailureTrackerEditor, AdminApiRateLimitEditor, LoginRateLimitEditor, AuditLogExportJobPolicyEditor (.razor(.cs)), appsettings.json

### G2. Sikkerhedspolicy-ændringer træder i kraft med det samme

- Route: `/admin/security-policies`
- Bruger: ny PolicyAdmin A pr. scenarie (2FA). Ingen godkender.
- Handling for hvert scenarie:
  a. A læser policy-værdien (skal svare til "Før").
  b. A redigerer og gemmer én værdi jf. tabel (Gem => step-up i frisk circuit). Succesbesked og policy-ændringsmail (Bruger-ID + Policy-navn) kontrolleres.
  c. Kontroladfærd udføres.
  d. A gendanner den originale værdi i en ny circuit (ny step-up) og kontrollerer værdien.

| Fane | Værdi | Før | Efter | Kontroladfærd |
| ------ | ------ | ------ | ------ | ------ |
| JIT-eskalering | MinRequestedMinutes | 30 | 15 | Frisk AuditViewer anmoder om SystemAdmin i 20 min. Før ændringen afvises det (grænser 30–90), efter Gem oprettes en Afventer-række. |
| Step-up re-auth | MaxAgeMinutes | 1 | 10 | Efter Gem vent over 62 sek. En ny Gem (gendan 1) i samme circuit viser ingen modal. En ny side (ny circuit) viser modal. |
| Fejl-tracker | Threshold | 5 | 2 | Ny bruger uden 2FA, to forkerte passwords: efter 1. ingen alarm-mail med Bruger-ID, efter 2. kommer alarmen. |
| Admin-API rate limit | PermitLimit | 100 | 10 | Frisk admin B sender 10 JIT-anmodninger (OK). Den 11. giver "For mange handlinger på kort tid". Ingen HTTP 429. |
| Login rate limit | PermitLimit | 1000 | 2 | Frisk bruger C uden 2FA: 3 loginforsøg (POST). 1.–2. er ikke 429, 3. er 429. |
| AuditLog-eksport | RetentionHours | 24 | 1 | Eksport gennemføres. `ExpiresAtUtc − CompletedAtUtc` ≈ 1 time (SQL). |

- Forventet: ændringen træder i kraft direkte efter hver Gem, og kontroladfærden opfyldes.
- Filer: se G1 samt JitElevationRequestForm, AdminActionGuard, StepUpGuard, ReloadableLoginRateLimiter, audit-log-siden med eksport.

### Kendte fund (rettes i separat PR)

- `AdminActionGuard` bruger en kvote-permit, før step-up er gennemført (også hvis modalen annulleres).
- `AdminActionGuard` viser rate-limit-fejlen, hvis bruger-id ikke kan bestemmes.