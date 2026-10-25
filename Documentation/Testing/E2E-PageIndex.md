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
- LoginAndGotoAsync(persona, path, width?): login som persona og åbn siden (valgfri viewport-bredde)
- LoginAndGotoAsync(SmokeTestUser, path, width): samme for en vilkårlig seedet bruger
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
  * TwoFactorAuthentication
  * LoginWith2fa.

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
- fixture.SeedExtraUserAsync(role) → SmokeTestDataSeeder.SeedExtraUserAsync(connectionString, role): ny bruger (e2e-`<guid>`@test.dk) med rollen og 2FA. Rollen skal findes (oprettes af SeedAsync). Giver en test egen fejltæller/session uden at røre personas.
- PlaywrightAppFixture.SecurityAlertSubject: emnet på sikkerhedsalarmen (delt med B2-testen, som har en privat kopi).
- PlaywrightAppFixture.ReAuthMaxAgeMinutes = 1: sendes som ReAuthenticationPolicy__MaxAgeMinutes til E2E-appen (appsettings: 15).
- StepUpModal (page-object for role=dialog "Bekræft din identitet"):
  - Dialog, PasswordField, TotpField (kun med 2FA), ConfirmButton ("Bekræft"), CancelButton ("Annullér")
  - Error (.status-message.status-danger)
  - OpenAsync(page, trigger): gentager triggeren, til modalen er vist
  - CompleteAsync(page, user): korrekt password + TOTP, slutter når modalen er lukket
