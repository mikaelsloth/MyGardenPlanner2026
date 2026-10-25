# 2. Verifikationsproces pr. modul

## J. Admin – AuditLog (`/admin/audit-log`, kræver AuditViewer)
- [ ] Filtrering pr. felt (entitet, entitets-ID, bruger, handling, dato), sortering, paginering og gemt præference giver korrekte resultater
- [ ] Alle forudgående admin CRUD-handlinger (E/F/G) findes som rækker her
- [ ] Detaljemodal viser Old/New JSON pænt formatteret
- [ ] Eksport CSV/JSON/Excel: kræver step-up, bekræftelse ved > 50.000 rækker, afvisning ved > 250.000 rækker

### Forudsætninger
- Hver test er selvforsynende og uafhængig af rækkefølge. Der asserteres aldrig på absolutte totaler.
- Hver test bruger en frisk AuditViewer (`SeedExtraUserAsync`), da visningspræferencer er pr. bruger.
- Testdata seedes direkte i admin.AuditLogs via app-forbindelsen (INSERT-rettighed) med unikke markører:
  - EntityName `J<n>-<guid8>`
  - UserEmail `j-<guid>@test.dk`
- J2 udfører selv handlingerne og sammenligner antal rækker før/efter (filter: EntityName + EntityId + Handling + dags dato).
- J4 volumentests (50.000/50.001/250.000/250.001 rækker) kører i egen collection og egen database.
- Dato-filtre er UTC. "Til" er inklusiv hele dagen (kræver PR 1).
- Grænser: bekræftelse ved > 50.000, afvisning ved > 250.000 (`IAuditLogExportService.HardRowLimit`).

### J1. Filtrering, sortering, paginering og præference
- Route: /admin/audit-log
- Bruger: #4 Auditor (rolle) – i testen en frisk AuditViewer
- Handling:
  a. Seed 16 rækker: entitet A (12 rækker) og entitet B (4 rækker), 2 brugere, 3 handlinger, tidspunkter ved dagsgrænser (00:00:00 og 23:59:59, anker = i dag − 30 dage UTC).
  b. Søg pr. felt: Entitet (dropdown), Entitets-ID, Bruger-ID, E-mail, Handling, Fra, Til.
  c. Kombiner felter. Sæt Fra = Til = én dag. Sæt Fra > Til.
  d. Sæt Rækker pr. side = 10 og brug Næste/Forrige.
  e. Søg, genindlæs siden (ny circuit) og kontrollér filterfelterne. Skift derefter side og genindlæs igen. Åbn siden som en anden bruger.
- Forventet:
  - #1 Dropdown indeholder de seedede entitetsnavne alfabetisk. Valg giver kun den entitets rækker.
  - #2 Entitets-ID, Bruger-ID og E-mail matcher eksakt. En delstreng giver "Ingen resultater".
  - #3 Handling viser kun valgt type (badges "Oprettet"/"Opdateret"/"Slettet").
  - #4 Kombinerede felter virker som AND.
  - #5 Fra og Til er inklusive: række 00:00:00 og 23:59:59 på valgt dag er med, 1 sek. udenfor er ikke. Fra = Til giver præcis én dag. Fra > Til giver "Ingen resultater".
  - #6 Sortering: nyeste først.
  - #7 Entitet A med 10 rækker pr. side: ‘Side 1 af 2 (12 rækker i alt)’. Forrige er deaktiveret på side 1, Næste er deaktiveret på side 2. Side 2 viser 2 rækker.
  - #8 Efter Søg og genindlæsning er felter og sidestørrelse genskabet, og tabellen matcher. Sidenavigation gemmes ikke. En anden bruger ser standard (25, intet filter). Filteret i testen er entitet A, Fra, Til og sidestørrelse 10.

- Skærmbilleder (768, 1280): Auditlog_J1_Filtreret, Auditlog_J1_IngenResultater
- Filer: AuditLogFilterBar.razor(+.cs), AuditLogResultsTable.razor(+.cs), AdminAuditLogPage.razor(+.cs), AuditLogQueryService.cs

### J2. Admin CRUD-handlinger findes som rækker
- Route: /admin/audit-log (handlinger udføres på admin-siderne)
- Bruger: #4 Auditor (læser). Handlinger udføres af frisk SystemAdmin, PolicyAdmin og AuditViewer/Admin.
- Handling:
  a. PolicyAdmin ændrer AuditLog-eksport retention og gendanner.
  b. SystemAdmin ændrer faktor på trin 2–5 og gendanner.
  c. SystemAdmin ændrer basispris (HaveArkitekt · Editor, årlig) og gendanner.
  d. SystemAdmin ændrer tilkøbspris (Bedforslag Niveau 2, månedlig) og gendanner.
  e. SystemAdmin opretter og sletter et volumenrabat-trin.
  f. AuditViewer anmoder to gange om SystemAdmin. Admin godkender den ene og afviser den anden.
  g. Åbn auditloggen som frisk AuditViewer og find rækkerne.
