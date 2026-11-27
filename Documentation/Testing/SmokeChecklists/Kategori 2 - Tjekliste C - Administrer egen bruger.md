# 2. Verifikationsproces pr. modul
## C. Kontoadministration
- [ ] Skift e-mail (bekræftelseslink)
- [ ] Skift password
- [ ] Download/slet personlige data
- [ ] Download backup før sletning
- [ ] Slet konto: haver med flere medlemmer
- [ ] Slet konto: gemte haver og tilbud om gendannelse
- [ ] Gendan en gemt have i checkout

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
### C5. Slet konto: haver med flere medlemmer
- Pre-requisite: Nye brugere uden 2FA og uden rolle (SeedExtraUserAsync) og haver seedet direkte i databasen (GardenTestData). Brugeren er logget ind.
- Route: `/Account/Manage/DeletePersonalData` (direkte URL)
- Handling:
  a. Eneste ejer i en have med andre medlemmer: vælg "Overdrag ejerskab til …", indtast adgangskode, klik "Slet data og luk min konto"
  b. Vælg "Slet haven for alle medlemmer", skriv havens navn (trimmet, versalfølsomt ikke), indtast adgangskode, slet
  c. Intet valg for haven, eller forkert havenavn
  d. To haver: forskellige valg i samme indsendelse
  e. Én af to ejere (ingen blokering)
- Forventet:
  a. Redirect til forsiden. Modtageren er ejer og Administrator (niveau uændret), haven bevares, brugerens rækker og entitlement er slettet, og der oprettes intet nyt entitlement. Login afvises.
  b. Haven soft-slettes (bruger-ID som sletter, ingen e-mail), andres medlemskaber og entitlements slettes fysisk, og ejerens rækker soft-slettes.
  c. "Error: Vælg, hvad der skal ske med haven '<navn>'." eller "Error: Skriv havens navn '<navn>' for at bekræfte, at haven slettes for alle medlemmer." Intet slettes, og kontoen findes stadig.
  d. Fieldsets står i havenavn-rækkefølge, og valget gælder pr. have.
  e. Ingen fieldset. Haven bevares med den anden ejer.
- Valglisten: "Vælg…", kandidater med højeste kategori først, "Slet haven for alle medlemmer".
- Skærmbillede: nej
- Filer: Components\Account\Pages\Manage\DeletePersonalData.razor(.cs), AccountDeletionService.cs, GardenTestData.cs
### C6. Slet konto: gemte haver og tilbud om gendannelse
- Pre-requisite: Nye brugere uden 2FA og uden rolle (SeedExtraUserAsync) og haver seedet direkte i databasen (GardenTestData).
- Route: `/Account/Manage/DeletePersonalData` --> forsiden --> `/Account/Register` --> `/Account/Login` --> `/onboarding/welcome`
- Handling:
  a. Slet konto som eneste medlem af en have
  b. Slet konto og vælg "Slet haven for alle medlemmer"
  c. Slet konto, hvor haven har en anden ejer, eller hvor ejerskabet overdrages
  d. Registrér, bekræft og log ind med SAMME e-mail som den slettede bruger, og åbn `/onboarding/welcome`
  e. Gør det samme med en ANDEN e-mail
- Forventet:
  a./b. Haven er soft-slettet (bruger-ID som sletter, ingen e-mail), ejerens medlemskab og entitlement er soft-slettet. Gendannelsesbilletten har ejerens tidligere bruger-ID, SHA-256-hash af e-mailen (ingen e-mail i klartekst), slettetidspunkt nu og udløb efter 30 dage.
  c. Haven er ikke slettet og har ingen gendannelsesbillet.
  d. Banneret "Du har gemte haver, der kan gendannes: <navne>. Den nærmeste frist er <dd.MM.yyyy> (UTC)." vises med linket "Vælg abonnement og gendan" til `/onboarding/checkout`.
  e. Intet banner.
- Skærmbillede: nej
- Filer: Components\Pages\OnboardingWelcomePage.razor(.cs), AccountDeletionService.cs, GardenRestoreService.cs, GardenTestData.cs
### C7. Gendan en gemt have i checkout
- Pre-requisite: Ny bruger uden 2FA og uden rolle (SeedExtraUserAsync) som eneste medlem/ejer af en have (BedDesigner/Administrator, seedet med GardenTestData). Kontoen slettes via UI, og brugeren registrerer, bekræfter og logger ind igen med SAMME e-mail.
- Route: `/onboarding/welcome` --> "Vælg abonnement og gendan" --> `/onboarding/checkout` --> `/gardens/<id>/access`
- Handling:
  a. Indtast et havenavn, vælg BedDesigner/Administrator, klik "Fortsæt", vælg "Gendan <havenavn>", klik "Gennemfør betaling og gendan have"
  b. Vælg Planlaegger/Editor og klik "Fortsæt"; klik "Ændr abonnement"; vælg BedDesigner/Administrator og gendan
  c. Behold "Opret en ny have" og gennemfør købet
  d. Efter c: åbn checkout igen og gendan den gamle have med et dækkende køb
  e. Udløbet gendannelsesbillet (seedet direkte i databasen)
- Forventet:
  a. Landing på `/gardens/<id>/access` med "Haveadgang – <havenavn>" (havens oprindelige navn). Haven er ikke længere slettet, medlemskabet tilhører den nye bruger med det købte niveau, det gamle entitlement er erstattet af et nyt, og billetten er slettet. AuditLog har én Update-række for Garden og mindst én for GardenMembership.
  b. Efter "Fortsæt" er valget "Gendan <havenavn>" deaktiveret med "Kræver mindst … Dit nuværende valg dækker ikke haven.", og "Opret en ny have" er valgt. "Ændr abonnement" går til trin 1 med havenavnet bevaret. Med et dækkende køb lykkes gendannelsen som i a.
  c. Landing på den nye have. Den gamle have er stadig soft-slettet (medlemskab og entitlement soft-slettet), og billetten findes.
  d. Den gamle have gendannes som i a. Den nye have er uberørt.
  e. Intet banner på `/onboarding/welcome` og i checkout, og betalingstrinnet viser ikke "Gendan en gemt have?".
- Skærmbillede: nej
- Filer: Components\Pages\OnboardingCheckoutPage.razor(.cs), Components\Domain\Subscriptions\RestoreGardenChoice.razor(.cs), PaymentMockPage.razor(.cs), PricingCalculator.razor(.cs), OnboardingService.cs, GardenRestoreCoverage.cs