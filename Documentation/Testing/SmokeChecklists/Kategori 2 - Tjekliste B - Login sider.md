# 2. Verifikationsproces pr. modul
## B. Registrering & login
- [ ] Opret bruger → bekræftelses-mail (no-op sender, link vises på siden)
- [ ] Login med forkert password 5× → konto låses (`ReAuthFailureTracker`, tjek mailkø for sikkerhedsalarm hvis SMTP er sat op)
- [ ] Aktivér 2FA (authenticator-app), log ud/ind → 2FA-prompt virker
- [ ] Recovery code login virker
- [ ] Passkey oprettelse + login (kræver browser med WebAuthn-support)
### B1. Oprettelse af bruger
- Route: `/` --> `/account/register` --> `/account/RegisterConfirmation`
- Bruger: ingen login
- Handling: opret login
- Forventet: modtag en bekræftelses mail med et link (vises reelt på siden `/account/RegisterConfirmation`)
- Skærmbillede: ja: `/account/register` + `/account/RegisterConfirmation` (375, 768, 939, 940, 1280)
- Filer: Components\Pages\LandingPage.razor, Components\Account\Pages\Register.razor, Components\Account\Pages\RegisterConfirmation.razor
### B2. Forkert login
- Pre-requisite: Kan der sættes en SMTP klient op i test? F.eks. smtp4dev
- Pre-requisite: Ny bruger oprettet og bekræftet via UI (hjælper). Authenticator-nøglen læses fra siden og omsættes til kode med `TotpHelper` (ingen rigtig app, ingen QR-scanning).
- Route: `/` --> `/account/Login` --> `/account/Lockout`
- Bruger: ny bruger
- Handling: Login med forkert password 5× 
- Forventet: konto låses (`ReAuthFailureTracker`, tjek mailkø for sikkerhedsalarm hvis SMTP er sat op)
- Skærmbillede: ja `/account/Lockout` (375, 768, 1280)
- Filer: Components\Pages\LandingPage.razor, Components\Account\Pages\Login.razor, Components\Account\Pages\Lockout.razor, Infrastructure\Services\ReAuthentication\ReAuthFailureTracker.cs
### B3. 2FA
- Pre-requisite: Ny bruger oprettet og bekræftet via UI (hjælper). Authenticator-nøglen læses fra siden og omsættes til kode med `TotpHelper` (ingen rigtig app, ingen QR-scanning).
- Route: `/account/login` --> `/Account/Manage/TwoFactorAuthentication` --> `/Account/Manage/EnableAuthenticator` --> (log ud) --> `/account/login` --> `/Account/LoginWith2fa`
- Bruger: ny bruger uden 2FA (fx `<guid>@test.dk`)
- Handling:
  1. Log ind med password (uden 2FA)
  2. Åbn `/Account/Manage/TwoFactorAuthentication`, klik "Add authenticator app"
  3. Læs nøglen fra `<kbd>` (fjern mellemrum, store bogstaver), generér TOTP, indtast under "Verification Code", klik "Verify"
  4. Log ud (ryd cookies), log ind med password
  5. Indtast TOTP på `/Account/LoginWith2fa`
  6. Negativ: forkert kode giver fejl og bliver på siden
- Forventet:
  - Efter trin 3: samme side viser "Gendannelseskoder" (10 koder) og succes-besked "Your authenticator app has been verified."
  - Trin 4: login stopper på `/Account/LoginWith2fa` (h1 "Totrinsbekræftelse")
  - Trin 5: login lykkes; `/Account/Manage` kan åbnes (h3 "Profile")
  - Trin 6: "Error: Ugyldig godkendelseskode."