- Forventet:
  - #1 Præcis 2 nye Update-rækker (ændring og gendannelse).
  - #2 Mindst 2 nye Update-rækker med Old/New som overgangen original ↔ ændret.
  - #3 Create-række (Old = ingen data, New viser MinGardens, PriceMultiplier, IsDeleted false) og Delete-række (Old viser IsDeleted false, New = ingen data), filtreret på trinnets ID.
  - #4 Create-række pr. anmodning (Status Pending). Update Pending → Approved og Pending → Rejected.
  - #5 Rækkerne kan genfindes via filteret (entitet + entitets-ID + handling).
  - #6 Rækken har handlende bruger (UserEmail i tabellen og UserId i detaljemodalens metalinje). IP-adressen er tom i circuits.

- Skærmbilleder (768, 1280): Auditlog_J2_Raekker
- Filer: AuditLoggingInterceptor.cs, HttpContextCurrentUserAccessor.cs, ‹entiteter og services verificeres›

### J3. Detaljemodal viser Old/New JSON pænt formatteret
- Route: /admin/audit-log
- Bruger: #4 Auditor (rolle) – i testen en frisk AuditViewer
- Handling:
  a. Seed 5 rækker: Create (Old null), Update (kompakt enkeltlinje-JSON), Delete (New null), ugyldig JSON og tom streng.
  b. Klik "Detaljer" på hver række.
  c. Mål layout ved 375, 768, 939, 940 og 1280.
  d. Luk med "Luk" og med klik på baggrunden.
- Forventet:
  - #1 Dialogen har rolle dialog og titel "Detaljer — <entitet> (<id>)". Metalinjen viser tidspunkt, e-mail og IP.
  - #2 Gyldig JSON er indrykket og flerlinjet, og indholdet kan parses som JSON.
  - #3 Null og tom streng viser "(ingen data)". Ugyldig JSON vises som rå tekst uden fejl.
  - #4 Update-rækken viser både Old og New som indrykket JSON, hvor kun de ændrede værdier afviger. En rigtig række fra admin-handlinger dækkes af J2 (#2).
  - #5 Fra og med 940 px står "Tidligere værdier" og "Nye værdier" side om side. Under 940 px står de under hinanden. Gælder ved 375, 768, 939, 940 og 1280 px.
  - #6 "Luk" og klik på baggrunden lukker. Klik inde i dialogen lukker ikke.
  - #7 Dialogen kan scrolle, og ‘Luk’ kan nås ved 375×812 og 939×900.

- Skærmbilleder (768, 1280): Auditlog_J3_Detaljer
- Filer: AuditLogDetailModal.razor(+.cs, .css), AuditLogResultsTable.razor

### J4. Eksport CSV/JSON/Excel: step-up, bekræftelse > 50.000, afvisning > 250.000
- Route: /admin/audit-log
- Bruger: #4 Auditor (rolle) – i testen en frisk AuditViewer
- Handling:
  a. Eksportér i en frisk circuit. Annullér modalen. Klik igen og gennemfør step-up.
  b. Kald `/admin/audit-log/export?format=Csv&token=ugyldigt` direkte.
  c. Seed 3 rækker (unik e-mail). Søg på e-mailen og eksportér som CSV, JSON og Excel.
  d. (egen collection) Seed 250.001 rækker i grupper, så filtrene giver 50.000, 50.001, 250.000 og 250.001. Eksportér for hvert filter.
  e. Klik "Send til baggrundseksport" med 3-rækkers filteret.
  f. (valgfri, beslutning 4) Send baggrundseksport med 250.001-filteret.
- Forventet:
  - #1 Første Eksportér i en frisk circuit viser step-up modalen, og intet download startes. Efter Annullér og nyt klik vises modalen igen. Efter korrekt password + TOTP starter download.
  - #2 Ugyldigt token giver 403 med "Eksport-token er ugyldig eller udløbet".
  - #3 Filnavn `audit-log-export-<yyyyMMdd-HHmmss>.<csv|json|xlsx>`. Kun rækker fra sidst søgte filter eksporteres. Ændrede, men ikke søgte felter påvirker ikke eksporten.
    - CSV: header `Id,UserId,UserEmail,…,TimestampUtc` + 3 rækker (BOM ignoreres).
    - JSON: array med 3 elementer, Action som tekst.
    - Excel: gyldig zip med ark ‘AuditLog’ og 3 datarækker (overskrift kommer i tillæg)
  - #4 50.000 rækker: ingen bekræftelse, download starter.
  - #5 50.001 rækker: "Eksporten omfatter 50001 rækker og kan tage et øjeblik. Fortsæt?". Annullér giver intet download. "Ja, fortsæt" downloader 50.001 rækker.
  - #6 250.000 rækker: bekræftelse (ikke fejl). 250.001 rækker: "Error: Eksporten omfatter 250001 rækker, hvilket overstiger grænsen på 250000. Indsnævr filteret og prøv igen." Ingen bekræftelse og intet download.
  - #7 Baggrundseksport: "Eksporten er sat i kø…" vises, og en række (Format Csv, status Afventer/Kører/Klar) står i "Baggrundseksporter". Færdiggørelse og download dækkes af G2c.
  - #8 (valgfri) Baggrundseksport over grænsen ender som "Fejlet" med fejltekst.

- Skærmbilleder (768, 1280): Auditlog_J4_StepUp, Auditlog_J4_Bekraeftelse, Auditlog_J4_Graense
- Filer: AuditLogExportPanel.razor(+.cs), AuditLogExportJobsPanel.razor(+.cs), AuditLogEndpointsExtension.cs, AuditLogExportService.cs, AuditLogExportJobProcessor.cs