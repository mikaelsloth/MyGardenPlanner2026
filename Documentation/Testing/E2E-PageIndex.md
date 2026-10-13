Indeks til manual smoke tests i MyGardenPlanner2026.Tests.E2E. Kun det, der er verificeret, står her. Mangler en side, så bed om .razor-filen.

## 1. Arbejdsproces

- Ny chat pr. tjekliste. 
- Upload: denne fil, tjeklisten (med specs) og de relevante .razor-filer.
- Claude svarer med filliste og højst et par afklaringer, derefter PR'er.
- Hver PR opdaterer afsnit 5 (dækning) og de berørte sidesektioner.

## 2. Infrastruktur (i MyGardenPlanner2026.Tests.E2E)
- Fixture: PlaywrightAppFixture
- NewPageAsync()
- NewPageAsync(int width): bredde fra Viewports
- LoginAndGotoAsync(persona, path)
- RootUri
- Skærmbilleder: ScreenshotRecorder.CaptureAsync(page, kapitel, tjekliste, testnavn, komponent)
- Gemmer TestResults/Screenshots/`<kapitel>`/`<tjekliste>`/`<testnavn>`_`<komponent>`_`<bredde>`.png (fuld side).
- CI uploader mappen som artefakt e2e-screenshots.

| Viewports | (Viewports.All) |
| ------ | ------ |
| 375×812 | (Mobile) |
|768×1024 | (Tablet) |
| 939×900 | (BelowBreakpoint) |
| 940×900 | (AtBreakpoint) |
| 1280×800 | (Desktop) |

- Breakpoint: mobil = under 940 px.
- Interaktion: BlazorInteraction
- ActUntilAsync(action, reached, ...) gentager en handling, til tilstanden er nået.
- WaitUntilAsync(condition, timeout) poller en betingelse.

- Trait: [Trait(TestCategories.Category, TestCategories.ManualSmoke)] og [Collection(PlaywrightAppCollection.Name)].
- Kørsel: dotnet test `<E2E-csproj>` --filter-trait "Category=ManualSmoke".
- Ruter: E2ERoutes: 
  * Landing
  * Pricing
  * About
  * Terms
  * Privacy
  * admin-ruter
  * EnableAuthenticator
  * AccessDenied
  * Login
  * Register
  * RegisterConfirmation
  * Lockout
  * Passkeys
  * ManageEmail
  * ConfirmEmailChange
  * ChangePassword
  * PersonalData
  * DeletePersonalData

Smoke-brugere (SmokeTestPersonas, seedes af SmokeTestDataSeeder):
| Persona | Rolle | 2FA |
| ------ | ------ | ------ |
| Admin | SystemAdmin | ja |
| DataAdmin | DataAdmin | ja |
| PolicyAdmin | PolicyAdmin | ja |
| Auditor | AuditViewer | ja |
| NoMfa | SystemAdmin | nej |
| Requester | ingen | ja |
| Plain | ingen | nej |

- Login: LoginFlow.LoginAsync bruger labels "E-mail" og "Adgangskode", knappen "Log ind" og "Godkendelseskode".
- PublicNavigation.ClickRegisterAsync(page): klikker "Opret bruger" i headeren, eller åbner NavDrawer, hvis linket er skjult.
- RegisteredUserFlow (nye brugere via UI, adgangskode = SmokeTestDataSeeder.SharedPassword, e-mail e2e-`<guid>`@test.dk):
  - NewEmail()
  - RegisterAsync(page, email?): fra /Account/Register til RegisterConfirmation
  - ConfirmAsync(page): følger bekræftelseslinket
  - RegisterAndConfirmAsync(page, rootUri)
- fixture.AppConnectionString: appens forbindelse til engangsdatabasen.
- LoginFlow.FillCredentialsAsync(page, email, password), LoginFlow.LoginButton(page).
- SmokeTestDataSeeder.LockOutUserAsync(connectionString, email): låser en bruger (LockoutEnd +30 min).
- SmokePageAssertions.ExpectPageAsync(page, title, heading): titel, præcis én h1, ingen skeletons, skjult #blazor-error-ui.

- fixture.SmtpServer (TestSmtpServer): SMTP-sink i testprocessen. Messages, WaitForMessageAsync(predicate, timeout).
- PlaywrightAppFixture.SecurityAlertRecipient: eneste modtager (Smtp__AdminSecurityEmails__0) i E2E.
- App-processen får Smtp__Host=127.0.0.1, Smtp__Port=<sinkens port>, Smtp__EnableSsl=false, tomme Smtp__UserName/Password.
- MimeText: dekoder RFC 2047-headere og base64/quoted-printable (bruges af TestSmtpServer).
- SmokeTestDataSeeder.GetUserIdAsync(connectionString, email).