- Skærmbillede: ja (375, 768, 1280): `EnableAuthenticator` (før Verify), `RecoveryCodes` (efter Verify), `LoginWith2fa`
- Filer: Components\Account\Pages\Login.razor(.cs), Components\Account\Pages\LoginWith2fa.razor(.cs), Components\Account\Pages\Manage\TwoFactorAuthentication.razor(.cs), Components\Account\Pages\Manage\EnableAuthenticator.razor(.cs), Components\Account\Shared\ShowRecoveryCodes.razor, Components\Account\Shared\StatusMessage.razor(.cs), Components\Account\Shared\ManageLayout.razor
- E2E-filer: LoginFlow.cs, TotpHelper.cs, SmokeTestUser.cs (genbruges via `user with { TwoFactorEnabled = true, AuthenticatorKey = key }`)
### B4. Recovery code
- Pre-requisite: Ny bruger med 2FA aktiveret via UI (samme hjælper som B3). Gendannelseskoderne læses fra `code.recovery-code` på EnableAuthenticator-siden.
- Route: `/account/login` --> `/Account/LoginWith2fa` --> `/Account/LoginWithRecoveryCode`
- Bruger: ny bruger med 2FA + 10 gendannelseskoder
- Handling:
  1. Ryd cookies, log ind med password
  2. På `/Account/LoginWith2fa`: klik "logge ind med en gendannelseskode"
  3. Indtast kode nr. 1 under "Gendannelseskode", klik "Log ind"
  4. Ny session (ryd cookies, password-login igen): forsøg samme kode igen
- Forventet:
  - Trin 3: login lykkes; `/Account/Manage` kan åbnes
  - Trin 4: "Error: Ugyldig gendannelseskode indtastet." og brugeren er ikke logget ind
  - Koden er altså engangs
- Skærmbillede: ja (375, 768, 1280): `LoginWithRecoveryCode` (normal) og `LoginWithRecoveryCode_Fejl` (efter trin 4)
- Filer: Components\Account\Pages\LoginWith2fa.razor, Components\Account\Pages\LoginWithRecoveryCode.razor(.cs), Components\Account\Pages\Manage\EnableAuthenticator.razor(.cs), Components\Account\Shared\ShowRecoveryCodes.razor, Components\Account\Shared\StatusMessage.razor(.cs)
### B5. Passkey
- Pre-requisite: Chromium med WebAuthn virtual authenticator (Playwright CDP: `WebAuthn.enable` + `addVirtualAuthenticator`, ctap2/internal, resident key, user verification, automaticPresenceSimulation). Siden skal åbnes på `http://localhost:<port>`, ikke `127.0.0.1` (se risici).
- Route: `/account/login` --> `/Account/Manage/Passkeys` --> `/Account/Manage/RenamePasskey/{id}` --> `/Account/Manage/Passkeys` --> (log ud) --> `/account/login`
- Bruger: ny bruger uden 2FA
- Handling:
  1. Log ind med password
  2. `/Account/Manage/Passkeys`: klik "Add a new passkey"
  3. På RenamePasskey: indtast "E2E-nøgle" under "Passkey name", klik "Continue"
  4. Ryd cookies (virtuel authenticator bevares på samme side), åbn `/account/login`
  5. Klik "Log ind med en passkey" (tomt e-mailfelt)
- Forventet:
  - Trin 2: redirect til `/Account/Manage/RenamePasskey/<credentialId>`
  - Trin 3: tilbage på Passkeys med "Passkey updated successfully." og "E2E-nøgle" i listen
  - Trin 5: login lykkes uden password; `/Account/Manage` kan åbnes
- Skærmbillede: ja (375, 768, 1280): `Passkeys` (med nøgle på listen), `RenamePasskey`, `Login`
- Filer: Components\Account\Pages\Login.razor(.cs), Components\Account\Pages\Manage\Passkeys.razor(.cs), Components\Account\Pages\Manage\RenamePasskey.razor(.cs), Components\Account\Shared\PasskeySubmit.razor(.js), Components\Account\PasskeyInputModel.cs, Components\Account\PasskeyOperation.cs, Components\Account\IdentityComponentsEndpointRouteBuilderExtensions.cs