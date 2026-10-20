# 2. Verifikationsproces pr. modul

## H. Rate limiting

- [ ] 6 hurtige login-forsøg → 429 (ingen lockout-besked)
- [ ] Hurtige gentagne admin-handlinger (fx gem-knap spam) → rate-limit-fejl vises i UI

### Forudsætninger

- E2E-appen kører med Login PermitLimit = 1000, Admin-API PermitLimit = 100 og ReAuth MaxAge = 1 (PlaywrightAppFixture).
- Alle policyændringer gendannes i `finally`.

### H1. 5+ hurtige login-forsøg → 429

- Route: `/Account/Login`
- Bruger: ny standard bruger A (uden 2FA) + ny PolicyAdmin.
- Handling:
  a. PolicyAdmin sætter Login rate limit PermitLimit til 5 (step-up).
  b. A logger ind 6 gange hurtigt efter hinanden (korrekt password, cookies ryddes mellem forsøgene).
  c. I en finally gendanner PolicyAdmin PermitLimit til 1000 (ny circuit i eksisterende context, intet login).
- Forventet: Forsøg 1-5 er ikke 429. Forsøg 6 giver HTTP 429. Der vises ingen lockout-besked (login bruger lockoutOnFailure: false, og OnRejected skriver ingen body). Efter gendannelsen kan A logge ind igen.

- Skærmbilleder (768, 1280): `RateLimiting_H1_<bredde>` (efter 6. forsøg; viser hvad brugeren ser ved 429)
- Filer: ReloadableLoginRateLimiter.cs, RateLimitingServicesExtensions.cs

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