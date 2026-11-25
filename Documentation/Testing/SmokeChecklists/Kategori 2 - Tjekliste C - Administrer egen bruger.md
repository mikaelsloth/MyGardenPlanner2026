# 2. Verifikationsproces pr. modul
## C. Kontoadministration
- [ ] Skift e-mail (bekræftelseslink)
- [ ] Skift password
- [ ] Download/slet personlige data
- [ ] Download backup før sletning

### C1. Skift e-mail (bekræftelseslink)
- Pre-requisite: Ny bruger oprettet og bekræftet via UI (RegisteredUserFlow) og logget ind (LoginFlow).
- Route: `/` --> `/Account/Login` --> (direkte URL, intet link i UI) --> `/Account/Manage/Email` --> `/Account/ConfirmEmailChange?...` (linket vises på Email-siden, fordi appen bruger IdentityNoOpEmailSender)
- Bruger: ny bruger. Ny e-mail genereres med RegisteredUserFlow.NewEmail() (ryddes af ResetSmokeTestUsersInDB.sql).
- Handling:
  a. Gå til `/Account/Manage/Email`, skift e-mail og følg bekræftelseslinket
  b. Log ud (ryd cookies) og log ind igen med den nye e-mail
  c. Log ud og forsøg at logge ind med den gamle e-mail
- Forventet:
  a. "Confirmation link to change email sent. Please check your email." og linket "Click here to confirm your new email". Linket åbner ConfirmEmailChange: "Tak, fordi du bekræftede skiftet af din e-mail."
  b. Login med den nye e-mail lykkes.
  c. Login med den gamle e-mail afvises ("Error: Invalid login attempt.").
- Negative tests:
  - Uændret e-mail: "Your email is unchanged." og intet link.
  - Ugyldig e-mail: valideringsfejl, ingen besked om sendt link.
  - Ugyldigt link (forkert token): "Error: Der opstod en fejl ved skift af e-mail."; den nye e-mail virker ikke og den gamle virker stadig.
- Skærmbillede: ja `/Account/Manage/Email` ved sideindlæsning og efter submit (375, 768, 939, 940, 1280)
- Filer: Components\Account\Pages\Manage\Email.razor(.cs), Components\Account\Pages\ConfirmEmailChange.razor(.cs)
### C2. Skift password
- Pre-requisite: Ny bruger oprettet og bekræftet via UI (RegisteredUserFlow) og logget ind (LoginFlow).
- Route: `/` --> `/Account/Login` --> (direkte URL, intet link i UI) --> `/Account/Manage/ChangePassword`
- Bruger: ny bruger
- Handling:
  a. Gå til ChangePassword og ændre password
  b. Log ud (ryd cookies) og log ind igen med det nye password
  c. Log ud og forsøg at logge ind med det gamle password
- Forventet:
  a. Ved gyldigt nyt password redirectes til samme side (`/Account/Manage/ChangePassword`) med beskeden "Your password has been changed".
  b. Login med det nye password lykkes.
  c. Login med det gamle password afvises ("Error: Invalid login attempt.").
- Negative tests:
  - Forkert nuværende password: "Error: Incorrect password."; det gamle password virker stadig.
  - Nyt og bekræftet password er forskellige: valideringsfejl "The new password and confirmation password do not match."
  - Nyt password under 6 tegn: valideringsfejl "The New password must be at least 6 and at max 100 characters long."
- Skærmbillede: ja `/Account/Manage/ChangePassword` ved sideindlæsning og efter skift (375, 768, 939, 940, 1280)
- Filer: Components\Account\Pages\Manage\ChangePassword.razor(.cs)
- Uden for scope: SetPassword.razor(.cs) (Pages\Manage). Den bruges kun af brugere uden lokalt password og redirecter ellers til ChangePassword.
### C3. Download/slet personlige data
- Pre-requisite: Ny bruger oprettet og bekræftet via UI (RegisteredUserFlow) og logget ind (LoginFlow).
- Route: `/` --> `/Account/Login` --> (direkte URL, intet link i UI) --> `/Account/Manage/PersonalData` --> (link "Delete") `/Account/Manage/DeletePersonalData`
- Bruger: ny bruger (hver test sin egen)
- Handling:
  a. Download personlige data (knappen "Download")
  b. Slet personlige data (link "Delete", indtast password, "Delete data and close my account"), og forsøg derefter at logge ind
- Forventet:
  a. Filen `PersonalData.json` hentes. JSON indeholder mindst "Email" (brugerens e-mail) og nøglen "Authenticator Key" (null for en ny bruger).
  b. Login med de slettede credentials afvises ("Error: Invalid login attempt."). Slet-formularen vises ikke længere.
- Negative tests:
  - Forkert password ved sletning: "Error: Incorrect password."; kontoen findes stadig (login virker).
- Skærmbillede: ja `/Account/Manage/PersonalData` og `/Account/Manage/DeletePersonalData` ved sideindlæsning (375, 768, 939, 940, 1280)
- Filer: Components\Account\Pages\Manage\PersonalData.razor(.cs), Components\Account\Pages\Manage\DeletePersonalData.razor(.cs), Components\Account\IdentityComponentsEndpointRouteBuilderExtensions.cs (POST /Account/Manage/DownloadPersonalData)
### C4. Download backup før sletning
- Pre-requisite: Smoke-bruger Plain (ingen 2FA) logget ind (LoginFlow).
- Route: `/` --> `/Account/Login` --> (direkte URL, intet link i UI) --> `/Account/Manage/DeletePersonalData` (kortet "Hent en backup, før du sletter")
- Bruger: Plain; NoMfa bruges kun til at sammenligne e-mail-hash.
- Handling:
  a. Klik "Download backup"
  b. Hent backup en gang til, og hent den som anden bruger (NoMfa)
- Forventet:
  a. Filen `MyGardenPlanner-backup.json` hentes. JSON: `format` = `MyGardenPlanner.AccountBackup`, `version` = 1, `createdUtc`, `emailHash` (64 hex-tegn), `signature`, `payload.personalData` med "Email" (uden "Authenticator Key") og `payload.gardens` (liste).
  b. `emailHash` er ens for samme bruger og forskellig mellem brugere.
- Negative tests: ingen. Signaturen kan ikke verificeres i E2E (appens nøglering er i en separat proces); den dækkes af xUnit (AccountBackupServiceTests, PersistedDataProtectionTests).
- Skærmbillede: nej (C3 viser kortet på `/Account/Manage/DeletePersonalData` ved alle bredder)
- Filer: Components\Account\Pages\Manage\DeletePersonalData.razor(.cs), Components\Account\IdentityComponentsEndpointRouteBuilderExtensions.cs (POST /Account/Manage/DownloadAccountBackup), AccountBackupService.cs
- 