- Ruter: + TwoFactorAuthentication, LoginWith2fa.
- LoginFlow: LoginAsync = SubmitPasswordAsync + (hvis 2FA) SubmitTwoFactorCodeAsync.
- TwoFactorSetup (bruger skal være logget ind uden 2FA):
  - ReadSharedKeyAsync(page): læser <kbd>, giver Base32 med store bogstaver
  - VerifyAsync(page, key): indtaster TOTP, klikker "Verify", returnerer gendannelseskoderne
  - EnableAsync(page, rootUri, user): giver TwoFactorEnrollment(User med nøgle, RecoveryCodes)

- VirtualAuthenticator.AddAsync(page): WebAuthn virtual authenticator (Chromium, CDP). CountCredentialsAsync(). Skal tilføjes før passkey-handlinger.

## 3. Læringer (undgå gentagelser)
- Playwright-regex understøtter kun IgnoreCase og Multiline. Andre flag giver ArgumentException.
- Interaktive Blazor Server-sider ignorerer klik og input, til kredsløbet er forbundet, og prerendering kan nulstille indtastninger. Brug ActUntilAsync, og indtast først, når siden har reageret.
- @bind og @onchange på inputfelter udløses ved blur. Efter FillAsync kaldes BlurAsync().
- aria-expanded renderes som "True"/"False". Brug case-insensitiv sammenligning.
- GetByLabel skal bruge Exact = true, hvis et label er en delstreng af et andet.
- Beløb vises med C2. Sammenlign cifrene (uden kultur, valutategn og separatorer) og divider med 100.
- Vent på skeletons: [class*='skeleton'] skal have antal 0 før skærmbillede og assertions.
- Hver side har præcis én h1.

- Tester er hardcodede forventede værdier. De udledes ikke af produktionskoden.
- E2E-appen skal have Smtp__*-miljøvariabler til TestSmtpServer. Ellers bruger den appsettings' Smtp:Host, og 5. forkerte login giver 500 (SmtpException).
- Sikkerhedsalarmen indeholder Bruger-ID (ikke e-mail) og sendes til Smtp:AdminSecurityEmails, ikke til brugeren. Genkend en brugers alarm på Bruger-ID.

- Manage-siderne ligger i ManageLayout (h1 "Manage your account", tekster og titler på engelsk, titler uden "– MyGardenPlanner").
- Ingen log ud-knap i PublicLayout/ManageLayout: log ud i E2E = page.Context.ClearCookiesAsync().
- StatusMessage: beskeder der starter med "Error" vises som danger (role=alert), ellers success (role=status). Præfikset skal forblive engelsk.

- WebAuthn kræver et domænenavn som host: kør passkey-tests på http://localhost:`<port>` (RootUri.Replace("127.0.0.1", "localhost")), ikke 127.0.0.1.
- Login.razor starter passkey-autofill (conditional mediation) ved sideload. Passkey-tests slår den fra med AddInitScriptAsync("PublicKeyCredential.isConditionalMediationAvailable = async () => false;").
- Passkey-fejl vises som StatusMessage (role=alert) efter redirect ("Error: ...").

- Email.razor viser bekræftelseslinket til e-mailskift på siden, når EmailSender er IdentityNoOpEmailSender (samme mønster som RegisterConfirmation). Uden det kan skiftet ikke bekræftes i E2E.
- ConfirmEmailChange: gyldigt Base64Url-format med forkert token giver "Error: Der opstod en fejl ved skift af e-mail.".
- Label "Email" (deaktiveret felt) er delstreng af "New email": brug Exact = true.

- ChangePassword: succes redirecter til samme side (RedirectToCurrentPageWithStatus) med statusbeskeden "Your password has been changed". Man forbliver logget ind (RefreshSignInAsync).
- ChangePassword: labels i markup ("Old password", "New password", "Confirm password") afviger fra Display-navnene i valideringsbeskeder ("Current password", "New password", "Confirm new password").

- PersonalData: "Download" er en almindelig POST-formular (ikke enhanced) til /Account/Manage/DownloadPersonalData. Brug page.RunAndWaitForDownloadAsync og læs filen via download.PathAsync(). "Delete" er et link (<a>), ikke en knap.
- DeletePersonalData: succes kører DeleteAsync + SignOutAsync og redirecter til samme side. Efterfølgende tjek er, at login afvises. Slet-knappen vises ikke længere.

