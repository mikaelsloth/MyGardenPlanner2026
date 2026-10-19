# 2. Verifikationsproces pr. modul

## H. Rate limiting

- [ ] 5+ hurtige login-forsøg → 429 / lockout-besked
- [ ] Hurtige gentagne admin-handlinger (fx gem-knap spam) → rate-limit-fejl vises i UI

### Forudsætninger

- E2E-appen kører med Login PermitLimit = 1000, Admin-API PermitLimit = 100 og ReAuth MaxAge = 1 (PlaywrightAppFixture).
- Alle policyændringer gendannes i `finally`.

### H2. Hurtige gentagne admin-handlinger → rate-limit-fejl vises i UI

- Route: `/admin/security-policies`
- Bruger: ny PolicyAdmin A. Ny PolicyAdmin B åbnes før ændringen og bruges til gendannelse.
- Handling:
  a. A sætter Admin-API PermitLimit til 10 (fanen "Admin-API rate limit", step-up).
  b. A gemmer på fanen "Step-up re-auth" MaxAge 16, 17, 18, 19, 20, 19, 18, 17, 16, 15 i hurtig rækkefølge (Gem efter hver ændring). Herefter forsøges 25.
  c. I en finally gendanner B PermitLimit = 100 og MaxAge = 1.
- Forventet: Ved forsøg 11 vises "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen.", ingen step-up modal, og værdien er stadig 15.

- Skærmbilleder (768, 1280): `RateLimiting_H2_<bredde>`
- Filer: AdminSecurityPolicyPage.razor(.cs), ReAuthenticationPolicyEditor.razor(.cs), AdminActionGuard.cs, AdminActionRateLimiter.cs, AdminApiRateLimitingServicesExtensions.cs
- 