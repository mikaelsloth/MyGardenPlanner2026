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
- Pre-requisite: Ny bruger oprettet og bekræftet via UI (RegisteredUserFlow). SMTP/smtp4dev udskudt til PR3.
- Route: `/` --> `/account/Login`. Test b: --> `/account/Lockout`
- Bruger: ny bruger (ikke Requester)
- Handling:
  a. Login med forkert password 5×, derefter login med korrekt password
  b. Bruger låses direkte i databasen (LockoutEnd), derefter login
- Forventet:
  a. Hvert forsøg bliver på Login med "Error: Invalid login attempt."; det 6. forsøg med korrekt password lykkes (nuværende adfærd: ingen lockout)
  b. Redirect til `/Account/Lockout` (titel "Konto låst – MyGardenPlanner", h1 "Konto låst")
- Alarm (PR3): sikkerhedsalarm ved 5. forsøg, verificeres via smtp4dev
- Skærmbillede: ja `/Account/Lockout` (375, 768, 1280)
- Filer: Login.razor(.cs), Lockout.razor, ReAuthFailureTracker.cs, ReAuthFailureTrackerOptions.cs
### B3. 2FA
- Pre-requisite: Ny bruger oprettet og bekræftet via UI (RegisteredUserFlow). Nøglen læses fra EnableAuthenticator-siden og omsættes til kode med TotpHelper (ingen rigtig app, ingen QR-scanning).
- Route: `/account/login` --> `/Account/Manage/TwoFactorAuthentication` --> `/Account/Manage/EnableAuthenticator` --> (log ud) --> `/account/login` --> `/Account/LoginWith2fa`
- Bruger: ny bruger uden 2FA
- Handling: se TofaktorTests (login, aktivér, log ud, log ind med forkert og korrekt kode)
- Forventet:
  - 2FA-oversigt før aktivering: "Add authenticator app", ingen "Disable 2FA"
  - Efter Verify: "Gendannelseskoder" (10 koder) og "Your authenticator app has been verified."
  - 2FA-oversigt efter aktivering: "Disable 2FA", ingen "Add authenticator app"
  - Login stopper på `/Account/LoginWith2fa` (h1 "Totrinsbekræftelse")
  - Forkert kode: "Error: Ugyldig godkendelseskode."; korrekt kode logger ind
- Skærmbillede: ja (375, 768, 1280): 2FA_EnableAuthenticator, 2FA_RecoveryCodes, 2FA_LoginWith2fa
- Filer: Login.razor(.cs), LoginWith2fa.razor(.cs), Manage\TwoFactorAuthentication.razor(.cs), Manage\EnableAuthenticator.razor(.cs), Shared\ShowRecoveryCodes.razor(.cs), Shared\StatusMessage.razor(.cs), Shared\ManageLayout.razor
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