## 4. Sider

Alle er offentlige (ingen login). Titler er `<PageTitle>`.
```
/ LandingPage
Titel: MyGardenPlanner – Planlæg din drømmehave
h1 ligger i HeroBanner (tekst ikke verificeret). Fremhævede priskort vises efter skeleton.
Fil: Components/Pages/LandingPage.razor
```
```
/pricing PricingPage
Titel: Priser & Abonnement – MyGardenPlanner. h1: Priser & Abonnement.
Indeholder PricingCalculator. Øvrige sektioner (matrix, rabattabel, tilkøbskort) er ikke E2E-verificeret.
PricingCalculator-selektorer:
Container: .pricing-calculator
Selects: #calc-level, #calc-category, #calc-cycle. Option-værdier er enum-navne.
Felter: #calc-active, #calc-archived
Tilkøb: input[id^='addon-']. Label = tilkøbets navn fra DefaultSubscriptionAddOnCatalog (brug Exact = true).
Knap: "Beregn pris". Resultat: .pricing-calculator-result
Resultatkort (.summary-card med .summary-card-value): "Vægtet antal haver", "Rabat-trappe", "Have-subtotal", "Tilkøb i alt", "Total"
Regler (PricingCalculatorService):
Vægtet antal haver = aktive + arkiverede × vægt (0,25 for Administrator, ellers 1,0).
Rabatfaktor = trinnet med størst MinGardens ≤ vægtet antal (MaxGardens bruges ikke).
Have-subtotal = basispris(cyklus) × rabatfaktor × vægtet antal.
Tilkøb = pakkepris(cyklus) × antal, uden rabat og uden ganges med antal haver.
Total = Have-subtotal + Tilkøb.
Ingen afrunding. Ved 0 haver vises fejlen "Ingen matchende volumenrabat-trappe fundet."
Filer: PricingPage.razor, PricingCalculator.razor (+ .razor.cs, .razor.css)
```
```
/about AboutPage
Titel: Om platformen – MyGardenPlanner. h1: Om MyGardenPlanner.
```
```
/terms TermsPage
Titel: Handelsbetingelser – MyGardenPlanner. h1: Handelsbetingelser.
```
```
/privacy PrivacyPage
Titel: Privatlivspolitik – MyGardenPlanner. h1: Privatlivspolitik.
PublicHeader og NavDrawer (offentlig navigation)
Header: header.public-header
Hamburger: button[aria-label='Åbn menu'], klasse public-header-menu-toggle, vises under 940 px.
Desktop-links: .public-header-links, vises fra 940 px.
Drawer: nav.nav-drawer, klassen open når den er åben. Baggrund: .drawer-backdrop (kun når åben).
Drawer har 4 links, fokus-trap via JS-modul (NavDrawer.razor.js, ikke læst). Esc lukker, og fokus returnerer til hamburger.
Breakpoint-stilarter ligger i PublicHeader.razor.css.
Filer: Components/Layout/PublicHeader.razor (+ .razor.cs, .razor.css), NavDrawer.razor (+ .razor.cs)
```
```
/Account/Register
Titel: Opret bruger – MyGardenPlanner. h1: Opret bruger (AuthPageShell: h1 = Title, brandpanelet er aria-hidden og har h2).
Labels: "E-mail", "Adgangskode" (Exact = true), "Bekræft adgangskode". Knap: "Opret bruger" (role button; headerens "Opret bruger" er et link).
Succes: redirect til /Account/RegisterConfirmation?email=... (kræver RequireConfirmedAccount; ellers logges brugeren ind og sendes til ReturnUrl).
Fejl: ValidationSummary (.text-danger) og StatusMessage "Error: ...".
Filer: Register.razor (+ .cs)
```
```
/Account/RegisterConfirmation
Titel: Bekræft registrering – MyGardenPlanner. h1: Bekræft registrering.
Link "Klik her for at bekræfte din konto" vises kun med IdentityNoOpEmailSender, ellers <p role="alert">.
Filer: RegisterConfirmation.razor (+ .cs)
```
```
/Account/ConfirmEmail
Titel: Bekræft e-mail – MyGardenPlanner. h1: Bekræft e-mail.
Succes: "Tak, fordi du bekræftede din e-mail." + link "Klik her for at logge ind".
Filer: ConfirmEmail.razor (+ .cs)
```
```
PublicHeader: header.public-header har <a href="/account/login"> "Log ind" og <a href="/account/register"> "Opret bruger".
Synlighed under 940 px er ikke verificeret (PublicHeader.razor.css ikke læst).
```
```
/Account/Login
Titel: Log ind – MyGardenPlanner. h1: Log ind. Layout: PublicLayout.
Labels: "E-mail", "Adgangskode" (Exact = true), "Husk mig". Knap: "Log ind" (Exact = true; passkey-knappen hedder "Log ind med en passkey").
Fejl ved forkert login: StatusMessage "Error: Invalid login attempt." (engelsk).
Login bruger lockoutOnFailure: false. Redirect til /Account/Lockout sker kun for en allerede låst bruger.
Forkert password kalder ReAuthFailureTracker.RecordFailureAsync. Ved præcis 5 forsøg (Threshold) sendes sikkerhedsalarm (ISecurityAlertService).
5. forkerte forsøg (Threshold) sender alarm til Smtp:AdminSecurityEmails. Emne: "[MyGardenPlanner] Sikkerhedsalarm: Gentagne fejlede login-/re-auth-forsøg". Krop indeholder "Bruger-ID: <id>" og "IP-adresse: <ip>".
Alarmen sendes kun ved præcis 5 (ikke ved 6+). Transportfejl (SmtpException) fanges ikke og giver 500 på login.
Filer: Login.razor (+ .cs), ReAuthFailureTracker.cs, ReAuthFailureTrackerOptions.cs
```
```
/Account/Lockout
Titel: Konto låst – MyGardenPlanner. h1: Konto låst.
Besked: div.status-message.status-danger[role=alert] "Denne konto er blevet låst. Prøv venligst igen senere."
Fil: Lockout.razor
```
```
/Account/AccessDenied
Titel: Adgang nægtet – MyGardenPlanner. h1: Adgang nægtet (AuthPageShell, PublicLayout).
Besked: div.status-message.status-danger[role=alert] "Du har ikke adgang til denne ressource."
Bruger uden 2FA redirectes til /Account/Manage/EnableAuthenticator med statusbesked.
Filer: AccessDenied.razor (+ .cs)
```
```
/Account/LoginWith2fa
Titel: Totrinsbekræftelse – MyGardenPlanner. h1: Totrinsbekræftelse.
Label: "Godkendelseskode", checkbox "Husk denne enhed", knap "Log ind" (Exact). Link "logge ind med en gendannelseskode".
Forkert kode: "Error: Ugyldig godkendelseskode." og et fejlforsøg registreres i ReAuthFailureTracker.
Filer: LoginWith2fa.razor (+ .cs)
```
```
/Account/Manage/TwoFactorAuthentication
Titel: Two-factor authentication (2FA). Uden 2FA: link "Add authenticator app". Med 2FA: "Disable 2FA", "Reset recovery codes" (+ advarsel ved højst 3 koder tilbage).
Filer: TwoFactorAuthentication.razor (+ .cs)
```
```
/Account/Manage/EnableAuthenticator
Titel: Configure authenticator app. Nøglen vises i <kbd> (små bogstaver, grupper á 4). Label "Verification Code", knap "Verify".
Fejl: "Error: Verification code is invalid." Succes (ingen koder i forvejen): ShowRecoveryCodes på samme URL med "Your authenticator app has been verified.", h3 "Gendannelseskoder" og 10 x code.recovery-code.
Filer: EnableAuthenticator.razor (+ .cs), Shared\ShowRecoveryCodes.razor (+ .cs)
```
```
/Account/LoginWithRecoveryCode
Titel: Gendannelseskode – MyGardenPlanner. h1: Bekræft med gendannelseskode.
Label: "Gendannelseskode" (Exact), knap "Log ind". Nås fra LoginWith2fa via linket "logge ind med en gendannelseskode" (kræver password-login først).
Gyldig kode: login, redirect til forsiden. Brugt/forkert kode: "Error: Ugyldig gendannelseskode indtastet." og et fejlforsøg registreres i ReAuthFailureTracker (5 forkerte udløser sikkerhedsalarm). Ingen lockout-tælling i koden.
Filer: LoginWithRecoveryCode.razor (+ .cs)
```
```
/Account/Manage/Passkeys
Titel: Manage your passkeys (uden "– MyGardenPlanner"). ManageLayout (h1 "Manage your account"), h3 "Manage your passkeys".
Tom: "No passkeys are registered." Knap "Add a new passkey" (PasskeySubmit, Create). Liste: tabel med navn ("Unnamed passkey" uden navn) og knapperne "Rename"/"Delete".
Filer: Passkeys.razor (+ .cs), Shared\PasskeySubmit.razor (+ .razor.js)
```
```
/Account/Manage/RenamePasskey/{id}
Ingen PageTitle. h4 "Enter a name for your passkey" (uden navn). Label "Passkey name", knap "Continue".
Succes: redirect til Passkeys med "Passkey updated successfully."
Filer: RenamePasskey.razor (+ .cs)
```
```
/Account/Login: knap "Log ind med en passkey" (PasskeySubmit, Request, e-mailfeltet sendes som username). Tomt felt giver discoverable credential.
```
```
/Account/Manage/Email
Titel: Manage email (uden "– MyGardenPlanner"). ManageLayout (h1 "Manage your account"), h3 "Manage email". Static SSR.
Labels: "New email" (Exact = true, forudfyldt med nuværende e-mail), deaktiveret felt "Email". Knap: "Change email".
Besked: "Confirmation link to change email sent. Please check your email." (success), "Your email is unchanged." ved uændret e-mail.
Link "Click here to confirm your new email" vises kun med IdentityNoOpEmailSender.
Ugyldig e-mail: ValidationMessage (.validation-message).
Filer: Email.razor (+ .cs)
```
```
/Account/ConfirmEmailChange?userId&email&code
Titel: Bekræft e-mailskift – MyGardenPlanner. h1: Bekræft e-mailskift (AuthPageShell).
Succes: "Tak, fordi du bekræftede skiftet af din e-mail." (e-mail og brugernavn skiftes, brugeren logges ind igen).
Fejl: "Error: Der opstod en fejl ved skift af e-mail." (ugyldig token). Mangler parametre: redirect til Login med "Error: Ugyldigt link til bekræftelse af e-mailskift.".
Filer: ConfirmEmailChange.razor (+ .cs)
```
```
/Account/Manage/ChangePassword
Titel: Change password (uden "– MyGardenPlanner"). ManageLayout (h1 "Manage your account"), h3 "Change password". Static SSR.
Labels (Exact = true): "Old password", "New password", "Confirm password". Knap: "Update password".
Succes: redirect til samme side, besked "Your password has been changed".
Forkert nuværende password: "Error: Incorrect password." (Identity-standardtekst).
Valideringsfejl (.validation-message): "The New password must be at least 6 and at max 100 characters long." og "The new password and confirmation password do not match.".
Brugere uden lokalt password redirectes til Account/Manage/SetPassword.
Filer: ChangePassword.razor (+ .cs)
```
```
/Account/Manage/PersonalData
Titel: Personal Data (uden "– MyGardenPlanner"). ManageLayout (h1 "Manage your account"), h3 "Personal Data". Static SSR.
Knap "Download" (POST til /Account/Manage/DownloadPersonalData, kræver login). Link "Delete" til /Account/Manage/DeletePersonalData.
Filen PersonalData.json: nøgler fra [PersonalData]-felterne (fx "Email") samt altid "Authenticator Key" (null uden authenticator).
Filer: PersonalData.razor (+ .cs), IdentityComponentsEndpointRouteBuilderExtensions.cs
```
```
/Account/Manage/DeletePersonalData
Titel: Delete Personal Data (uden "– MyGardenPlanner"). ManageLayout (h1 "Manage your account"), h3 "Delete Personal Data".
Felt "Password" (kun hvis brugeren har password), knap "Delete data and close my account".
Forkert password: "Error: Incorrect password.". Succes: konto slettes, bruger logges ud, redirect til samme side.
Filer: DeletePersonalData.razor (+ .cs)
```
```
/admin/subscriptions (Admin) · /admin/security-policies (PolicyAdmin) · /admin/audit-log (Auditor) · /admin/jit-requests (DataAdmin)
Rolle- og 2FA-beskyttet (RequireGlobalAdmin / RequirePolicyAdmin / RequireAuditViewer / RequireAnyAdminRole + MfaRequirement). Siden kræver ikke "recent auth".
Step-up: StepUpGuard.RunAsync omkring følsomme handlinger → StepUpReAuthModal (role=dialog, "Bekræft din identitet").
Modal vises ved første handling i en frisk circuit (ReAuthenticationService er Scoped; login markerer kun request-scope). MaxAgeMinutes default 15.
Selektorer: #step-up-password, #step-up-totp (kun 2FA), knapper "Bekræft"/"Annullér", fejl .status-message.status-danger[role=alert].
Tests: ManualSmoke/TjeklisteD/ModalVisesTests (StepUpModal-hjælperen).
```
```
/admin/security-policies — selektorer
Faner: role=tab; valgt fane har klassen "active" (aria-selected er en bool-attribut: true = tom attribut, false = udeladt).
Fanen AuditLog-eksport: #exportjob-retention, #exportjob-maxactive (type=number), knap "Gem" (step-up via StepUpGuard efter AdminActionGuard).
Succes: .status-success[role=status] på siden: "AuditLog-eksportjob-policyen er opdateret og trådt i kraft." (ryddes ved fanskift).
Fejl i editor: .status-danger[role=alert] (fx rate limit, ArgumentOutOfRange).
Test-infrastruktur: fixture.SeedExtraUserAsync(role) giver en isoleret admin pr. test (egen fejltæller).
Tests: ManualSmoke/TjeklisteD/ForkertPasswordModalTests.
```
```
Step-up: E2E-appen kører med ReAuthenticationPolicy__MaxAgeMinutes=1 (appsettings: 15). Modal vises ved første handling i hver circuit; efter step-up gælder vinduet i samme circuit; reload = ny circuit = ny modal.
Fanehop (SelectTab) rydder statusMessage og genindlæser editoren.
Page-objects: SecurityPolicyPage (faner, #exportjob-retention, Gem, .status-success), StepUpModal (Dialog, felter, CompleteAsync).
Tests: ManualSmoke/TjeklisteD/KorrektStepUpTests.
```
```
/admin/jit-requests — selektorer
Titel: JIT-adgang – MyGardenPlanner. h1: JIT-adgang. Policy: RequireAnyAdminRole + MfaRequirement (direkte admin-rolle eller aktiv JIT). Brugere uden admin-rolle (Requester, Plain) kan ikke åbne siden.
Faner: role=tab, valgt fane har klassen "active". "Anmod om adgang" (standard) og "Godkend anmodninger". Fanehop rydder statusbeskeden og genindlæser fanens komponent.
Statusbesked ved godkend/afvis: .status-success[role=status] på siden.
Anmod-fanen (JitElevationRequestForm):
  #jit-request-role (select, værdier = rollenavne, standard SystemAdmin), #jit-request-minutes (number, standard 30, @onchange), #jit-request-reason (@bind).
  Knap "Send anmodning": ikke idempotent, ingen succesbesked; årsagsfeltet ryddes ved succes.
  Checkbox #jit-request-show-history ("Vis afviste/udløbne"): standard fra, så kun Afventer/Godkendt vises. Nulstilles ved fanehop.
  Tabel "Mine anmodninger": Rolle, Minutter, Årsag, Status, Oprettet. Status-badges: "Afventer", "Godkendt", "Afvist", "Udløbet".
  Fejl: .status-danger[role=alert] "Error: <ex.Message>". Uden for grænserne: "RequestedMinutes skal være mellem {min} og {max}." efterfulgt af "(Parameter 'minutes')" og "Actual value was N.".
  Rate limit: "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen." Ingen step-up ved anmodning.
Godkend-fanen (JitElevationApprovalQueue):
  h3 "Anmodninger til godkendelse". Tabel: Ansøger (bruger-ID, ikke e-mail), Rolle, Minutter, Årsag, Anmodet, Handling.
  Knapper pr. række: "Godkend" og "Afvis" (AdminActionGuard → StepUpGuard → StepUpReAuthModal).
  Tom kø: "Ingen ventende anmodninger, du kan godkende."
  Beskeder: "Anmodning om '<rolle>' er godkendt." / "Anmodning om '<rolle>' er afvist." Fejl: "Error: ...".
Kø-regler (JitElevationService.GetPendingRequestsForApprovalAsync): kun Pending, ikke egne, og kun roller godkenderen er direkte medlem af (aktiv JIT tæller ikke).
Køen indlæses ved fanevalg og opdateres ikke automatisk. Komponenten renderer først "Ingen ventende anmodninger …" og derefter data: negative asserts kan bestå for tidligt. Brug en positiv kontrol (en anden række) før en count == 0-assert.
Adgang efter godkendelse: HasActiveElevationAsync læser DB ved hvert kald (ingen cache). Ny sideindlæsning er nok.
Filer: AdminJitRequestsPage.razor (+ .cs), JitElevationRequestForm.razor (+ .cs), JitElevationApprovalQueue.razor (+ .cs), IJitElevationService.cs, JitElevationService.cs, JitElevationPolicyOptions.cs, AnyAdminRoleAuthorizationHandler.cs, JitRoleAuthorizationHandler.cs, AuthorizationServicesExtensions.cs
```
```
/admin/security-policies — fanen JIT-eskalering
Faner i rækkefølge: JIT-eskalering (aktiv ved indlæsning), Step-up re-auth, Fejl-tracker, Admin-API rate limit, Login rate limit, AuditLog-eksport.
Felter (JitElevationPolicyEditor): #jit-min, #jit-max, #jit-sweep (type=number, @onchange → blur). Knap "Gem" (AdminActionGuard → StepUpGuard).
Succes: "JIT-eskaleringspolicyen er opdateret og trådt i kraft." Fejl: .status-danger[role=alert] (ArgumentOutOfRange). Skeleton vises, til policyen er indlæst.
Standard (JitElevationPolicy): Min 30, Max 90, Sweep 5 minutter.
Sweep (RoleElevationExpirySweepService): kun Approved med ValidToUtc < nu → Expired. Pending og Rejected røres aldrig. Interval er mindst 1 minut. En ændret policy afbryder ventetiden, og sweep kører med det samme.
Adgang afgøres uafhængigt af Status: HasActiveElevationAsync tjekker ValidToUtc direkte.
Filer: AdminSecurityPolicyPage.razor (+ .cs), JitElevationPolicyEditor.razor (+ .cs), RoleElevationExpirySweepService.cs
```

