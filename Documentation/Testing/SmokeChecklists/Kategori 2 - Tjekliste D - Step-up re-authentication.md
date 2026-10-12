# 2. Verifikationsproces pr. modul
## D. Step-up re-authentication
- [ ] Beskyttet handling i en frisk circuit → modal "Bekræft din identitet" vises (ingen ventetid; login markerer ikke re-auth i circuit)
- [ ] Forkert password i modal → fejl vises, tælles i `ReAuthFailureTracker`
- [ ] Korrekt password + TOTP → handling gennemføres
- [ ] Inden for `ReAuthenticationPolicy:MaxAgeMinutes` → ingen ny modal; efter udløb i samme circuit → modal igen
### D1. Modal vises ved beskyttet handling
- Route: `/` --> `/Account/Login` --> (direkte URL, intet link i UI)
- Bruger: Admin, PolicyAdmin, Auditor, DataAdmin, Requester
- Handling:
  a. Requester logger ind og går til de fire admin-ruter
  b. Admin-brugere logger ind og udløser en beskyttet handling: `/admin/subscriptions` ("Gem"), `/admin/security-policies` ("Gem"), `/admin/audit-log` ("Eksportér"). `/admin/jit-requests` vises kun (Godkend/Afvis kræver en ventende anmodning)
- Forventet:
  a. Requester får AccessDenied (Plain og NoMfa redirectes til EnableAuthenticator, jf. PersonaAccessTests)
  b. Modal "Bekræft din identitet" med password- og TOTP-felt; Annullér lukker uden ændring
- Skærmbillede: ja side + modal (375, 768, 939, 940, 1280)
- Filer: AdminSubscriptionManagementPage, AdminSecurityPolicyPage, AdminAuditLogPage, AdminJitRequestsPage, StepUpReAuthModal, StepUpGuard, AccessDenied
### D2. Forkert password i modal
- Route: `/` --> `/Account/Login` --> (direkte URL, intet link i UI)
- Bruger: ny PolicyAdmin pr. test (isoleret fejltæller)
- Handling:
  a. `/admin/security-policies`, fanen "AuditLog-eksport" --> "Gem" --> forkert password i modal
  b. Korrekt password med tom/forkert TOTP
  c. 5 forkerte forsøg i samme modal
- Forventet:
  a. "Error: Forkert adgangskode." vises, modalen forbliver åben, intet gemmes
  b. "Error: Indtast venligst din godkendelseskode." (tælles ikke) / "Error: Ugyldig godkendelseskode." (tælles)
  c. Sikkerhedsalarm sendes ved 5. fejl (tælles i `ReAuthFailureTracker`; Bruger-ID i mailen)
- Skærmbillede: ja visning af fejl (375, 768, 939, 940, 1280)
- Filer: StepUpReAuthModal, ReAuthFailureTracker, AuditLogExportJobPolicyEditor, AdminSecurityPolicyPage
### D3. Korrekt password + TOTP → handling gennemføres
- Route: `/` --> `/Account/Login` --> (direkte URL, intet link i UI)
- Bruger: ny PolicyAdmin pr. test; NoMfa som negativ test
- Handling:
  a. `/admin/security-policies`, fanen "AuditLog-eksport": ændr `#exportjob-retention` --> "Gem" --> korrekt password + TOTP i modal
  b. Reload siden
  c. Ny handling inden for `MaxAgeMinutes`, derefter efter udløb (1 min i E2E) i samme circuit
  d. NoMfa åbner de fire admin-ruter
- Forventet:
  a. Prompt for TOTP; ændringen gemmes ("AuditLog-eksportjob-policyen er opdateret og trådt i kraft.")
  b. Ændringen vises efter reload (nyt circuit; ny modal ved næste handling); værdien gendannes bagefter
  c. Ingen modal inden for vinduet; modal vises igen efter udløb
  d. NoMfa redirectes til EnableAuthenticator
- Skærmbillede: ja før/efter ændring (375, 768, 1280)
- Filer: StepUpReAuthModal, StepUpGuard, RequireRecentAuthenticationHandler, ReAuthenticationService, AuditLogExportJobPolicyEditor, AdminSecurityPolicyPage
 