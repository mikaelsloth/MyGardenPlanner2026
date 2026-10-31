Indeks til manual smoke tests i MyGardenPlanner2026.Tests.E2E. Kun det, der er verificeret, står her. Mangler en side, så bed om .razor-filen.

## 1. Arbejdsproces

- Ny chat pr. tjekliste.
- Upload: denne fil, tjeklisten (med specs) og de relevante .razor-filer.
- Claude svarer med filliste og højst et par afklaringer, derefter PR'er.
- Hver PR opdaterer afsnit 5 (dækning) og de berørte sidesektioner.
- Rettelser fra afsnit 6: én chat pr. gruppe (R1–R9). Hver PR fjerner de løste punkter i afsnit 6 og opdaterer afsnit 3/4, hvis adfærden ændres.

## 2. Infrastruktur (i MyGardenPlanner2026.Tests.E2E)

### 2.1 Fixture, kørsel og skærmbilleder

- Fixture: `PlaywrightAppFixture`
  - `NewPageAsync()`, `NewPageAsync(int width)` (bredde fra Viewports)
  - `LoginAndGotoAsync(persona, path, width?)`: login som persona og åbn siden
  - `LoginAndGotoAsync(SmokeTestUser, path, width)`: samme for en vilkårlig seedet bruger
  - `RootUri`
  - `AppConnectionString`: appens forbindelse til engangsdatabasen
  - `SmtpServer` (`TestSmtpServer`), `SecurityAlertRecipient`, `SecurityAlertSubject` (delt med B2-testen, som har en privat kopi)
  - `ReAuthMaxAgeMinutes = 1`: sendes som `ReAuthenticationPolicy__MaxAgeMinutes` til E2E-appen (appsettings: 15)
  - `SeedExtraUserAsync(role)` → `SmokeTestDataSeeder.SeedExtraUserAsync(connectionString, role)`: ny bruger (e2e-`<guid>`@test.dk) med rollen og 2FA. Rollen skal findes (oprettes af SeedAsync). Giver en test egen fejltæller/session uden at røre personas.
  - Identity__ShowEmailLinks=true sendes til E2E-appen (links på RegisterConfirmation/Email vises kun med flaget, R2.1/R2.2)
- Skærmbilleder: `ScreenshotRecorder.CaptureAsync(page, kapitel, tjekliste, testnavn, komponent)`
  - Gemmer TestResults/Screenshots/`<kapitel>`/`<tjekliste>`/`<testnavn>`_`<komponent>`_`<bredde>`.png (fuld side).
  - CI uploader mappen som artefakt e2e-screenshots.
- Breakpoint: mobil = under 940 px.

| Viewports | (Viewports.All) |
| ------ | ------ |
| 375×812 | (Mobile) |
| 768×1024 | (Tablet) |
| 939×900 | (BelowBreakpoint) |
| 940×900 | (AtBreakpoint) |
| 1280×800 | (Desktop) |

- Trait: `[Trait(TestCategories.Category, TestCategories.ManualSmoke)]` og `[Collection(PlaywrightAppCollection.Name)]`.
- Kørsel: `dotnet test <E2E-csproj> --filter-trait "Category=ManualSmoke"`.
- `PlaywrightVolumeAppCollection` ("Playwright App Volume"): egen `PlaywrightAppFixture` (app, database, SMTP-sink) til tests med store datasæt, så den fælles database ikke forurenes. Collections kører sekventielt; i CI droppes og genskabes databasen med fast navn ved næste CreateAsync.
- Ruter (`E2ERoutes`): Landing, Pricing, About, Terms, Privacy, admin-ruter, EnableAuthenticator, AccessDenied, Login, Register, RegisterConfirmation, Lockout, Passkeys, ManageEmail, ConfirmEmailChange, ChangePassword, PersonalData, DeletePersonalData, TwoFactorAuthentication, LoginWith2fa.

### 2.2 Brugere og login

Smoke-brugere (`SmokeTestPersonas`, seedes af `SmokeTestDataSeeder`):

| Persona | Rolle | 2FA |
| ------ | ------ | ------ |
| Admin | SystemAdmin | ja |
| DataAdmin | DataAdmin | ja |
| PolicyAdmin | PolicyAdmin | ja |
| Auditor | AuditViewer | ja |
| NoMfa | SystemAdmin | nej |
| Requester | ingen | ja |
| Plain | ingen | nej |

- `LoginFlow`: `LoginAsync` = `SubmitPasswordAsync` + (hvis 2FA) `SubmitTwoFactorCodeAsync`. Bruger labels "E-mail" og "Adgangskode", knappen "Log ind" og "Godkendelseskode".
  - `FillCredentialsAsync(page, email, password)`, `LoginButton(page)`
  - `AttemptStatusAsync(page, rootUri, user)`: ryd cookies, åbn login, send formularen (ingen 2FA) og returnér HTTP-statussen på POST'en. Til tests, hvor svaret kan være 429.
- `PublicNavigation.ClickRegisterAsync(page)`: klikker "Opret bruger" i headeren, eller åbner NavDrawer, hvis linket er skjult.
- `RegisteredUserFlow` (nye brugere via UI, adgangskode = `SmokeTestDataSeeder.SharedPassword`, e-mail e2e-`<guid>`@test.dk):
  - `NewEmail()`, `RegisterAsync(page, email?)` (fra /Account/Register til RegisterConfirmation), `ConfirmAsync(page)` (følger bekræftelseslinket), `RegisterAndConfirmAsync(page, rootUri)`
- `SmokeTestDataSeeder`: `LockOutUserAsync(connectionString, email)` (LockoutEnd +30 min), `GetUserIdAsync(connectionString, email)`.
- `TwoFactorSetup` (bruger skal være logget ind uden 2FA):
  - `ReadSharedKeyAsync(page)`: læser `<kbd>`, giver Base32 med store bogstaver
  - `VerifyAsync(page, key)`: indtaster TOTP, klikker "Verify", returnerer gendannelseskoderne
  - `EnableAsync(page, rootUri, user)`: giver `TwoFactorEnrollment(User med nøgle, RecoveryCodes)`
- `VirtualAuthenticator.AddAsync(page)`: WebAuthn virtual authenticator (Chromium, CDP). `CountCredentialsAsync()`. Skal tilføjes før passkey-handlinger.
- `LoginRedirect.ReturnPath(url)`: stien i ReturnUrl fra en login-URL (relativ "ReturnUrl" fra middleware og absolut "returnUrl" fra RedirectToLogin).

### 2.3 Mail (SMTP-sink)

- `fixture.SmtpServer` (`TestSmtpServer`): SMTP-sink i testprocessen. `Messages`, `WaitForMessageAsync(predicate, timeout)`.
- App-processen får `Smtp__Host=127.0.0.1`, `Smtp__Port=<sinkens port>`, `Smtp__EnableSsl=false`, tomme `Smtp__UserName/Password`, og `Smtp__AdminSecurityEmails__0` = `SecurityAlertRecipient` (eneste modtager).
- `MimeText`: dekoder RFC 2047-headere og base64/quoted-printable (bruges af `TestSmtpServer`).
- `SecurityAlerts.CountPolicyChangedMails(mails, userId, policyName)`: antal "Sikkerhedspolicy ændret"-mails for brugeren og policyen (bekræfter hvert gem, når UI ikke har unik besked).

### 2.4 Interaktion og asserts

- `BlazorInteraction`: `ActUntilAsync(action, reached, ...)` gentager en handling, til tilstanden er nået. `WaitUntilAsync(condition, timeout)` poller en betingelse.
- `SmokePageAssertions.ExpectPageAsync(page, title, heading)`: titel, præcis én h1, ingen skeletons, skjult #blazor-error-ui.
- `AccessAssertions.ExpectAccessDeniedAsync(page)`: URL indeholder /Account/AccessDenied, titel/h1 "Adgang nægtet", teksten "Du har ikke adgang til denne ressource.".
- `PriceAssertions.ExpectValueAsync(locator, forventet, parse, beskrivelse)` / `ExpectAmountAsync(locator, forventet, beskrivelse)`: poller en vist værdi (5 sek.).
- `AmountParser.Parse(text)`: beløb ud fra cifrene i en vist pris (C2). `ParseNumber(text)`: heltal, brøk og procent (fx "85 %").
- `TabNavigation.SelectAsync(page, name)`: vælger role=tab og gentager klikket, til fanen er "active".