Admin-sider (kendt fra PersonaAccessTests, ikke gennemgået med .razor)
| Rute	Overskrift	Persona |
| ------ | ------ | ------ |
| /admin/subscriptions | Administrer abonnementer | Admin |
| /admin/jit-requests | JIT-adgang | DataAdmin |
| /admin/security-policies | Sikkerhedspolicies | PolicyAdmin |
| /admin/audit-log | AuditLog | Auditor |

- Uden 2FA redirectes til /Account/Manage/EnableAuthenticator.
- Uden admin-rolle redirectes til /Account/AccessDenied med teksten "Du har ikke adgang til denne ressource."

Ikke indekseret endnu: 
- Alle sider med login (undtagen admin-overskrifterne ovenfor).
- Login-, registrerings- og kontosider.
- Komponenter ud over dem nævnt her.

## 5. Dækning

| Tjekliste | Punkt | Testklasse | Skærmbilleder | Status |
| ------ | ------ | ------ | ------ | ------ |
| A1 | Offentlige sider loader | OffentligeSiderLoadTests | Loadside_`<Side>`_`<bredde>` (25) | grøn |
| A2 | Pricing-calculator vs. Prismatrix | PricingCalculatorTests | Prisberegning_PricingCalculator_`<bredde>` (3) | grøn |
| A3 | Mobilmenu og fokus-trap | MobilmenuTests | ingen | grøn |
| B1 | Opret bruger | OpretBrugerTests | Oprettelse_Register_`<bredde>` (5), Oprettelse_RegisterConfirmation_`<bredde>` (5) | ikke kørt |
| B2 | Forkert login og Lockout | ForkertLoginTests | ForkertLogin_Lockout_`<bredde>` (3) | ikke kørt |
| B3 | 2FA | TofaktorTests | 2FA_EnableAuthenticator, 2FA_RecoveryCodes, 2FA_LoginWith2fa (`<bredde>`) (9) | ikke kørt |
| B4 | Recovery code | GendannelseskodeTests | Gendannelseskode_LoginWithRecoveryCode, Gendannelseskode_LoginWithRecoveryCodeFejl (`<bredde>`) (6) | ikke kørt |
| B5 | Passkey | PasskeyTests | Passkey_Passkeys, Passkey_RenamePasskey, Passkey_Login (`<bredde>`) (9) | ikke kørt |
| C1 | Skift e-mail | SkiftEmailTests | SkiftEmail_Email_`<bredde>` (5), SkiftEmail_EmailEfterSkift_`<bredde>` (5) | ikke kørt |
| C2 | Skift password | SkiftPasswordTests | SkiftPassword_ChangePassword_`<bredde>` (5), SkiftPassword_ChangePasswordEfterSkift_`<bredde>` (5) | ikke kørt |
| C3 | Download/slet personlige data | PersonligeDataTests | PersonligeData_PersonalData_`<bredde>` (5), PersonligeData_DeletePersonalData_`<bredde>` (5) | ikke kørt |

