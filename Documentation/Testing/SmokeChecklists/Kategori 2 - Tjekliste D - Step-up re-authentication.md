# 2. Verifikationsproces pr. modul
## D. Step-up re-authentication
- [ ] Log ind, vent > `ReAuthenticationPolicy:MaxAgeMinutes` (eller sæt lav værdi midlertidigt), forsøg en beskyttet handling → modal vises
- [ ] Forkert password i modal → fejl vises, tælles i `ReAuthFailureTracker`
- [ ] Korrekt password (+ TOTP hvis 2FA) → handling gennemføres
### D1. Modal vises ved beskyttet handling
- Route: `/` --> `/Account/Login` --> (direkte URL, intet link i UI)
- Bruger: admin brugere + standard bruger
- Handling:
  a. log ind med forskellige brugere, vent > `ReAuthenticationPolicy:MaxAgeMinutes`
  b. Gå til `/admin/audit-log`, `/admin/security-policies`, `/admin/subscriptions`, `/admin/jit-requests`
- Forventet:
  a. Standard brugere afvises - får ikke lov at se siderne
  b. Admin brugere: ændre data --> vises modal til at requeste JIT
- Skærmbillede: ja visning af sider + modal (375, 768, 939, 940, 1280)
- Filer: Components\Pages\Admin\AdminAuditLogPage.razor(.cs), Components\Pages\Admin\AdminJitRequestsPage.razor(.cs), , Components\Pages\Admin\AdminSecurityPolicyPage.razor(.cs), Components\Pages\Admin\AdminSubscriptionManagementPage.razor(.cs)
### D2. Forkert password i modal
- Route: `/` --> `/Account/Login` --> (direkte URL, intet link i UI)
- Bruger: admin bruger
- Handling:
  a. log ind med admin bruger, vent > `ReAuthenticationPolicy:MaxAgeMinutes`
  b. Gå til `/admin/audit-log` --> lav ændring  -->  ved modal indtastes forkert password
- Forventet:
  a. Fejl vises
  b. Fejl registreres i `ReAuthFailureTracker`
- Skærmbillede: ja Visning af fejl side (375, 768, 939, 940, 1280)
- Filer: ReAuthFailureTracker.cs
### D3. Korrekt password (+ TOTP hvis 2FA) → handling gennemføres
- Route: `/` --> `/Account/Login` --> (direkte URL, intet link i UI)
- Bruger: admin bruger
- Handling:
  a. log ind med forskellige brugere med og uden MFA, vent > `ReAuthenticationPolicy:MaxAgeMinutes`
  b. Gå til `/admin/audit-log` --> lav ændring  -->  ved modal indtastes korrekt password
- Forventet:
  a. Prompt for MFA hvor påkrævet
  b. Ændringer gemmes og vises ved reload af side
- Skærmbillede: ja Visning af side før efter / ændring (375, 768, 1280)
 