- SecurityPolicyPage (/admin/security-policies):
  - Retention (#exportjob-retention), SaveButton ("Gem"), Success (.status-success), ExportJobSavedText
  - SelectTabAsync(page, name), OpenExportJobTabAsync(page)
  - ReadRetentionAsync(page), SetRetentionAsync(page, value)
  - SecurityAlerts.CountPolicyChangedMails(mails, userId, policyName): antal "Sikkerhedspolicy ændret"-mails for brugeren og policyen (bruges til at bekræfte hvert gem, når UI ikke har unik besked).

 - TabNavigation.SelectAsync(page, name): vælger role=tab og gentager klikket, til fanen er "active"
 - AmountParser.Parse(text): beløb ud fra cifrene i en vist pris (C2)
 - AdminSubscriptionsPage (/admin/subscriptions, fanen Basispriser):
   - OpenAsync(fixture, bruger, bredde?): login, åbn, vent til interaktiv (fanehop; 12 rækker)
   - PriceInput/SaveButton/SavedMessage(page, lag, kategori), ReadPriceAsync, SetPriceAsync
   - SaveTierAsync(page, bruger, lag, kategori): Gem, step-up hvis modalen vises, afventer besked
 - PricingMatrix (/pricing, matrixen):
   - OpenAnonymousAsync(fixture, bredde), SelectCycleAsync(page, cyklus), ExpectPriceAsync(page, lag, kategori, beløb)

- AmountParser.ParseNumber(text): heltal, brøk og procent (fx "85 %")
- AdminSaveFlow.SaveWithStepUpAsync(page, bruger, gemKnap, succesbesked, fejl, beskrivelse): Gem, step-up hvis modalen vises, afventer unik succesbesked
- AdminVolumeDiscountsPage (/admin/subscriptions, fanen Volumen-rabatter):
    - OpenAsync(fixture, bruger, bredde?): login, åbn, vælg fane, vent på 7 rækker
    - Rækker findes via indeks (ens labels), sorteret efter MinGardens
    - ReadMin/ReadMax/ReadMultiplierAsync, SetMultiplierAsync, SaveTierAsync(page, bruger, indeks, minGardens)
- PricingCalculatorPage (/pricing):
    - WaitUntilInteractiveAsync(page), CalculateAsync(page, lag, kategori, cyklus, aktive, arkiverede), ExpectResultAsync(page, vægtet, rabat%, subtotal, tilkøb, total)

- PriceAssertions.ExpectValueAsync(locator, forventet, parse, beskrivelse) / ExpectAmountAsync(locator, forventet, beskrivelse): poller en vist værdi (5 sek.)
- AdminSaveFlow.RejectWithStepUpAsync(page, bruger, knap, fejl, forventetTekst): klik, step-up hvis modalen vises, afventer forventet fejltekst

- AuditLogTestData (seeder admin.AuditLogs via app-forbindelsen, kun INSERT/SELECT; rækker kan ikke ryddes, brug unikke markører):
  - NewEntityName(tag), NewUserEmail(), Row(...), SeedAsync(connectionString, rows)
  - SeedBulkAsync(connectionString, BulkSeed(entitet, e-mail, antal, handling, første tidspunkt, trin i sek.)): set-baseret INSERT … SELECT, EntityId 1..antal
  - CountAsync(connectionString, entitet?, e-mail?)
- AuditLogPage (/admin/audit-log): OpenAsync(fixture, bruger, bredde?) (venter på interaktivitet via første rækkes "Detaljer"; kræver mindst én række), ReloadAsync
  - Filter: SelectEntityAsync/SelectActionAsync/SelectPageSizeAsync, SetTextAsync, SetDateAsync, SearchAsync
  - Resultater: Rows/RowsFor(page, entitet)/Cell(række, kolonne), EmptyState, PaginationInfo, Forrige/Næste
  - Modal: DetailDialog, JsonBlock(page, 0|1), CloseButton, DetailBackdrop
  - Eksport: ExportButton, BackgroundExportButton, ThresholdConfirm(+Button/Cancel), ExportError, ExportSuccess; JobRows
- AuditLogExport: window.open opsnappes (init-script, installeres af AuditLogPage.OpenAsync). WaitForNewUrlAsync(page, kendtAntal), FetchAsync(page, url) → ExportResponse(Status, ContentType, ContentDisposition, Body, Text, FileName), ParseCsv, JsonRowCount, XlsxRowCount, IsZip. Verificerer ikke selve browser-downloadet.
- AdminAddOnsPage (/admin/subscriptions, fanen Tilkøbsmoduler):
    - OpenAsync(fixture, bruger, bredde?): login, åbn, vælg fane, vent på første række (antal kontrolleres ikke)
    - Rækker findes via type (første kolonne): Row/NameInput/PriceInput/SaveButton(page, type)
    - ReadNameAsync, ReadPriceAsync, SetPriceAsync, SaveAddOnAsync(page, bruger, type, navn)
    - AddDuplicateAsync(page, bruger, type, navn): forventer afvisning; DeleteLastRowAsync(page, bruger)

- PricingAddOns (/pricing, tilkøbskort): Cards(page), Card/Price(page, navn, cyklus), ExpectPriceAsync(page, navn, cyklus, beløb)
- PricingVolumeDiscounts.PercentCell/ExpectPercentAsync(page, rækkenavn, procent): rabattabellen på /pricing (række fx "2 - 5 haver")
   
- LoginFlow: LoginAsync = SubmitPasswordAsync + (hvis 2FA) SubmitTwoFactorCodeAsync.
- TwoFactorSetup (bruger skal være logget ind uden 2FA):
  - ReadSharedKeyAsync(page): læser <kbd>, giver Base32 med store bogstaver
  - VerifyAsync(page, key): indtaster TOTP, klikker "Verify", returnerer gendannelseskoderne
  - EnableAsync(page, rootUri, user): giver TwoFactorEnrollment(User med nøgle, RecoveryCodes)
- LoginFlow.AttemptStatusAsync(page, rootUri, user): ryd cookies, åbn login, send formularen (ingen 2FA) og returnér HTTP-statussen på POST'en. Til tests, hvor svaret kan være 429.
 
- VirtualAuthenticator.AddAsync(page): WebAuthn virtual authenticator (Chromium, CDP). CountCredentialsAsync(). Skal tilføjes før passkey-handlinger.

- AdminAccessMatrix: hardcodet matrix persona → tilladte admin-sider (Admin: Abonnementer + JIT-adgang; DataAdmin: JIT-adgang; PolicyAdmin: Sikkerhedspolicies + JIT-adgang; Auditor: JIT-adgang + AuditLog).
  - AdminPage(Route, Title, Heading, LinkText), AllPages, For(persona), DeniedPages(persona) (komplementet), Personas.
- AdminNavigation (page-object for nav.admin-nav-menu):
  - Nav(page), Links(page), Link(page, side) (href-suffiks, versalfølsomt ikke)
  - ExpectExactLinksAsync(page, forventede): positiv kontrol først, derefter fravær og samlet antal
- LoginRedirect.ReturnPath(url): stien i ReturnUrl fra en login-URL (relativ "ReturnUrl" fra middleware og absolut "returnUrl" fra RedirectToLogin)

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


- Step-up: ReAuthenticationService er Scoped. Login markerer kun re-auth i request-scope, så en frisk circuit har intet tidsstempel, og StepUpReAuthModal vises ved første beskyttede handling uden ventetid. Efter step-up gælder vinduet i samme circuit; reload = ny circuit = ny modal.
- Kun udløb i samme circuit kræver ventetid: E2E-appen kører med MaxAgeMinutes=1, og testen venter ca. 62 sek.
- Fane-knapper med aria-selected="@bool": true renderes som en tom attribut, false udelades. Brug klassen "active" som valgt-signal i stedet for aria-selected.
- Fanehop (SelectTab) rydder statusbeskeden og genindlæser editoren: brug det til at bevise, at en ny besked stammer fra en ny gem.
- Fejltæller (ReAuthFailureTracker): alarm sendes kun ved præcis Threshold (5). Fejl tælles samlet pr. bruger på tværs af login og step-up (vindue 2 dage). Brug en ny bruger pr. test (SeedExtraUserAsync).
- Modal: forkert password og forkert TOTP (med korrekt password) tælles. Tom TOTP tælles ikke. Skift mellem de to fejltyper, når flere forsøg skal afventes, så den nye fejltekst adskiller sig fra den forrige.
- Alarm fra en circuit: HttpContext er normalt null, så IP-adressen i mailen er sandsynligvis "ukendt". Assertér kun "IP-adresse:", ikke værdien.
- Admin-editors deler data på tværs af tests: gendan ændrede værdier (retention ændres ±1 time) og brug ikke basispriser (påvirker A2-beregningerne).
- Admin-sider kræver 2FA: NoMfa redirectes til EnableAuthenticator og når aldrig modalen.
- /Account/AccessDenied har præcis én h1 (rettet i PR0). Tjek den med SmokePageAssertions.ExpectPageAsync.

- Admin-API-limiter (AdminActionRateLimiter): sliding window pr. bruger (singleton, 60 sek., 6 segmenter), genopbygges kun ved ændring af AdminApiRateLimitOptions (ny kvote). Forbruges efter step-up og før service-validering, så et afvist gem giver rate-limit-teksten uanset værdi. Et afvist forsøg bruger ingen permit.
- Policy-editors har ingen unik succesbesked pr. gem og ingen disabled-knap: bekræft hvert gem via alarm-mailen "Policy: <navn>". Afvent mailen før næste indtastning (editoren genindlæser efter gem).
- En blokeret bruger kan ikke gendanne selv: åbn en ny PolicyAdmin FØR ændringen og brug den til gendannelse.
- Login-limiter (ReloadableLoginRateLimiter): fixed window pr. IP, kun beskyttede auth-POST'er, genopbygges (tæller nulstilles) ved policyændring. 429 uden body (OnRejected logger kun).

- AdminNavMenu ligger i AdminLayout og er statisk (ingen rendermode på layoutet). Efter en ændring i adgang (fx godkendt JIT) kræves en ny sideindlæsning, før linkene ændres.
- Nav-assertions: positiv kontrol (eget link synligt) før fravær/antal, ellers kan en negativ assert bestå, før nav'en er renderet.
- AuditLog-linket kan indeholde .nav-badge (antal nye eksportfiler): find links via href-suffiks, ikke via tekst.

- AuditLog-tests deler engangsdatabasen og rækkerne kan ikke ryddes (DENY DELETE): brug unikke EntityName/UserEmail/EntityId-præfikser. Resultater verificeres ved at sammenligne hele Entitets-ID-listen, ikke kun antal, da tabellen opdateres asynkront og et gammelt resultat ellers kan bestå.
- Visningspræferencen gemmes uden UI-signal efter søgning: vent på rækken i dbo.AuditLogViewerPreferences, før siden genindlæses.

- Komponenter, der kun synkroniserer fra en parameter ved første OnParametersSet, må ikke renderes, før forælderen har indlæst parameterens endelige værdi (Blazor renderer ved første await i OnInitializedAsync).

- ToHaveTextAsync/ToContainTextAsync normaliserer whitespace. Brug TextContentAsync (linjeskift normaliseret) til at verificere indrykning og flerlinjet tekst.
- Negativ assert på en Blazor-lukning (fx "klik inde i dialogen lukker ikke"): vent med WaitUntilAsync til en lukning ville være sket (fx 1 sek.) og assertér, at den ikke skete. En umiddelbar visibility-assert kan bestå, før hændelsen er nået til serveren.

- Modaler skal kunne scrolle på lave viewports: en Playwright-fejl "element is outside of the viewport" efter scroll betyder, at elementet ikke kan nås for en rigtig bruger. Test at lukkeknappen kan klikkes ved 375×812 og 939×900. 

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
Indeholder PricingCalculator. Matrixen er E2E-verificeret (PricingMatrix). Rabattabel og tilkøbskort er ikke.
Rabattabellen (.garden-volume-discount-table; th[scope=row] = ‘<min> - <max> haver’ / ‘1 have’ / ‘<min>+ haver’, td = ‘<procent> % af basispris’) læser databasen via IGardenVolumeDiscountService.
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
Login rate limit: fixed window pr. IP, kun beskyttede auth-POST'er (AdminAuthPathMatcher). Ved overskridelse svarer serveren 429 uden body (OnRejected logger kun). Limiteren genopbygges ved policyændring, så tælleren nulstilles straks.
Filer: ReloadableLoginRateLimiter.cs, RateLimitingServicesExtensions.cs, Login.razor (+ .cs), ReAuthFailureTracker.cs, ReAuthFailureTrackerOptions.cs
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
Volumen-rabatter: .volume-discount-editor, 7 rækker i tbody; inputs med id min-/max-/mult-<id> (labels er ens på alle rækker); "Gem" og "Slet" pr. række; "Tilføj trappe" (#add-tier); "Nulstil til standardkatalog" (+ "Ja, nulstil"). Succesbesked: "Volumenrabat-trappe fra <n> haver er opdateret."
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
Page-objects: SecurityPolicyPage (faner, #exportjob-retention, Gem, .status-success), StepUpModal (Dialog, felter, CompleteAsync), JitRequestsPage (se /admin/jit-requests).
Fanen Step-up re-auth: #reauth-maxage (type=number, min=1, @onchange → blur), knap "Gem". Fejl vises øverst i editoren (.security-policy-editor .status-danger[role=alert]); StepUpReAuthModal ligger i editoren. Succes: "Step-up re-auth-policyen er opdateret og trådt i kraft." (page-level, bliver stående mellem gem).
Fanen Admin-API rate limit: #adminapi-permit, #adminapi-window, #adminapi-segments.
Rate-limit-fejl: "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen." (AdminActionGuard.RateLimitedMessage).
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
Page-object JitRequestsPage: OpenAsync(fixture, bruger|persona, bredde?) logger ind, åbner siden og venter, til den er interaktiv (fanehop til Godkend og tilbage; Godkend-fanens overskrift findes ikke i prerenderet HTML). Slutter på "Anmod om adgang".
SubmitRequestAsync klikker "Send anmodning" én gang (ikke idempotent). RequestAndWaitAsync venter på rækken. RequestRow/QueueRow(page, årsag), StatusBadge(række), Error(page), ShowHistoryAsync(page). Unik årsag: NewReason().
GotoAsync(fixture, page): åbner siden i en allerede logget ind side. ApproveAsync/RejectAsync(page, godkender, årsag, rolle): fanehop til Godkend, venter på rækken, klikker (gentages til step-up modalen vises), StepUpModal.CompleteAsync, afventer beskeden og at rækken er væk. Kræver en frisk circuit (ingen re-auth i forvejen).
AccessAssertions.ExpectAccessDeniedAsync(page): URL indeholder /Account/AccessDenied, titel/h1 "Adgang nægtet", teksten "Du har ikke adgang til denne ressource.".
Tests: ManualSmoke/TjeklisteE/.
RefreshRequestsAsync(page, showHistory?): genindlæser "Mine anmodninger" via fanehop Godkend → Anmod (ingen sideindlæsning). "Vis afviste/udløbne" nulstilles ved hop og krydses af igen med showHistory.
```
```
AdminNavMenu / AdminLayout (alle admin-sider)
Layout: AdminLayout (_Imports i Pages/Admin) → MainLayout. Ingen h1 i layoutet (hver side har sin egen).
Navigation: nav.admin-nav-menu[aria-label='Administration'], ét <li><a> pr. tilladt link, href relativ ("admin/subscriptions"). Aktuel side har klassen "active".
Links og policy: Abonnementer (RequireGlobalAdmin = SystemAdmin), Sikkerhedspolicies (RequirePolicyAdmin), JIT-adgang (RequireAnyAdminRole: alle fire roller), AuditLog (RequireAuditViewer). Direkte rolle eller aktiv JIT. Alle kræver MFA.
AuditLog-linket får .nav-badge[aria-label] med antal nye eksportfiler, når der er ulæste.
Adgang pr. persona (AdminAccessMatrix): Admin: Abonnementer, JIT-adgang. DataAdmin: JIT-adgang. PolicyAdmin: Sikkerhedspolicies, JIT-adgang. Auditor: JIT-adgang, AuditLog.
Filer: AdminLayout.razor (+ .css), AdminNavMenu.razor (+ .cs, .css), AuthorizationServicesExtensions.cs, AnyAdminRoleAuthorizationHandler.cs, JitRoleAuthorizationHandler.cs
Tests: ManualSmoke/TjeklisteI/AdminNavigationTests
```
```
Direkte URL til admin-side uden rolle
Anonym: redirect til /Account/Login med ReturnUrl til den forespurgte side. Auth-middleware sender relativ "ReturnUrl"; RedirectToLogin (NotAuthorized, interaktiv) sender absolut "returnUrl". Der er ingen eksplicit LoginPath/AccessDeniedPath (cookie-standarder).
Autentificeret uden rolle (2FA aktiv): /Account/AccessDenied. Uden 2FA: redirect til EnableAuthenticator.
Sidens h1 og AdminNavMenu vises ikke på login- eller AccessDenied-siden.
Filer: Routes.razor (+ .cs), RedirectToLogin.razor(.cs), RedirectToAccessDenied.razor(.cs), AccessDenied.razor (+ .cs)
Tests: ManualSmoke/TjeklisteI/AdminDirektUrlTests
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
Page-object SecurityPolicyPage (JIT): OpenJitTabAsync (fanehop væk og tilbage = forbundet kredsløb; venter på #jit-min), ReadJitPolicyAsync → JitPolicyValues(Min, Max, Sweep), SaveJitPolicyAsync(page, bruger, min, sweep) (sætter Min og Sweep, Gem, step-up, afventer succesbesked).
```
```
/admin/audit-log — detaljemodal (AuditLogDetailModal)
Åbnes med knappen "Detaljer" på en række. Backdrop: .confirm-dialog-backdrop (klik lukker; dialogen stopper bobling). Dialog: .audit-log-detail-dialog, role=dialog, aria-labelledby → h2 "Detaljer — <entitet> (<id>)".
Metalinje: p.meta "<tidspunkt g da-DK, lokal tid> · <e-mail> (<bruger-id>) · <ip>".
To kolonner (.audit-log-detail-column) med h3 "Tidligere værdier" / "Nye værdier" og pre.audit-log-detail-json. Side om side fra 940 px, under hinanden under (max-width 939.98px).
Visning: gyldig JSON indrykket (2 mellemrum); null, tom eller whitespace → "(ingen data)"; ugyldig JSON → rå tekst. Knap "Luk" lukker.
Tests: ManualSmoke/TjeklisteJ/AuditLogDetailTests.
```

Admin-sider (kendt fra PersonaAccessTests, ikke gennemgået med .razor)
| Rute	Overskrift	Persona |
| ------ | ------ | ------ |
| /admin/subscriptions | Administrer abonnementer | Admin |
| /admin/jit-requests | JIT-adgang | DataAdmin |
| /admin/security-policies | Sikkerhedspolicies | PolicyAdmin |
| /admin/audit-log | AuditLog | Auditor |

/admin/subscriptions: faner (role=tab) "Basispriser", "Volumen-rabatter", "Tilkøbsmoduler"; succesbesked .status-success (role=status).
Basispriser: .base-price-matrix-editor, 12 rækker i tbody; inputs med labels "Årlig pris for <navn>", "Månedlig pris for <navn>", "Perpetual-pris for <navn>" (<navn> = "<lag> · <kategori>"); "Gem" pr. række (step-up); fejl .status-danger.
Tilkøbsmoduler: .add-on-editor, 5 rækker i tbody (første kolonne = AddOnType); inputs med id name-/unit-/annual-/monthly-/perpetual-<id> (labels er ens); "Gem" og "Slet" pr. række; "Tilføj tilkøb" (#new-type, #new-name, …); "Nulstil til standardkatalog". Succesbesked "Tilkøb '<navn>' er opdateret."; duplikat-fejl "Der findes allerede et tilkøb med typen '<type>'." (.status-danger, uden "Error:").
/pricing matrix: .pricing-matrix-table-wrapper, cyklus-faner "Årligt"/"Månedligt"/"Engangsbeløb", h3 pr. lag, th[scope=row] = kategori, pris i td.numeric (C2).
Tilkøbskort: .addon-card med h4.addon-card-title; priser i .addon-card-prices span ("<beløb> /år", "<beløb> /md.", "<beløb> (engang)", C2 da-DK). Læser databasen.

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
| D1 | Modal vises ved beskyttet handling (+ AccessDenied for Requester) | ModalVisesTests | StepUp_`<Side>`_Side (20), StepUp_`<Side>`_Modal (15), StepUp_AccessDenied_`<bredde>` (5) | grøn |
| D2 | Forkert password/TOTP i modal + alarm ved 5. fejl | ForkertPasswordModalTests | StepUp_AdminSecurityPolicies_ForkertPassword_`<bredde>` (5) | grøn |
| D3 | Korrekt step-up, vindue og udløb, NoMfa | KorrektStepUpTests | StepUp_AdminSecurityPolicies_Foer, StepUp_AdminSecurityPolicies_Efter (`<bredde>`) (6) | ikke kørt |
| E1 | Anmodning uden for policy-grænser | AnmodningUdenForGraenserTests | JIT_E1_Fejl_`<bredde>` (3) | ikke kørt |
| E2 | Ansøger ser ikke egen anmodning i kø | EgenAnmodningIKoeTests | JIT_E2_Koe_`<bredde>` (5) | ikke kørt |
| E3 | Godkendelse → adgang straks | GodkendelseTests | JIT_E3_Foer_`<bredde>` (3), JIT_E3_Efter_`<bredde>` (3) | ikke kørt |
| E4 | Afvisning → adgang nægtes | AfvisningTests | JIT_E4_AccessDenied_`<bredde>` (3) | ikke kørt |
| E5 | Sweep markerer udløbne som Expired | SweepUdloebTests | JIT_E5_Udloebet_`<bredde>` (3) | ikke kørt |
| F1 | Rediger basispris | RedigerBasisprisTests | Basispris_AdminSubscriptions_Gem, Basispris_Pricing_Efter (`<bredde>`) (4) | ikke kørt |
| F2 | Rediger volumenrabat | RedigerVolumenrabatTests | Volumenrabat_AdminSubscriptions_Gem, Volumenrabat_Pricing_Efter (`<bredde>`) (4) | ikke kørt |
| F3 | Tilkøb og duplikat | RedigerTilkoebTests | Tilkoeb_AdminSubscriptions_Gem, Tilkoeb_AdminSubscriptions_Duplikat, Tilkoeb_Pricing_Efter (`<bredde>`) (6) | ikke kørt |
| G1 | Sikkerhedspolicy-faner loader seedede værdier | SikkerhedspolicyFanerTests | Sikkerhedspolicies_G1_`<Fane>`_`<bredde>` (12) | ikke kørt |
| G2a | JIT-policy træder i kraft | JitPolicyIKraftTests | ingen | ikke kørt |
| G2b | Fejl-tracker-policy træder i kraft | FejlTrackerPolicyIKraftTests | ingen | ikke kørt |
| G2c | AuditLog-eksport-policy træder i kraft | ExportJobPolicyIKraftTests | ingen | ikke kørt |
| G2d | ReAuth-policy træder i kraft | ReAuthPolicyIKraftTests | ingen | ikke kørt |
| G2e | Admin-API-ratelimit træder i kraft | AdminApiPolicyIKraftTests | ingen | ikke kørt |
| G2f | Login-ratelimit træder i kraft | LoginPolicyIKraftTests | ingen | ikke kørt |
| H1 | 6. hurtige login-forsøg → 429 | LoginRateLimitTests | RateLimiting_H1_`<bredde>` (2) | ikke kørt |
| H2 | Gem-spam i policy-editor → rate-limit-fejl | AdminHandlingerRateLimitTests | RateLimiting_H2_`<bredde>` (2) | ikke kørt |
| I1 | AdminNavMenu viser kun tilladte links (+ JIT-eskalering) | AdminNavigationTests | Adgangskontrol_I1_`<Persona>`_`<bredde>` (8), Adgangskontrol_I1d_`<bredde>` (2) | grøn |
| I2 | Direkte URL uden rolle (anonym → login, krydsadgang → AccessDenied) | AdminDirektUrlTests | Adgangskontrol_I2_Anonym_`<Side>`_`<bredde>` (8), Adgangskontrol_I2_AccessDenied_`<Persona>`_`<Side>`_`<bredde>` (18) | grøn |
| J1 | AuditLog: filtrering, sortering, paginering, præference | AuditLogFilterTests | Auditlog_J1_Filtreret, Auditlog_J1_IngenResultater (`<bredde>`) (4) | ikke kørt |
| J2 | Admin CRUD findes som rækker | (PR 5) | Auditlog_J2_Raekker | planlagt |
| J3 | AuditLog: detaljemodal viser Old/New JSON | AuditLogDetailTests | Auditlog_J3_Detaljer_`<bredde>` (2) | ikke kørt |
| J4 | Eksport: step-up, bekræftelse > 50.000, afvisning > 250.000 | (PR 6-7) | Auditlog_J4_StepUp, Auditlog_J4_Bekraeftelse, Auditlog_J4_Graense | planlagt |

Tests ligger under ManualSmoke/TjeklisteA/.
Tests ligger under ManualSmoke/TjeklisteB/.
Tests ligger under ManualSmoke/TjeklisteC/.
Tests ligger under ManualSmoke/TjeklisteD/.
Tests ligger under ManualSmoke/TjeklisteE/.
Tests ligger under ManualSmoke/TjeklisteF/.
Tests ligger under ManualSmoke/TjeklisteG/.
Tests ligger under ManualSmoke/TjeklisteH/.
Tests ligger under ManualSmoke/TjeklisteI/.
Tests ligger under ManualSmoke/TjeklisteJ/.

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
JitRequestsPage: "Actual value was N." i fejlbeskeden er .NET's engelske tekst og bruges til at skelne to fejl i træk. Skifter den tekst (fx lokalisering), skal E1 justeres.
SweepUdloebTests (E5) ændrer JIT-policyen i engangsdatabasen (Min = 1, Sweep = 1) og gendanner den i finally. Afbrydes processen midt i, forbliver policyen ændret, til databasen droppes (lokalt ved dispose, i CI ved næste CreateAsync). Testen tager 2-4 minutter, fordi appen kører med rigtig tid.
Basispriser deles med A2. F1 gendanner i finally; ved proces-kill kan databasen forblive ændret indtil næste CreateAsync (i CI) eller dropning (lokalt).
TabNavigation.SelectAsync dubleres af JitRequestsPage.SelectTabAsync og SecurityPolicyPage.SelectTabAsync, og AmountParser af PricingCalculatorTests.ParseAmount (kandidater til dedup-PR).
AdminActionGuard har en kvote pr. bruger (ukendt grænse). F1 gemmer 6 gange pr. bruger.
PricingCalculatorTests (A2) har egne kopier af calculator-selektorer og ParseAmount/ParseNumber, og af pris-polling; kandidat til dedup mod PricingCalculatorPage, AmountParser og PriceAssertions.
Login-limiter: OnRejected skriver ingen body/besked, så brugeren ser blank side eller browserens egen 429-fejlside (ingen "lockout-besked"). Verificeres i H1.
Policy-editors: Gem-knappen er ikke disabled under gem, og der er ingen unik succesbesked pr. gem.
ReAuthenticationPolicyEditor: valideringsgrænser for MaxAge ligger i servicen (min=1 i markup, intet max i UI).
LoginPolicyIKraftTests (G2f) har en privat kopi af LoginAttemptStatusAsync; kandidat til dedup mod LoginFlow.AttemptStatusAsync.
AuthorizationServicesExtensions: RequireGlobalAdminPolicy er bekræftet registreret to gange (første, RequireRole(SystemAdmin), er død kode). Fjern den første.
AuthorizationServicesExtensions: RequireDataAdminPolicy bruges ikke af nogen side eller af AdminNavMenu. DataAdmin har kun JIT-adgang som side.
Login/AccessDenied-stier er kun implicitte (ingen LoginPath/AccessDeniedPath i cookie-opsætningen). Sæt dem eksplicit.
To redirect-veje til login med forskellig ReturnUrl: auth-middleware (relativ, "ReturnUrl") og RedirectToLogin (absolut, "returnUrl"). LoginRedirect.ReturnPath accepterer begge.
AdminNavMenu er statisk: linkene opdateres først ved ny sideindlæsning efter ændret adgang (fx godkendt JIT).
Adgangsrettigheder.md er ikke opdateret med admin-siderne (/admin/subscriptions, /admin/security-policies, /admin/jit-requests, /admin/audit-log).
AuditLogFilterBar: "Til"-dato var eksklusiv (midnat); rettet i PR "AuditLog-filter Til-dato inklusiv". Datoer tolkes som UTC, men tabellen viser lokal tid (ToLocalTime), så rækker kort efter lokal midnat kan stå på en anden dag end filteret.
AuditLogFilterBar synkroniserer kun fra InitialFilter ved første OnParametersSet. Siden renderede den før præferencen var indlæst, så filterfelterne var tomme efter genindlæsning. Rettet: AdminAuditLogPage renderer først filterbaren, når præferencen er indlæst (filterReady).
AuditLog-rækker fra Blazor-handlinger har formentlig ingen handlende bruger (HttpContext er null i en circuit). Rettes efter J2.
AuditLogDetailModal: aria-modal="true", men markup'et har hverken Esc-håndtering, focus-trap eller returnering af fokus (jf. designets ConfirmDialog). Ikke testet i J3.
AuditLogDetailModal: dialogen kunne ikke scrolle, og "Luk" lå uden for viewporten ved 375×812 og 939×900 (to stablede JSON-blokke á max 400 px). Rettet: max-height + overflow-y: auto på dialogen og max-height 200 px på JSON-blokkene under 940 px.