Tests ligger under ManualSmoke/TjeklisteA/.
Tests ligger under ManualSmoke/TjeklisteB/.
Tests ligger under ManualSmoke/TjeklisteC/.

## 6. Åbne observationer (ikke rettet)
Lukket NavDrawer er kun skubbet ud af skærmen med transform, så linkene i den kan nås med Tab.
aria-expanded renderes med stort begyndelsesbogstav.
Rabatopslaget bruger kun MinGardens.
Register.razor.cs: valideringsbeskeder (StringLength, Compare) og Display-navne er på engelsk.
Login.razor.cs: fejlbeskeden "Error: Invalid login attempt." er på engelsk.
Login.razor.cs: IsLockedOut-grenen kalder ikke TrackReAuthOutcomeAsync (relevant, når lockout-featuren bygges).
SmtpSecurityEmailSender: doc-kommentaren siger "fejler blødt", men det gælder kun manglende konfiguration. Transportfejl (SmtpException) kastes videre og vælter login-flowet ved 5. forkerte forsøg.
Manage-siderne (TwoFactorAuthentication, EnableAuthenticator) og StatusMessage-teksterne er på engelsk. Login-siderne er på dansk.
RenamePasskey.razor har ingen PageTitle.
Passkeys- og RenamePasskey-siderne og deres status-tekster er på engelsk.
Email.razor: med IdentityNoOpEmailSender vises bekræftelseslinket til enhver ny e-mailadresse uden ejerskabsbevis (forsvinder, når en rigtig IEmailSender indsættes).
Email.razor: OnSendEmailVerificationAsync viser ikke linket (kun relevant for ubekræftede e-mails, ikke dækket).
ConfirmEmailChange.razor.cs: WebEncoders.Base64UrlDecode(Code) er uden try/catch. Et ikke-Base64Url-format i code kaster formentlig FormatException (500). Ikke testet.
ChangePassword: fejlbeskeder fra Identity-password-politikken (fx "Error: Passwords must have at least one digit") er engelske og ikke dækket af test, da politikken ikke er i konteksten.
DeletePersonalData: hvis UserManager.DeleteAsync fejler, kastes InvalidOperationException (500) uden UI-fejl. Sletning af brugere med relationer (haver, medlemskaber) er ikke undersøgt eller dækket (ny bruger har ingen).
DownloadPersonalData: (await GetAuthenticatorKeyAsync(user))! giver null i JSON for brugere uden authenticator (ingen fejl, men null-forgiving er unøjagtig).
Efter sletning er destinationen for en ikke-logget-ind bruger (login eller NotAuthorized) ikke verificeret.
AccessDenied.razor: indlejret AuthPageShell (to h1) rettet i PR "Fix: AccessDenied renderer AuthPageShell to gange".
Login/LoginWith2fa/LoginWithRecoveryCode kalder MarkReAuthenticated i request-scope (Scoped service) → ingen effekt på circuit; modalen vises altid ved første beskyttede handling. Modaltekst "sidst logget ind" er misvisende.
HttpContextCurrentUserAccessor: HttpContext er normalt null i en circuit → alarm fra StepUpReAuthModal får IP "ukendt".
AuthorizationServicesExtensions: RequireGlobalAdminPolicy registreres to gange (første overskrives).
ReAuthFailureTracker: alarm sendes kun ved præcis Threshold (==); ved 6+ fejl inden for vinduet sendes ingen ny alarm før tælleren nulstilles (succes i login/step-up). Fejl tælles samlet på tværs af login og step-up.
JitElevationService.RequestElevationAsync: tjekker kun minutter inden for policyen og at rollen findes. Ansøgerens berettigelse til rollen tjekkes ikke, og en bruger kan anmode om en rolle, vedkommende allerede har (backlog).
Brugere uden admin-rolle kan ikke åbne /admin/jit-requests (RequireAnyAdminRole), så de kan ikke anmode via UI.
JitElevationService.ApproveElevationAsync kalder securityAlertService.AlertJitRequestedAsync ved godkendelse, ikke ved anmodning. Mailens indhold er ikke verificeret.
JitElevationApprovalQueue viser ansøgerens bruger-ID (GUID), ikke e-mail.
JitElevationRequestForm: "Send anmodning" giver ingen succesbesked, og fejlbeskeden indeholder .NET-teksterne "(Parameter 'minutes')" og "Actual value was N." (engelsk).
JitElevationApprovalQueue: kun InvalidOperationException fanges ved godkend/afvis; andre fejl vælter circuiten.
Sweep rører kun Approved. Pending anmodninger udløber aldrig og forbliver i køen.
