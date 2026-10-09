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
```/Account/Login
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
| ------ | ------ | ------ | ------ |
| A1 | Offentlige sider loader | OffentligeSiderLoadTests | Loadside_`<Side>`_`<bredde>` (25) | grøn |
| A2 | Pricing-calculator vs. Prismatrix | PricingCalculatorTests | Prisberegning_PricingCalculator_`<bredde>` (3) | grøn |
| A3 | Mobilmenu og fokus-trap | MobilmenuTests | ingen | grøn |
| B1 | Opret bruger | OpretBrugerTests | Oprettelse_Register_`<bredde>` (5), Oprettelse_RegisterConfirmation_`<bredde>` (5) | ikke kørt |
| B2 | Forkert login og Lockout | ForkertLoginTests | ForkertLogin_Lockout_`<bredde>` (3) | ikke kørt |
| B3 | 2FA | TofaktorTests | 2FA_EnableAuthenticator, 2FA_RecoveryCodes, 2FA_LoginWith2fa (`<bredde>`) (9) | ikke kørt |

Tests ligger under ManualSmoke/TjeklisteA/.
Tests ligger under ManualSmoke/TjeklisteB/.

## 6. Åbne observationer (ikke rettet)
Lukket NavDrawer er kun skubbet ud af skærmen med transform, så linkene i den kan nås med Tab.
aria-expanded renderes med stort begyndelsesbogstav.
Rabatopslaget bruger kun MinGardens.
Register.razor.cs: valideringsbeskeder (StringLength, Compare) og Display-navne er på engelsk.
Login.razor.cs: fejlbeskeden "Error: Invalid login attempt." er på engelsk.
Login.razor.cs: IsLockedOut-grenen kalder ikke TrackReAuthOutcomeAsync (relevant, når lockout-featuren bygges).
SmtpSecurityEmailSender: doc-kommentaren siger "fejler blødt", men det gælder kun manglende konfiguration. Transportfejl (SmtpException) kastes videre og vælter login-flowet ved 5. forkerte forsøg.
Manage-siderne (TwoFactorAuthentication, EnableAuthenticator) og StatusMessage-teksterne er på engelsk. Login-siderne er på dansk.