### 2.5 Admin og pricing: page-objects

- `StepUpModal` (role=dialog "Bekræft din identitet"):
  - `Dialog`, `PasswordField` (#step-up-password), `TotpField` (#step-up-totp, kun med 2FA), `ConfirmButton` ("Bekræft"), `CancelButton` ("Annullér")
  - `Error` (.status-message.status-danger[role=alert])
  - `OpenAsync(page, trigger)`: gentager triggeren, til modalen er vist
  - `CompleteAsync(page, user)`: korrekt password + TOTP, slutter når modalen er lukket
- `AdminSaveFlow`:
  - `SaveWithStepUpAsync(page, bruger, gemKnap, succesbesked, fejl, beskrivelse)`: Gem, step-up hvis modalen vises, afventer unik succesbesked
  - `RejectWithStepUpAsync(page, bruger, knap, fejl, forventetTekst)`: klik, step-up hvis modalen vises, afventer forventet fejltekst
- `AdminAccessMatrix`: hardcodet matrix persona → tilladte admin-sider (se 4.4). `AdminPage(Route, Title, Heading, LinkText)`, `AllPages`, `For(persona)`, `DeniedPages(persona)` (komplementet), `Personas`.
- `AdminNavigation` (page-object for nav.admin-nav-menu): `Nav(page)`, `Links(page)`, `Link(page, side)` (href-suffiks, versalfølsomt ikke), `ExpectExactLinksAsync(page, forventede)` (positiv kontrol først, derefter fravær og samlet antal).
- `SecurityPolicyPage` (/admin/security-policies):
  - `Retention` (#exportjob-retention), `SaveButton` ("Gem"), `Success` (.status-success), `ExportJobSavedText`
  - `SelectTabAsync(page, name)`, `OpenExportJobTabAsync(page)`, `ReadRetentionAsync(page)`, `SetRetentionAsync(page, value)`
  - JIT: `OpenJitTabAsync` (fanehop væk og tilbage = forbundet kredsløb; venter på #jit-min), `ReadJitPolicyAsync` → `JitPolicyValues(Min, Max, Sweep)`, `SaveJitPolicyAsync(page, bruger, min, sweep)` (sætter Min og Sweep, Gem, step-up, afventer succesbesked)
- `AdminSubscriptionsPage` (/admin/subscriptions, fanen Basispriser):
  - `OpenAsync(fixture, bruger, bredde?)`: login, åbn, vent til interaktiv (fanehop; 12 rækker)
  - `PriceInput/SaveButton/SavedMessage(page, lag, kategori)`, `ReadPriceAsync`, `SetPriceAsync`
  - `SaveTierAsync(page, bruger, lag, kategori)`: Gem, step-up hvis modalen vises, afventer besked
- `AdminVolumeDiscountsPage` (/admin/subscriptions, fanen Volumen-rabatter):
  - `OpenAsync(fixture, bruger, bredde?)`: login, åbn, vælg fane, vent på 7 rækker
  - Rækker findes via indeks (ens labels), sorteret efter MinGardens
  - `ReadMin/ReadMax/ReadMultiplierAsync`, `SetMultiplierAsync`, `SaveTierAsync(page, bruger, indeks, minGardens)`
  - `ReadTierIdAsync(page, indeks)` (Guid = EntityId i AuditLog), `AddTierAsync(page, bruger, minGardens, faktor)` (Max tom), `DeleteTierAsync(page, bruger, indeks)`. Et trin med højeste MinGardens havner som sidste række.
- `AdminAddOnsPage` (/admin/subscriptions, fanen Tilkøbsmoduler):
  - `OpenAsync(fixture, bruger, bredde?)`: login, åbn, vælg fane, vent på første række (antal kontrolleres ikke)
  - Rækker findes via type (første kolonne): `Row/NameInput/PriceInput/SaveButton(page, type)`
  - `ReadNameAsync`, `ReadPriceAsync`, `SetPriceAsync`, `SaveAddOnAsync(page, bruger, type, navn)`
  - `AddDuplicateAsync(page, bruger, type, navn)`: forventer afvisning. `DeleteLastRowAsync(page, bruger)`.
- `JitRequestsPage` (/admin/jit-requests):
  - `OpenAsync(fixture, bruger|persona, bredde?)`: logger ind, åbner siden og venter, til den er interaktiv (fanehop til Godkend og tilbage; Godkend-fanens overskrift findes ikke i prerenderet HTML). Slutter på "Anmod om adgang".
  - `SubmitRequestAsync`: klikker "Send anmodning" én gang (ikke idempotent). `RequestAndWaitAsync`: venter på rækken.
  - `RequestRow/QueueRow(page, årsag)`, `StatusBadge(række)`, `Error(page)`, `ShowHistoryAsync(page)`. Unik årsag: `NewReason()`.
  - `GotoAsync(fixture, page)`: åbner siden i en allerede logget ind side.
  - `ApproveAsync/RejectAsync(page, godkender, årsag, rolle)`: fanehop til Godkend, venter på rækken, klikker (gentages til step-up modalen vises), `StepUpModal.CompleteAsync`, afventer beskeden og at rækken er væk. Kræver en frisk circuit (ingen re-auth i forvejen).
  - `RefreshRequestsAsync(page, showHistory?)`: genindlæser "Mine anmodninger" via fanehop Godkend → Anmod (ingen sideindlæsning). "Vis afviste/udløbne" nulstilles ved hop og krydses af igen med showHistory.
- `PricingMatrix` (/pricing): `OpenAnonymousAsync(fixture, bredde)`, `SelectCycleAsync(page, cyklus)`, `ExpectPriceAsync(page, lag, kategori, beløb)`.
- `PricingCalculatorPage` (/pricing): `WaitUntilInteractiveAsync(page)`, `CalculateAsync(page, lag, kategori, cyklus, aktive, arkiverede)`, `ExpectResultAsync(page, vægtet, rabat%, subtotal, tilkøb, total)`.
- `PricingAddOns` (/pricing, tilkøbskort): `Cards(page)`, `Card/Price(page, navn, cyklus)`, `ExpectPriceAsync(page, navn, cyklus, beløb)`.
- `PricingVolumeDiscounts`: `PercentCell/ExpectPercentAsync(page, rækkenavn, procent)` (rabattabellen på /pricing, række fx "2 - 5 haver").

### 2.6 AuditLog

- `AuditLogTestData` (seeder admin.AuditLogs via app-forbindelsen, kun INSERT/SELECT; rækker kan ikke ryddes, brug unikke markører):
  - `NewEntityName(tag)`, `NewUserEmail()`, `Row(...)`, `SeedAsync(connectionString, rows)`
  - `SeedBulkAsync(connectionString, BulkSeed(entitet, e-mail, antal, handling, første tidspunkt, trin i sek.))`: set-baseret INSERT … SELECT, EntityId 1..antal. `StepSeconds: 0` giver rækker med identisk tidspunkt (til dagsfiltre med præcist antal).
  - `CountAsync(connectionString, entitet?, e-mail?)`, `CountForEntityAsync(connectionString, entitet, entitetsId?, handling?)` (til før/efter-sammenligning)
- `AuditLogPage` (/admin/audit-log): `OpenAsync(fixture, bruger, bredde?)` (venter på interaktivitet via første rækkes "Detaljer"; kræver mindst én række), `ReloadAsync`
  - Filter: `SelectEntityAsync/SelectActionAsync/SelectPageSizeAsync`, `SetTextAsync`, `SetDateAsync`, `SearchAsync`
  - Resultater: `Rows/RowsFor(page, entitet)/Cell(række, kolonne)`, `EmptyState`, `PaginationInfo`, Forrige/Næste
  - Modal: `DetailDialog`, `JsonBlock(page, 0|1)`, `CloseButton`, `DetailBackdrop`
  - Eksport: `ExportButton`, `BackgroundExportButton`, `ThresholdConfirm` (+Button/Cancel), `ExportError`, `ExportSuccess`; `JobRows`
- `AuditLogExport`: window.open opsnappes (init-script, installeres af `AuditLogPage.OpenAsync`). Verificerer ikke selve browser-downloadet.
  - `WaitForNewUrlAsync(page, kendtAntal)`, `FetchAsync(page, url, timeoutMs?)` (standard 30 sek.) → `ExportResponse(Status, ContentType, ContentDisposition, Body, Text, FileName)`
  - `ParseCsv`, `JsonRowCount`, `XlsxRowCount`, `IsZip`, `XlsxColumn(body, kolonne)`

## 3. Læringer (undgå gentagelser)

### Playwright og Blazor
- Playwright-regex understøtter kun IgnoreCase og Multiline. Andre flag giver ArgumentException.
- Interaktive Blazor Server-sider ignorerer klik og input, til kredsløbet er forbundet, og prerendering kan nulstille indtastninger. Brug `ActUntilAsync`, og indtast først, når siden har reageret.
- `@bind` og `@onchange` på inputfelter udløses ved blur. Efter `FillAsync` kaldes `BlurAsync()`.
- aria-expanded renderes som "True"/"False". Brug case-insensitiv sammenligning.
- `GetByLabel` skal bruge `Exact = true`, hvis et label er en delstreng af et andet (fx "Email" i "New email").
- Beløb vises med C2. Sammenlign cifrene (uden kultur, valutategn og separatorer) og divider med 100.
- Vent på skeletons: `[class*='skeleton']` skal have antal 0 før skærmbillede og assertions.
- Hver side har præcis én h1 (AccessDenied rettet i PR0; tjek med `SmokePageAssertions.ExpectPageAsync`).
- Fane-knapper med `aria-selected="@bool"`: true renderes som en tom attribut, false udelades. Brug klassen "active" som valgt-signal.
- Fanehop (SelectTab) rydder statusbeskeden og genindlæser editoren/komponenten: brug det til at bevise, at en ny besked stammer fra en ny gem.
- Positiv kontrol før negativ assert: nav (eget link synligt før fravær/antal), JIT-godkendelseskøen (en anden række før count == 0; komponenten renderer først "Ingen ventende anmodninger …", så data) og audit-loggen. Ellers kan en negativ assert bestå, før siden er renderet.
- Negativ assert på en Blazor-lukning (fx "klik inde i dialogen lukker ikke"): vent med `WaitUntilAsync` til en lukning ville være sket (fx 1 sek.) og assertér, at den ikke skete.
- `ToHaveTextAsync`/`ToContainTextAsync` normaliserer whitespace. Brug `TextContentAsync` (linjeskift normaliseret) til indrykning og flerlinjet tekst.
- Modaler skal kunne scrolle på lave viewports: en Playwright-fejl "element is outside of the viewport" efter scroll betyder, at elementet ikke kan nås for en rigtig bruger. Test at lukkeknappen kan klikkes ved 375×812 og 939×900.
- Komponenter, der kun synkroniserer fra en parameter ved første OnParametersSet, må ikke renderes, før forælderen har indlæst parameterens endelige værdi (Blazor renderer ved første await i OnInitializedAsync).
- Singleton-services (fx EF-interceptors) når circuit-brugeren via `CircuitUserContext` (AsyncLocal sat af `CircuitHandler.CreateInboundActivityHandler`), ikke via `IHttpContextAccessor`.
- `StatusMessage`: beskeder der starter med "Error" vises som danger (role=alert), ellers success (role=status). Præfikset skal forblive engelsk.
- WebAuthn kræver et domænenavn som host: kør passkey-tests på `http://localhost:<port>` (`RootUri.Replace("127.0.0.1", "localhost")`).
- Login.razor starter passkey-autofill (conditional mediation) ved sideload. Passkey-tests slår den fra med `AddInitScriptAsync("PublicKeyCredential.isConditionalMediationAvailable = async () => false;")`. Passkey-fejl vises som StatusMessage (role=alert) efter redirect ("Error: ...").

### Testdesign og delt data
- Tests er hardcodede forventede værdier. De udledes ikke af produktionskoden.
- Admin-editors deler data på tværs af tests: gendan ændrede værdier (retention ændres ±1 time) og brug ikke basispriser (påvirker A2-beregningerne).
- AuditLog-tests deler engangsdatabasen, og rækkerne kan ikke ryddes (DENY DELETE): brug unikke EntityName/UserEmail/EntityId-præfikser. Verificér resultater ved at sammenligne hele Entitets-ID-listen, ikke kun antal, da tabellen opdateres asynkront.
- Store datasæt hører hjemme i `PlaywrightVolumeAppCollection` (ellers rammer fx G2c, baggrundseksport med tomt filter, 250.000-grænsen). Præcise antal for et dagsfilter: seed én gruppe pr. dag med `StepSeconds: 0` og tidspunkt midt på dagen (12:00 UTC), og brug Fra–Til-intervaller.
- Visningspræferencen gemmes uden UI-signal efter søgning: vent på rækken i dbo.AuditLogViewerPreferences, før siden genindlæses.
- Admin-handlinger i AuditLog findes ved indhold (overgangen Old → New i detaljemodalen) blandt de nyeste rækker, ikke ved position. Antal nye rækker pr. handling afhænger af servicen (fx renummerering af DisplayOrder): assertér delta ≥ forventet, medmindre servicen er gennemgået. Åbn auditloggen EFTER handlingerne (entitetsdropdownen populeres ved indlæsning).
- Datofilter: "Til" er inklusiv (hele dagen).
- Eksporten bruger sidst SØGTE filter (`CurrentFilter`), ikke de aktuelle feltværdier. `Results.Problem` skriver ikke-ASCII som \u-escapes i den rå tekst: parse JSON og læs "detail".
- Policy-editors har ingen unik succesbesked pr. gem og ingen disabled-knap: bekræft hvert gem via alarm-mailen "Policy: <navn>". Afvent mailen før næste indtastning (editoren genindlæser efter gem).
- En blokeret bruger kan ikke gendanne selv: åbn en ny PolicyAdmin FØR ændringen og brug den til gendannelse.

### Step-up, re-auth og sikkerhedsalarm
- `ReAuthenticationService` er Scoped. Login markerer kun re-auth i request-scope, så en frisk circuit har intet tidsstempel, og `StepUpReAuthModal` vises ved første beskyttede handling uden ventetid. Efter step-up gælder vinduet i samme circuit; reload = ny circuit = ny modal.
- Kun udløb i samme circuit kræver ventetid: E2E-appen kører med MaxAgeMinutes=1, og testen venter ca. 62 sek.
- Eksport: step-up vises kun første gang i en circuit inden for re-auth-vinduet. Hjælpere, der eksporterer flere gange, må håndtere begge udfald (modal eller direkte URL).
- Admin-sider kræver 2FA: NoMfa redirectes til EnableAuthenticator og når aldrig modalen.
- Fejltæller (`ReAuthFailureTracker`): alarm sendes ved hvert multiplum af Threshold (5, 10, 15 …) inden for vinduet. Fejl tælles samlet pr. bruger på tværs af login og step-up (vindue 2 dage). Brug en ny bruger pr. test (`SeedExtraUserAsync`).
- Modal: forkert password og forkert TOTP (med korrekt password) tælles. Tom TOTP tælles ikke. Skift mellem de to fejltyper, når flere forsøg skal afventes, så den nye fejltekst adskiller sig fra den forrige.
- Alarmen sendes til `Smtp:AdminSecurityEmails` (ikke til brugeren) og indeholder Bruger-ID (ikke e-mail): genkend en brugers alarm på Bruger-ID. Alarm fra en circuit: IP-adressen hentes af CircuitUserCircuitHandler, når circuiten åbnes eller forbindes (R1.5). I E2E er den 127.0.0.1.
- E2E-appen skal have Smtp__*-miljøvariabler til TestSmtpServer. Ellers bruger den appsettings’ Smtp:Host. En transportfejl ved 5. forkerte login logges som advarsel (EventId 1130), alarmmailen udebliver, og login giver ikke længere 500. Afsendelsen afbrydes efter Smtp:TimeoutSeconds (standard 10 sek.) og logges som advarsel (EventId 1134).

### Rate limiting
- Admin-API-limiter (`AdminActionRateLimiter`): sliding window pr. bruger (singleton, 60 sek., 6 segmenter), genopbygges kun ved ændring af `AdminApiRateLimitOptions` (ny kvote). Forbruges efter step-up og før service-validering, så et afvist gem giver rate-limit-teksten uanset værdi. Et afvist forsøg bruger ingen permit.
- Login-limiter (`ReloadableLoginRateLimiter`): fixed window pr. IP, kun beskyttede auth-POST'er, genopbygges (tæller nulstilles) ved policyændring. 429 uden body (OnRejected logger kun).

### Identity-sider
- Manage-siderne ligger i ManageLayout (se 4.3).
- Ingen log ud-knap i PublicLayout/ManageLayout: log ud i E2E = `page.Context.ClearCookiesAsync()`.
- Email.razor RegisterConfirmation viser links til e-mailskift på siden, når EmailSender er IdentityNoOpEmailSender og Identity:ShowEmailLinks er true (E2E-fixturen sætter flaget). Uden det kan skiftet ikke bekræftes i E2E.
- ConfirmEmailChange: forkert token eller ugyldigt Base64Url-format i code giver "Error: Der opstod en fejl ved skift af e-mail." (status 200).
- ChangePassword: succes redirecter til samme side (`RedirectToCurrentPageWithStatus`) med "Your password has been changed". Man forbliver logget ind (`RefreshSignInAsync`). Labels i markup ("Old password", "New password", "Confirm password") afviger fra Display-navnene i valideringsbeskeder ("Current password", "New password", "Confirm new password").
- PersonalData: "Download" er en almindelig POST-formular (ikke enhanced) til /Account/Manage/DownloadPersonalData. Brug `page.RunAndWaitForDownloadAsync` og læs filen via `download.PathAsync()`. "Delete" er et link (`<a>`), ikke en knap.
- DeletePersonalData: succes sletter først brugerens data (AccountDeletionService), derefter brugeren (DeleteAsync + SignOutAsync) og redirecter til forsiden. Efterfølgende tjek er, at login afvises. Slet-knappen vises ikke længere.

### Admin-navigation
- AdminNavMenu ligger i AdminLayout og er statisk (ingen rendermode på layoutet). Efter en ændring i adgang (fx godkendt JIT) kræves en ny sideindlæsning, før linkene ændres. Adgang efter godkendelse: `HasActiveElevationAsync` læser DB ved hvert kald (ingen cache), så ny sideindlæsning er nok.
- AuditLog-linket kan indeholde `.nav-badge` (antal nye eksportfiler): find links via href-suffiks, ikke via tekst.

## 4. Sider

### 4.1 Offentlige sider

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
Indeholder matrix, rabattabel, tilkøbskort og PricingCalculator. Alt læser databasen.
Matrix: .pricing-matrix-table-wrapper, cyklus-faner "Årligt"/"Månedligt"/"Engangsbeløb", h3 pr. lag, th[scope=row] = kategori, pris i td.numeric (C2).
Rabattabel: .garden-volume-discount-table (via IGardenVolumeDiscountService). th[scope=row] = '<min> - <max> haver' / '1 have' / '<min>+ haver', td = '<procent> % af basispris'.
Tilkøbskort: .addon-card med h4.addon-card-title; priser i .addon-card-prices span ('<beløb> /år', '<beløb> /md.', '<beløb> (engang)', C2 da-DK).
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
Verificeret: matrix + calculator (A2). Rabattabel og tilkøbskort: F2/F3 (ikke kørt).
```
```
/about AboutPage        Titel: Om platformen – MyGardenPlanner.     h1: Om MyGardenPlanner.
/terms TermsPage        Titel: Handelsbetingelser – MyGardenPlanner. h1: Handelsbetingelser.
/privacy PrivacyPage    Titel: Privatlivspolitik – MyGardenPlanner.  h1: Privatlivspolitik.
```
```
PublicHeader og NavDrawer (offentlig navigation)
Header: header.public-header med <a href="/account/login"> "Log ind" og <a href="/account/register"> "Opret bruger".
Hamburger: button[aria-label='Åbn menu'], klasse public-header-menu-toggle, vises under 940 px.
Desktop-links: .public-header-links, vises fra 940 px.
Synlighed af "Log ind"/"Opret bruger" under 940 px er ikke verificeret (PublicHeader.razor.css ikke læst).
Drawer: nav.nav-drawer, klassen open når den er åben. Baggrund: .drawer-backdrop (kun når åben).
Drawer har 4 links, fokus-trap via JS-modul (NavDrawer.razor.js, ikke læst). Esc lukker, og fokus returnerer til hamburger.
Breakpoint-stilarter ligger i PublicHeader.razor.css.
Filer: Components/Layout/PublicHeader.razor (+ .razor.cs, .razor.css), NavDrawer.razor (+ .razor.cs)
```

### 4.2 Registrering og login

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
Link "Klik her for at bekræfte din konto" vises kun med IdentityNoOpEmailSender og Identity:ShowEmailLinks = true (E2E-fixturen sætter flaget). Ukendt e-mail (flag til): status 404 og "Error: Kunne ikke finde bruger for den angivne e-mail.". Uden flaget (eller med anden sender) slås brugeren ikke op, og siden viser <p role="alert"> "Tjek venligst din e-mail for at bekræfte din konto." uanset om e-mailen findes. Flag-fra er kun dækket af bUnit.
Filer: RegisterConfirmation.razor (+ .cs)
```
```
/Account/ConfirmEmail
Titel: Bekræft e-mail – MyGardenPlanner. h1: Bekræft e-mail.
Succes: "Tak, fordi du bekræftede din e-mail." + link "Klik her for at logge ind".
Fejl (forkert token eller ugyldigt format i code): "Error: Der opstod en fejl ved bekræftelse af din e-mail." Ingen login-link.
Filer: ConfirmEmail.razor (+ .cs)
```
```
/Account/Login
Titel: Log ind – MyGardenPlanner. h1: Log ind. Layout: PublicLayout.
Labels: "E-mail", "Adgangskode" (Exact = true), "Husk mig". Knap: "Log ind" (Exact = true; passkey-knappen hedder "Log ind med en passkey": PasskeySubmit, Request, e-mailfeltet sendes som username; tomt felt giver discoverable credential).
Fejl ved forkert login: StatusMessage "Error: Invalid login attempt." (engelsk).
Login bruger lockoutOnFailure: false. Redirect til /Account/Lockout sker kun for en allerede låst bruger. Et login-forsøg på en låst bruger registreres også i ReAuthFailureTracker.
Forkert password kalder ReAuthFailureTracker.RecordFailureAsync. Ved 5, 10, 15 … forsøg (multipla af Threshold) sendes sikkerhedsalarm (ISecurityAlertService) til Smtp:AdminSecurityEmails.
  Emne: "[MyGardenPlanner] Sikkerhedsalarm: Gentagne fejlede login-/re-auth-forsøg". Krop: "Bruger-ID: <id>" og "IP-adresse: <ip>".
Login rate limit: fixed window pr. IP, kun beskyttede auth-POST'er (AdminAuthPathMatcher). Ved overskridelse svarer serveren 429 uden body.
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
Et forsøg på en låst bruger registreres også som fejlforsøg.
Filer: LoginWith2fa.razor (+ .cs)
```
```
/Account/LoginWithRecoveryCode
Titel: Gendannelseskode – MyGardenPlanner. h1: Bekræft med gendannelseskode.
Label: "Gendannelseskode" (Exact), knap "Log ind". Nås fra LoginWith2fa via linket "logge ind med en gendannelseskode" (kræver password-login først).
Gyldig kode: login, redirect til forsiden. Brugt/forkert kode: "Error: Ugyldig gendannelseskode indtastet." og et fejlforsøg registreres i ReAuthFailureTracker (5 forkerte udløser sikkerhedsalarm). Ingen lockout-tælling i koden.
Et forsøg på en låst bruger registreres også som fejlforsøg.
Filer: LoginWithRecoveryCode.razor (+ .cs)
```
```
/Account/ConfirmEmailChange?userId&email&code
Titel: Bekræft e-mailskift – MyGardenPlanner. h1: Bekræft e-mailskift (AuthPageShell).
Succes: "Tak, fordi du bekræftede skiftet af din e-mail." (e-mail og brugernavn skiftes, brugeren logges ind igen).
Fejl: "Error: Der opstod en fejl ved skift af e-mail." (ugyldig token eller ugyldigt Base64Url-format).
Filer: ConfirmEmailChange.razor (+ .cs)
```

### 4.3 Manage (kontosider efter login)

Fælles: ManageLayout (h1 "Manage your account"). Titler og tekster er på engelsk, og titlerne har ikke "– MyGardenPlanner".

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
/Account/Manage/Passkeys
Titel: Manage your passkeys. h3 "Manage your passkeys".
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
/Account/Manage/Email
Titel: Manage email. h3 "Manage email". Static SSR.
Labels: "New email" (Exact = true, forudfyldt med nuværende e-mail), deaktiveret felt "Email". Knap: "Change email".
Besked: "Confirmation link to change email sent. Please check your email." (success), "Your email is unchanged." ved uændret e-mail.
Link "Click here to confirm your new email" vises kun med IdentityNoOpEmailSender og Identity:ShowEmailLinks = true.
Ugyldig e-mail: ValidationMessage (.validation-message).
Ubekræftet e-mail: knappen "Send verification email" giver "Verification email sent. Please check your email." og (samme betingelser) linket "Click here to verify your email". Ikke dækket af E2E: brugere med ubekræftet e-mail kan ikke logge ind (RequireConfirmedAccount), så kun bUnit dækker det.
Filer: Email.razor (+ .cs)
```
```
/Account/Manage/ChangePassword
Titel: Change password. h3 "Change password". Static SSR.
Labels (Exact = true): "Old password", "New password", "Confirm password". Knap: "Update password".
Succes: redirect til samme side, besked "Your password has been changed".
Forkert nuværende password: "Error: Incorrect password." (Identity-standardtekst).
Valideringsfejl (.validation-message): "The New password must be at least 6 and at max 100 characters long." og "The new password and confirmation password do not match.".
Brugere uden lokalt password redirectes til Account/Manage/SetPassword.
Filer: ChangePassword.razor (+ .cs)
```
```
/Account/Manage/PersonalData
Titel: Personal Data. h3 "Personal Data". Static SSR.
Knap "Download" (POST til /Account/Manage/DownloadPersonalData, kræver login). Link "Delete" til /Account/Manage/DeletePersonalData.
Filen PersonalData.json: nøgler fra [PersonalData]-felterne (fx "Email") samt eksterne login-nøgler. "Authenticator Key" eksporteres ikke.
Filer: PersonalData.razor (+ .cs), IdentityComponentsEndpointRouteBuilderExtensions.cs, PersonalDataExporter.cs
```
```
/Account/Manage/DeletePersonalData
Titel: Delete Personal Data. h3 "Delete Personal Data".
Felt "Password" (kun hvis brugeren har password), knap "Delete data and close my account".
Forkert password: "Error: Incorrect password.". Eneste ejer i have med andre medlemmer: "Error: You are the only owner of gardens shared with other members: <haver>. Ownership must be transferred before your account can be deleted." Fejlet DeleteAsync: "Error: Your account could not be deleted. Please try again." Succes: data og konto slettes, bruger logges ud, redirect til forsiden.
Filer: DeletePersonalData.razor (+ .cs), AccountDeletionService.cs, IAccountDeletionService.cs
```

### 4.4 Admin

Fælles for alle admin-sider:
- Layout: AdminLayout (_Imports i Pages/Admin) → MainLayout. Ingen h1 i layoutet (hver side har sin egen). Filer: AdminLayout.razor (+ .css), AdminNavMenu.razor (+ .cs, .css).
- Alle sider er rolle- og 2FA-beskyttede (MfaRequirement). Siden kræver ikke "recent auth". Direkte rolle eller aktiv JIT giver adgang.
- Uden 2FA: redirect til /Account/Manage/EnableAuthenticator. Uden admin-rolle: /Account/AccessDenied. Anonym: redirect til /Account/Login med ReturnUrl til den forespurgte side (auth-middleware sender relativ "ReturnUrl", RedirectToLogin sender absolut "returnUrl"). Der er ingen eksplicit LoginPath/AccessDeniedPath (cookie-standarder). Sidens h1 og AdminNavMenu vises ikke på login- eller AccessDenied-siden. Filer: Routes.razor (+ .cs), RedirectToLogin.razor(.cs), RedirectToAccessDenied.razor(.cs), AccessDenied.razor (+ .cs). Tests: ManualSmoke/TjeklisteI/AdminDirektUrlTests.
- Step-up: `StepUpGuard.RunAsync` omkring følsomme handlinger → `StepUpReAuthModal` (role=dialog, "Bekræft din identitet"). Vises ved første handling i en frisk circuit (se afsnit 3). MaxAgeMinutes default 15, E2E-appen kører med 1. Selektorer: se `StepUpModal` i 2.5.
- AdminNavMenu: nav.admin-nav-menu[aria-label='Administration'], ét `<li><a>` pr. tilladt link, href relativ ("admin/subscriptions"). Aktuel side har klassen "active". AuditLog-linket får .nav-badge[aria-label] med antal nye eksportfiler, når der er ulæste. Filer: AuthorizationServicesExtensions.cs, AnyAdminRoleAuthorizationHandler.cs, JitRoleAuthorizationHandler.cs. Tests: ManualSmoke/TjeklisteI/AdminNavigationTests.

| Rute | Overskrift | Nav-link | Policy | Persona |
| ------ | ------ | ------ | ------ | ------ |
| /admin/subscriptions | Administrer abonnementer | Abonnementer | RequireGlobalAdmin (SystemAdmin) | Admin |
| /admin/security-policies | Sikkerhedspolicies | Sikkerhedspolicies | RequirePolicyAdmin | PolicyAdmin |
| /admin/jit-requests | JIT-adgang | JIT-adgang | RequireAnyAdminRole (alle fire roller) | DataAdmin (alle har adgang) |
| /admin/audit-log | AuditLog | AuditLog | RequireAuditViewer | Auditor |

Adgang pr. persona (`AdminAccessMatrix`): Admin: Abonnementer, JIT-adgang. DataAdmin: JIT-adgang. PolicyAdmin: Sikkerhedspolicies, JIT-adgang. Auditor: JIT-adgang, AuditLog. Tabellens persona-kolonne er kendt fra PersonaAccessTests (ikke gennemgået med .razor).

```
/admin/subscriptions
Faner (role=tab): "Basispriser", "Volumen-rabatter", "Tilkøbsmoduler". Succesbesked .status-success (role=status). Fejl .status-danger.
Basispriser: .base-price-matrix-editor, 12 rækker i tbody; inputs med labels "Årlig pris for <navn>", "Månedlig pris for <navn>", "Perpetual-pris for <navn>" (<navn> = "<lag> · <kategori>"); "Gem" pr. række (step-up).
Volumen-rabatter: .volume-discount-editor, 7 rækker i tbody; inputs med id min-/max-/mult-<id> (labels er ens på alle rækker); "Gem" og "Slet" pr. række; "Tilføj trappe" (#add-tier); "Nulstil til standardkatalog" (+ "Ja, nulstil"). Succesbesked: "Volumenrabat-trappe fra <n> haver er opdateret."
Tilkøbsmoduler: .add-on-editor, 5 rækker i tbody (første kolonne = AddOnType); inputs med id name-/unit-/annual-/monthly-/perpetual-<id> (labels er ens); "Gem" og "Slet" pr. række; "Tilføj tilkøb" (#new-type, #new-name, …); "Nulstil til standardkatalog". Succesbesked "Tilkøb '<navn>' er opdateret."; duplikat-fejl "Der findes allerede et tilkøb med typen '<type>'." (.status-danger, uden "Error:").
```
```
/admin/security-policies
Faner i rækkefølge (role=tab, valgt = klassen "active"): JIT-eskalering (aktiv ved indlæsning), Step-up re-auth, Fejl-tracker, Admin-API rate limit, Login rate limit, AuditLog-eksport.
Fejl i editor: .status-danger[role=alert] (fx rate limit, ArgumentOutOfRange). Skeleton vises, til policyen er indlæst. Gem-knap "Gem" (AdminActionGuard → StepUpGuard).
JIT-eskalering (JitElevationPolicyEditor): #jit-min, #jit-max, #jit-sweep (type=number, @onchange → blur). Succes: "JIT-eskaleringspolicyen er opdateret og trådt i kraft." Standard (JitElevationPolicy): Min 30, Max 90, Sweep 5 minutter.
  Sweep (RoleElevationExpirySweepService): kun Approved med ValidToUtc < nu → Expired. Pending og Rejected røres aldrig. Interval er mindst 1 minut. En ændret policy afbryder ventetiden, og sweep kører med det samme.
  Adgang afgøres uafhængigt af Status: HasActiveElevationAsync tjekker ValidToUtc direkte.
Step-up re-auth: #reauth-maxage (type=number, min=1, @onchange → blur). Fejl vises øverst i editoren (.security-policy-editor .status-danger[role=alert]); StepUpReAuthModal ligger i editoren. Succes: "Step-up re-auth-policyen er opdateret og trådt i kraft." (page-level, bliver stående mellem gem).
Admin-API rate limit: #adminapi-permit, #adminapi-window, #adminapi-segments. Fejl: "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen." (AdminActionGuard.RateLimitedMessage).
AuditLog-eksport: #exportjob-retention, #exportjob-maxactive (type=number). Succes: .status-success[role=status] "AuditLog-eksportjob-policyen er opdateret og trådt i kraft." (ryddes ved fanskift).
Filer: AdminSecurityPolicyPage.razor (+ .cs), JitElevationPolicyEditor.razor (+ .cs), RoleElevationExpirySweepService.cs
Tests: ManualSmoke/TjeklisteD/ForkertPasswordModalTests, ManualSmoke/TjeklisteD/KorrektStepUpTests.
```
```
/admin/jit-requests
Titel: JIT-adgang – MyGardenPlanner. h1: JIT-adgang. Brugere uden admin-rolle (Requester, Plain) kan ikke åbne siden.
Faner: role=tab, valgt fane har klassen "active". "Anmod om adgang" (standard) og "Godkend anmodninger". Fanehop rydder statusbeskeden og genindlæser fanens komponent.
Statusbesked ved godkend/afvis: .status-success[role=status] på siden.
Anmod-fanen (JitElevationRequestForm):
  #jit-request-role (select, værdier = rollenavne, standard SystemAdmin), #jit-request-minutes (number, standard 30, @onchange), #jit-request-reason (@bind).
  Knap "Send anmodning": ikke idempotent; ved succes vises .status-success på siden: ‘Anmodning om ‘<rolle>‘ er sendt.’; årsagsfeltet ryddes. Ingen step-up ved anmodning.
  Checkbox #jit-request-show-history ("Vis afviste/udløbne"): standard fra, så kun Afventer/Godkendt vises. Nulstilles ved fanehop.
  Tabel "Mine anmodninger": Rolle, Minutter, Årsag, Status, Oprettet. Status-badges: "Afventer", "Godkendt", "Afvist", "Udløbet".
  Fejl: .status-danger[role=alert] "Error: <ex.Message>". Uden for grænserne: “<N> minutter ligger uden for grænsen. Antal minutter skal være mellem {min} og {max}.” Tom årsag: “Årsag skal udfyldes.”. Rate limit: "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen."
Godkend-fanen (JitElevationApprovalQueue):
  h3 "Anmodninger til godkendelse". Tabel: Ansøger (bruger-ID, ikke e-mail), Rolle, Minutter, Årsag, Anmodet, Handling.
  Knapper pr. række: "Godkend" og "Afvis" (AdminActionGuard → StepUpGuard → StepUpReAuthModal).
  Tom kø: "Ingen ventende anmodninger, du kan godkende."
  Beskeder: "Anmodning om '<rolle>' er godkendt." / "Anmodning om '<rolle>' er afvist." Fejl: "Error: ...".
  Kø-regler (JitElevationService.GetPendingRequestsForApprovalAsync): kun Pending, ikke egne, og kun roller godkenderen er direkte medlem af (aktiv JIT tæller ikke).
  Køen indlæses ved fanevalg og opdateres ikke automatisk.
Filer: AdminJitRequestsPage.razor (+ .cs), JitElevationRequestForm.razor (+ .cs), JitElevationApprovalQueue.razor (+ .cs), IJitElevationService.cs, JitElevationService.cs, JitElevationPolicyOptions.cs, AnyAdminRoleAuthorizationHandler.cs, JitRoleAuthorizationHandler.cs, AuthorizationServicesExtensions.cs
Tests: ManualSmoke/TjeklisteE/.
```
```
/admin/audit-log
Detaljemodal (AuditLogDetailModal):
  Åbnes med knappen "Detaljer" på en række. Backdrop: .confirm-dialog-backdrop (klik lukker; dialogen stopper bobling). Dialog: .audit-log-detail-dialog, role=dialog, aria-labelledby → h2 "Detaljer — <entitet> (<id>)".
  Metalinje: p.meta "<tidspunkt g da-DK, lokal tid> · <e-mail> (<bruger-id>) · <ip>".
  To kolonner (.audit-log-detail-column) med h3 "Tidligere værdier" / "Nye værdier" og pre.audit-log-detail-json. Side om side fra 940 px, under hinanden under (max-width 939.98px). Dialogen scroller (max-height + overflow-y: auto; JSON-blokke max 200 px under 940 px).
  Visning: gyldig JSON indrykket (2 mellemrum); null, tom eller whitespace → "(ingen data)"; ugyldig JSON → rå tekst. Knap "Luk" lukker.
  Tests: ManualSmoke/TjeklisteJ/AuditLogDetailTests.
Rækker fra Blazor-handlinger har handlende bruger via CircuitUserCircuitHandler (AsyncLocal CircuitUserContext) og HttpContextCurrentUserAccessor. IP-adressen er klientens IP fra circuit-forbindelsen (R1.5).
Filter: AuditLogFilterBar synkroniserer kun fra InitialFilter ved første OnParametersSet. AdminAuditLogPage renderer derfor først filterbaren, når præferencen er indlæst (filterReady). Datoer tolkes som UTC.
Eksportpanel (AuditLogExportPanel):
  Panel: .audit-log-export-panel. Format: #export-format (værdier Csv/Json/Xlsx). Knapper: "Eksportér" og "Send til baggrundseksport" (begge bag step-up og rate limit).
  Eksportér åbner /admin/audit-log/export?format=…&token=…&entityName=…&userEmail=… via window.open(url, "_blank"). Filnavn audit-log-export-<yyyyMMdd-HHmmss>.<csv|json|xlsx>. Ugyldigt token: 403 med problem details "Eksport-token er ugyldig eller udløbet: …".
  CSV-header: Id,UserId,UserEmail,IpAddress,Action,EntityName,EntityId,OldValues,NewValues,TimestampUtc. JSON: array af AuditLogEntryDto (Action som tekst). Excel: ark "AuditLog".
  Baggrundseksport: succes "Eksporten er sat i kø. …", job i .audit-log-export-jobs-panel (kolonner: Oprettet, Format, Status, Rækker, Handling; status Afventer/Kører/Klar/Fejlet). Listen genindlæses med "Opdater".
  Grænser: bekræftelse ved > 50.000 rækker ("Eksporten omfatter <n> rækker og kan tage et øjeblik. Fortsæt?", n uden tusindtalsseparator), afvisning ved > 250.000 ("Error: Eksporten omfatter <n> rækker, hvilket overstiger grænsen på 250000. Indsnævr filteret og prøv igen."). Endpointet afviser > 250.000 med 422 (problem details "detail").
  Baggrundseksport tjekker ikke rækkeantallet ved oprettelse: AuditLogExportJobProcessor sætter jobbet til Fejlet ("Eksporten omfatter <n> rækker, hvilket overstiger grænsen på 250000. Indsnævr filteret."), vist som .meta i job-rækken.
  Tests: ManualSmoke/TjeklisteJ/AuditLogEksportTests, AuditLogEksportVolumenTests.
```

### 4.5 Ikke indekseret endnu

- Komponenter ud over dem nævnt her.
- Loginbeskyttede sider uden for Manage og admin (ingen er indekseret).

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
| J2 | Admin CRUD findes som rækker (policy, volumenrabat, basispris, tilkøb, opret/slet trin, JIT) | AdminHandlingerIAuditLogTests | Auditlog_J2_Raekker_`<bredde>` (2) | ikke kørt |
| J3 | AuditLog: detaljemodal viser Old/New JSON | AuditLogDetailTests | Auditlog_J3_Detaljer_`<bredde>` (2) | ikke kørt |
| J4 | Eksport: step-up, formater, ugyldigt token, baggrundseksport; grænser 50.000/250.000 | AuditLogEksportTests, AuditLogEksportVolumenTests | Auditlog_J4_StepUp_`<bredde>` (2), Auditlog_J4_Bekraeftelse_`<bredde>` (2), Auditlog_J4_Graense_`<bredde>` (2) | ikke kørt |

Testklasserne ligger under `ManualSmoke/Tjekliste<bogstav>/` (A–J). Volumentests (`PlaywrightVolumeAppCollection`) ligger under `ManualSmoke/TjeklisteJ/` (`AuditLogEksportVolumenTests`).

## 6. Åbne observationer (ikke rettet)

Backlog grupperet i rettelsesgrupper i prioriteret rækkefølge. Én chat pr. gruppe. Løste punkter slettes; når alle grupper er tomme, fjernes afsnittet. Alle punkter er observationer fra tidligere gennemgange; intet er verificeret på ny.

### R1 – Login, re-auth og sikkerhedsalarm (prioritet 1)
~~- R1.1 `SmtpSecurityEmailSender`: doc-kommentaren siger "fejler blødt", men det gælder kun manglende konfiguration. Transportfejl (SmtpException) kastes videre og vælter login-flowet (500) ved 5. forkerte forsøg.~~
~~- R1.2 `ReAuthFailureTracker`: alarm sendes kun ved præcis Threshold (==). Ved 6+ fejl inden for vinduet sendes ingen ny alarm, før tælleren nulstilles (succes i login/step-up). Fejl tælles samlet på tværs af login og step-up.~~
~~- R1.3 `Login.razor.cs`: IsLockedOut-grenen kalder ikke `TrackReAuthOutcomeAsync` (relevant, når lockout-featuren bygges; login bruger i dag lockoutOnFailure: false).~~
~~- R1.4 Login/LoginWith2fa/LoginWithRecoveryCode kalder `MarkReAuthenticated` i request-scope (Scoped service) → ingen effekt på circuit; modalen vises altid ved første beskyttede handling. Modaltekst "sidst logget ind" er misvisende.~~
~~- R1.5 `HttpContextCurrentUserAccessor`: HttpContext er normalt null i en circuit → alarm fra `StepUpReAuthModal` får IP "ukendt", og AuditLog-rækker fra Blazor-handlinger har tom IP (UserId er kendt via `CircuitUserContext`).~~

~~Filer til Upload: SmtpSecurityEmailSender, ReAuthFailureTracker(+Options), Login/LoginWith2fa/LoginWithRecoveryCode, ReAuthenticationService, StepUpReAuthModal, HttpContextCurrentUserAccessor, CircuitUserCircuitHandler~~

### R2 – Kontosider: fejlhåndtering og e-mail (prioritet 2)
~~- R2.1 \RegisterConfirmation`: viser stadig bekræftelseslinket uden flag og afslører, om en e-mail findes (D8). `Email.razor` er rettet i PR 3.`~~
~~- R2.2 `Email.razor`: `OnSendEmailVerificationAsync` viser ikke linket (kun relevant for ubekræftede e-mails, ikke dækket).~~
~~- R2.3 `ConfirmEmailChange.razor.cs`: `WebEncoders.Base64UrlDecode(Code)` er uden try/catch. Et ikke-Base64Url-format i code kaster formentlig FormatException (500). Ikke testet.~~
~~- R2.4 `DeletePersonalData`: hvis `UserManager.DeleteAsync` fejler, kastes InvalidOperationException (500) uden UI-fejl. Sletning af brugere med relationer (haver, medlemskaber) er ikke undersøgt eller dækket (ny bruger har ingen). Destinationen efter sletning for en ikke-logget-ind bruger (login eller NotAuthorized) er ikke verificeret.~~
~~- R2.5 `DownloadPersonalData`: `(await GetAuthenticatorKeyAsync(user))!` giver null i JSON for brugere uden authenticator (ingen fejl, men null-forgiving er unøjagtig).~~

~~Filer til Upload: Email, ConfirmEmailChange, DeletePersonalData, RegisterConfirmation, IdentityComponentsEndpointRouteBuilderExtensions~~

### R3 – JIT-eskalering: regler og robusthed (prioritet 3)
- R3.1 `JitElevationService.RequestElevationAsync` tjekker kun minutter inden for policyen og at rollen findes. Ansøgerens berettigelse til rollen tjekkes ikke, og en bruger kan anmode om en rolle, vedkommende allerede har (backlog).
- R3.2 Brugere uden admin-rolle kan ikke åbne /admin/jit-requests (RequireAnyAdminRole), så de kan ikke anmode via UI. Afklar den tilsigtede adfærd.
- R3.3 `ApproveElevationAsync` kalder `securityAlertService.AlertJitRequestedAsync` ved godkendelse, ikke ved anmodning. Mailens indhold er ikke verificeret.
- R3.4 `JitElevationApprovalQueue` viser ansøgerens bruger-ID (GUID), ikke e-mail.
~~- R3.5 `JitElevationRequestForm`: "Send anmodning" giver ingen succesbesked, og fejlbeskeden indeholder .NET-teksterne "(Parameter 'minutes')" og "Actual value was N." (engelsk). `JitRequestsPage` bruger "Actual value was N." til at skelne to fejl i træk (E1); ændres teksten (fx lokalisering), skal E1 justeres.~~
- R3.6 `JitElevationApprovalQueue`: kun InvalidOperationException fanges ved godkend/afvis; andre fejl vælter circuiten.
- R3.7 Sweep rører kun Approved. Pending anmodninger udløber aldrig og forbliver i køen.
- R3.8 Approve/Reject i JitElevationService tjekker ikke, at godkenderen er direkte medlem af rollen (kun køen filtrerer).
- R3-backlog: Ansøger skal kunne trække egen anmodning tilbage (ny funktionalitet, uden for R3-scope).

Filer til Upload: JitElevationService/IJitElevationService, JitElevationRequestForm, JitElevationApprovalQueue, RoleElevationExpirySweepService, AdminJitRequestsPage

### R4 – Abonnementsdata og admin-services (prioritet 4)
- R4.1 `GardenVolumeDiscountTier` (og `SubscriptionAddOn.Type`): det unikke indeks på MinGardens har intet filter på IsDeleted. Et soft-slettet trin optager sit MinGardens, så et nyt trin med samme værdi afvises med "Der findes allerede en trappe der starter ved N haver", selvom ingen synlig række har den. Ikke testet; J2 bruger en tilfældig MinGardens.
- R4.2 `GardenVolumeDiscountAdminService.SaveAsync`: DbUpdateException fanges som duplikat-fejl uanset årsag.
- R4.3 Rabatopslaget bruger kun MinGardens; MaxGardens bruges ikke.

Filer til Upload: GardenVolumeDiscountTier og SubscriptionAddOn med EF-konfiguration, GardenVolumeDiscountAdminService, migrationer

### R5 – AuditLog: korrekthed og volumen (prioritet 5)
- R5.1 Datoer i filteret tolkes som UTC, men tabellen viser lokal tid (ToLocalTime), så rækker kort efter lokal midnat kan stå på en anden dag end filteret.
- R5.2 Baggrundseksport: oprettelsen accepterer filtre over 250.000 rækker. Brugeren får først besked om afvisningen, når jobbet er behandlet (status Fejlet). Overvej at afvise ved oprettelse.
- R5.3 `AuditLogExportService.ExportAsync` henter alle rækker i hukommelsen (SearchAsync med PageSize = totalCount). Ved 250.000 rækker som Excel (ClosedXML) kan det belaste appen kraftigt; ikke testet.

Filer til Upload: AuditLogFilterBar, AdminAuditLogPage, AuditLogExportService, AuditLogExportJobProcessor

### R6 – Tilgængelighed (prioritet 6)
- R6.1 Lukket NavDrawer er kun skubbet ud af skærmen med transform, så linkene i den kan nås med Tab.
- R6.2 aria-expanded renderes med stort begyndelsesbogstav ("True"/"False").
- R6.3 `AuditLogDetailModal`: aria-modal="true", men markup'et har hverken Esc-håndtering, focus-trap eller returnering af fokus (jf. designets ConfirmDialog). Ikke testet i J3.

Filer til Upload: NavDrawer (+.cs/.js/.css), AuditLogDetailModal, ConfirmDialog

### R7 – Admin-konfiguration, rate limit-UX og dokumentation (prioritet 7)
- R7.1 Login-limiter: `OnRejected` skriver ingen body/besked, så brugeren ser blank side eller browserens egen 429-fejlside (ingen "lockout-besked"). Verificeres i H1.
- R7.2 Policy-editors: Gem-knappen er ikke disabled under gem, og der er ingen unik succesbesked pr. gem.
- R7.3 `ReAuthenticationPolicyEditor`: valideringsgrænser for MaxAge ligger i servicen (min=1 i markup, intet max i UI).
- R7.4 `AuthorizationServicesExtensions`: `RequireGlobalAdminPolicy` er registreret to gange (første, RequireRole(SystemAdmin), er død kode). Fjern den første.
- R7.5 `AuthorizationServicesExtensions`: `RequireDataAdminPolicy` bruges ikke af nogen side eller af AdminNavMenu. DataAdmin har kun JIT-adgang som side.
- R7.6 Login/AccessDenied-stier er kun implicitte (ingen LoginPath/AccessDeniedPath i cookie-opsætningen). Sæt dem eksplicit.
- R7.7 To redirect-veje til login med forskellig ReturnUrl: auth-middleware (relativ, "ReturnUrl") og `RedirectToLogin` (absolut, "returnUrl"). `LoginRedirect.ReturnPath` accepterer begge.
- R7.8 AdminNavMenu er statisk: linkene opdateres først ved ny sideindlæsning efter ændret adgang (fx godkendt JIT).
- R7.9 Adgangsrettigheder.md er ikke opdateret med admin-siderne (/admin/subscriptions, /admin/security-policies, /admin/jit-requests, /admin/audit-log).

Filer til Upload: ReloadableLoginRateLimiter, RateLimitingServicesExtensions, policy-editors, AuthorizationServicesExtensions, cookie-opsætning, Adgangsrettigheder.md

### R8 – Lokalisering (prioritet 8, efter R1–R3)
- R8.1 `Register.razor.cs`: valideringsbeskeder (StringLength, Compare) og Display-navne er på engelsk.
- R8.2 `Login.razor.cs`: fejlbeskeden "Error: Invalid login attempt." er på engelsk.
- R8.3 Manage-siderne (TwoFactorAuthentication, EnableAuthenticator, Passkeys, RenamePasskey) og StatusMessage-teksterne er på engelsk. Login-siderne er på dansk.
- R8.4 `RenamePasskey.razor` har ingen PageTitle.
- R8.5 ChangePassword: fejlbeskeder fra Identity-password-politikken (fx "Error: Passwords must have at least one digit") er engelske og ikke dækket af test, da politikken ikke er i konteksten.
- R8.6 `StatusMessage` skelner danger/success på præfikset "Error" (afsnit 3). Ved lokalisering skal præfikset bevares, eller konventionen ændres.
- R8.7 Alle hardcodede forventede tekster i E2E-testene (A–J) og afsnit 4 skal opdateres sammen med tekstændringerne.

Filer til Upload: de berørte sider, StatusMessage

### R9 – Test-oprydning (prioritet 9, sidst)
- R9.1 Dedup: `TabNavigation.SelectAsync` dubleres af `JitRequestsPage.SelectTabAsync` og `SecurityPolicyPage.SelectTabAsync`. `AmountParser` dubleres af `PricingCalculatorTests.ParseAmount`.
- R9.2 `PricingCalculatorTests` (A2) har egne kopier af calculator-selektorer, ParseAmount/ParseNumber og pris-polling; dedup mod `PricingCalculatorPage`, `AmountParser` og `PriceAssertions`.
- R9.3 `LoginPolicyIKraftTests` (G2f) har en privat kopi af `LoginAttemptStatusAsync`; dedup mod `LoginFlow.AttemptStatusAsync`.
- R9.4 `AuditLogEksportVolumenTests.ClickWithStepUpAsync` dublerer `ExportAsync` i `AuditLogEksportTests`.
- R9.5 `SecurityAlertSubject` er delt med B2-testen, som har en privat kopi.
- R9.6 `SweepUdloebTests` (E5) ændrer JIT-policyen i engangsdatabasen (Min = 1, Sweep = 1) og gendanner den i finally. Afbrydes processen midt i, forbliver policyen ændret, til databasen droppes (lokalt ved dispose, i CI ved næste CreateAsync). Testen tager 2-4 minutter, fordi appen kører med rigtig tid.
- R9.7 Basispriser deles med A2. F1 gendanner i finally; ved proces-kill kan databasen forblive ændret indtil næste CreateAsync (i CI) eller dropning (lokalt).
- R9.8 `AdminActionGuard` har en kvote pr. bruger (ukendt grænse). F1 gemmer 6 gange pr. bruger.

Filer til Upload: de berørte E2E-filer

### R10 – Kontosletning: ejerskab og backup (separat scope)
- R10.1 Eneste ejer i en have med andre medlemmer blokerer kontosletning. Mangler: ejerskabsoverdragelse (vælg ny ejer) eller bekræftelse af, at alles adgang til havedata slettes (dog ikke andre brugeres konti).
- R10.2 Tilbyd en backup-fil af brugerens data, som kun kan bruges, hvis brugeren vender tilbage.
- R10.3 Layer 2/3-data (bede, planer m.m.) skal medtages i AccountDeletionService (eller kaskade), når de bygges.
- R10.4 AccountDeletionService bruger ExecuteDelete og skriver derfor ingen AuditLog-rækker. Overvej en eksplicit audit-post som dokumentation.
- R10.5 Kontosider på engelsk (blokeringsbesked mv.) lokaliseres i R8.

Filer til Upload: AccountDeletionService, DeletePersonalData, Garden-entiteterne