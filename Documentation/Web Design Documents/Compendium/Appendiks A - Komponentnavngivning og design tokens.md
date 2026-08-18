# Appendiks A: Komponentnavngivning og design tokens

Dette appendiks fastlægger den samlede komponentarkitektur, navngivningskonvention og design token-struktur for **MyGardenPlanner**. Appendikset fungerer som den ultimative reference for både frontend-udviklere og UI/UX-designere.

---

## 1. Komponentarkitektur og Komponentordbog

### 1.1 Navngivningsprincipper og Konventioner

For at sikre en skalerbar, vedligeholdelsesvenlig og DRY (Don't Repeat Yourself) kodebase gælder følgende regler:

1. **Sprogopdeling:** Kode, komponentnavne, C#-properties, CSS-klasser og tokens skrives **altid på engelsk**. Alle brugerrettede tekster (labels, hjælptekster, fejlmeddelelser og statusbeskeder) skrives **på dansk**.
2. **Standardisering af Komponentnavne (Prefix/Suffix):**
   * I kildedokumenterne er visse komponenter omtalt med et `Mgp`-prefix (fx `MgpButton`, `MgpCard`). I koden og i dette systemkatalog **fjernes `Mgp`-prefixet** på selve klasse- og filnavnet (fx `Button.razor`, `Card.razor`), da projektets C#-namespace (`MyGardenPlanner.Components`) yder tilstrækkelig indkapsling.
   * Komponenter navngives efter **rolle og formål**, ikke udseende.
   * Der anvendes konsekvente suffixes til at indikere mønsteret.
3. **States beskriver aktuel tilstand, fx `loading`, `expanded`, `collapsed`, `empty`, `error`, `archived`, `processing`.**

#### Tabel: Standardiserede Komponent-suffixes

| Suffix | Mønsterbeskrivelse | Eksempel |
| :--- | :--- | :--- |
| `-Card` | Afgrænset overfladecontainer til entiteter, opsummering eller sektioner. | `Card`, `PlantCard`, `SummaryCard` |
| `-Dialog` / `-Modal` | Modal overlejring, der kræver direkte brugerafklaring. | `ConfirmDialog` |
| `-Drawer` | Off-canvas / bottom-sheet panel til mobil navigation og filtrering. | `FilterDrawer`, `NavDrawer` |
| `-Badge` | Kompakt status-, tæller- eller klassifikationsindikator. | `RoleBadge`, `AttentionBadge`, `ArchivedBadge` |
| `-Banner` | Bred, tværgående notifikations- eller statuslinje. | `StatusBanner`, `ArchiveBanner`, `GlobalBanner` |
| `-Grid` | Responsiv layout-container til kort eller medier. | `SummaryGrid`, `ThumbnailGrid` |
| `-Row` | Horisontalt listeelement optimeret til høj datatæthed. | `CompactEntityRow` |
| `-Input` / `-Select` | Specifikke formularkontroller med udvidet logik. | `SearchInput`, `SortSelect` |
| `-Section` | Formular- eller indholdssektion. | `FormSection`, `ReadOnlySection` |
| `-Skeleton` | Placeholder til uafklaret indlæsningstilstand. | `SkeletonCard`, `SkeletonRow`, `SkeletonMediaCard` |
| `-Gate` | Logisk wrapper-komponent til betinget adgang/visning. | `PermissionGate` |

---

### 1.2 Fælles Komponenttilstande (States)

For at undgå komponenteksplosion håndteres dynamisk opførsel via fælles, standardiserede states på tværs af komponentkataloget:

| State | Betydning / UI-manifestation | Eksempel på anvendelse |
| :--- | :--- | :--- |
| `default` | Standard, stabil tilstand. | Kort eller knap i normal visning. |
| `hover` | Brugeren har markøren over elementet (`:hover`). | Fremhævet kant eller baggrunds-tint. |
| `focus` | Tastaturfokus aktivt (`:focus-visible`). | Tydelig og kontrastrig focus ring (`--mgp-focus-ring`). |
| `active` / `selected` | Elementet er valgt eller aktivt i en samling. | Valgt tab, valgt view-mode knap. |
| `expanded` | Elementet/sektionen er foldet ud og viser fuldt indhold. | Sammenfoldeligt sidekort (`CollapsibleSectionCard`), `DangerZone`, mobil drawer. |
| `collapsed` | Elementet/sektionen er sammenfoldet og viser kun overskrift/resumé. | Sammenfoldeligt sidekort (`CollapsibleSectionCard`), `DangerZone`. |
| `disabled` | Elementet kan ikke aktiveres i den aktuelle kontekst. | Deaktiveret knap ledsaget af `.permission-hint`. |
| `loading` / `processing` | Handlingen afventer asynkront svar eller behandling. | Knap med spinner (`.btn-spinner`), `ObjectStatus`. |
| `empty` | Komponenten eller visningen indeholder ingen data. | Tom liste, ubenyttet upload-zone (`UploadZone`), `EmptyState`. |
| `error` | Der er opstået en validerings- eller systemfejl. | Valideringsfejl i `FormField`, mislykket upload eller API-fejl. |
| `archived` | Objektet er inaktiveret/historisk gemt. | `.card-archived` med muted visual stil. |
| `restricted` | Adgang nægtet eller begrænset pga. permissions. | Visning med metadata, men låste handlinger. |

---

### 1.3 Komponentordbog (Opdelt i 6 Kategori-Domæner)

---

#### A. Foundation Components

##### 1. `Button`
* **Dokumentnavn:** `MgpButton`
* **Formål / Usage:** Udførelse af primære, sekundære eller destruktive brugerhandlinger.
* **Variants:** `primary`, `secondary`, `accent`, `danger`, `ghost`, `icon`.
* **States:** `default`, `hover`, `focus`, `active`, `disabled`, `loading` (med `.btn-spinner` og bydeformstekst).
* **Tokens:** `--mgp-primary`, `--mgp-primary-dark`, `--mgp-accent`, `--mgp-danger`, `--mgp-danger-bg`, `--mgp-danger-border`, `--mgp-touch-target-min`, `--mgp-focus-ring`.
* **Accessibility:** Minimum 44×44px touch target. Ikonknapper uden tekst kræver `aria-label`. Synlig focus ring ved `:focus-visible`.
* **Mobile & Print:** Udvides til full-width (`.btn-mobile-full`) i mobilformularer. Skjules på print.
* **Relaterede komponenter:** `FormActions`, `ButtonAligned`, `LoadingButton`.

##### 2. `Badge`
* **Dokumentnavn:** `MgpRoleBadge`, `MgpAttentionBadge`, `MgpArchivedBadge`, `MgpFileBadge`, `NavBadge`
* **Formål / Usage:** Viser status, rollemærkning, antal eller klassifikation i en kompakt indpakning.
* **Variants:** `neutral`, `primary`, `accent`, `danger`, `archived`.
* **States:** `default`.
* **Tokens:** `--mgp-surface-muted`, `--mgp-primary-soft`, `--mgp-primary-dark`, `--mgp-accent`, `--mgp-danger`, `--mgp-state-archived`.
* **Accessibility:** Tekstbåret information (farve står aldrig alene).
* **Mobile & Print:** Skalleres proportionalt; udskrives i monokrom med synlig kant.
* **Relaterede komponenter:** `AttentionBadge`, `RoleBadge`, `ArchivedBadge`.

##### 3. `Thumbnail`
* **Dokumentnavn:** `MgpThumbnail`
* **Formål / Usage:** Billed- og dokument-forhåndsvisning i lister, kort og modaler med automatisk fallback.
* **Variants:** `sm` (48px), `md` (96px), `lg` (16:9 responsive).
* **States:** `loading` (shimmer), `loaded`, `error`/`fallback` (filtypeikon).
* **Tokens:** `--mgp-surface-muted`, `--mgp-border`, `--mgp-primary-dark`.
* **Accessibility:** Kraver `alt`-tekst baseret på kontekst (tom `alt=""` hvis dekorativ).
* **Mobile & Print:** Skalleres fleksibelt; skjules ved ultra-kompakte mobiludskrifter.
* **Relaterede komponenter:** `ThumbnailGrid`, `SkeletonMediaCard`, `MediaPreview`.

##### 4. `SkeletonCard` / `SkeletonRow` / `SkeletonMediaCard`
* **Dokumentnavn:** `MgpSkeletonCard`, `MgpSkeletonRow`, `MgpSkeletonMediaCard`
* **Formål / Usage:** Lokal placeholder under dataindlæsning for at forhindre Layout Shift (CLS).
* **Variants:** `card`, `row`, `media`.
* **States:** `shimmer` (animation), `reduced-motion` (statisk dæmpet).
* **Tokens:** `--mgp-surface-muted`, `--mgp-primary-soft`, `--mgp-border`.
* **Accessibility:** Deaktiverer shimmer ved `prefers-reduced-motion: reduce`. Skjult for skærmlæsere (`aria-hidden="true"`).
* **Mobile & Print:** Bevarer samme aspect-ratio og height på mobil. Skjules ved print.
* **Relaterede komponenter:** `InlineLoading`, `EmptyState`.

##### 5. `Stack`
* **Dokumentnavn:** `MgpStack`
* **Formål / Usage:** Layout-hjælper til vertikal og horisontal adskillelse efter den faste spacing-skala.
* **Variants:** `2xs` (4px), `xs` (8px), `sm` (12px), `md` (16px), `lg` (24px), `xl` (32px).
* **States:** N/A.
* **Tokens:** `--mgp-space-2xs` til `--mgp-space-xl`.
* **Accessibility:** Bevarer logisk læserækkefølge i DOM'en.
* **Mobile & Print:** Justerer gabs automatisk via responsive page margin tokens.
* **Relaterede komponenter:** `AppShell`, `FormSection`.

---

#### B. Layout Components

##### 1. `AppShell`
* **Dokumentnavn:** `AppShell`
* **Formål / Usage:** Overordnet applikations-ramme med sidebar, topbar og main content-område.
* **Variants:** `default`, `sidebar-collapsed` (nav rail).
* **States:** `desktop` (stiv sidebar), `mobile` (off-canvas drawer).
* **Tokens:** `--mgp-sidebar-width` (280px), `--mgp-sidebar-collapsed-width` (72px), `--mgp-mobile-header-height` (56px), `--mgp-bg`.
* **Accessibility:** Indeholder `main`-landemærke og "Spring til hovedindhold"-link.
* **Mobile & Print:** Skifter til sticky mobile topbar + off-canvas drawer på skærme < 940px. Fjernes helt ved print (kun main udskrives).
* **Relaterede komponenter:** `NavDrawer`, `MobileHeader`.

##### 2. `MobileHeader`
* **Dokumentnavn:** `MgpMobileHeader`
* **Formål / Usage:** Sticky topbar på mobilskærme (< 940px) til titel, tilbagevej og skuffeudløser.
* **Variants:** `standard`, `with-overflow`.
* **States:** `sticky`.
* **Tokens:** `--mgp-mobile-header-height` (56px), `--mgp-surface`, `--mgp-border`.
* **Accessibility:** Touch target for menu-knap min. 44×44px.
* **Mobile & Print:** Kører udelukkende på mobil/tablet. Skjules ved print.
* **Relaterede komponenter:** `NavDrawer`, `ContextBackButton`.

##### 3. `DetailPage`
* **Dokumentnavn:** `MgpDetailPage`
* **Formål / Usage:** Skabelon for entiteters detaljevisninger med support for staged loading og grid-layout.
* **Variants:** `standard` (main + sidebar), `full-width`.
* **States:** `loading` (staged), `loaded`, `restricted`.
* **Tokens:** `--mgp-space-lg`, `--mgp-border`.
* **Accessibility:** Korrekt h1->h2 overskriftshierarki og logisk fokus-rækkefølge.
* **Mobile & Print:** Foldes sammen til 1 kolonne på mobil. Konverteres til fladt A4-dokument ved print.
* **Relaterede komponenter:** `DetailHeader`, `SummaryGrid`, `CollapsibleSectionCard`, `DangerZone`.

##### 4. `DetailHeader`
* **Dokumentnavn:** `MgpDetailHeader`
* **Formål / Usage:** Overskriftsektion på detaljesider med titel, metadata, status-badges og 1–3 primære handlinger.
* **Variants:** `standard`.
* **States:** `default`, `archived`.
* **Tokens:** `--mgp-text`, `--mgp-text-muted`, `--mgp-border`.
* **Accessibility:** Indeholder sidens primære `<h1>`.
* **Mobile & Print:** Knapper udvides og stabeles vertikalt på mobil. Handlingsknapper skjules ved print.
* **Relaterede komponenter:** `DetailPage`, `ContextTabs`, `StatusMessage`.

##### 5. `SummaryGrid`
* **Dokumentnavn:** `MgpSummaryGrid`
* **Formål / Usage:** Raster-container til 3–5 fremhævede nøgletal og statusser øverst på detaljesider.
* **Variants:** `responsive-fit`.
* **States:** `default`.
* **Tokens:** `--mgp-summary-card-min-width` (140px), `--mgp-space-sm`.
* **Accessibility:** Semantisk grupperet.
* **Mobile & Print:** Stackes vertikalt på mobil. Udskrives som ren opstilling uden skygger.
* **Relaterede komponenter:** `SummaryCard`, `DetailPage`.

##### 6. `CollapsibleSectionCard`
* **Dokumentnavn:** `MgpCollapsibleSectionCard`
* **Formål / Usage:** Sammenfoldeligt sidekort til sekundært indhold på detaljesider (fx medlemmer, aktivitet).
* **Variants:** `default`, `attention-forced-open`.
* **States:** `collapsed` (med resumé i overskrift), `expanded`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-surface-muted`.
* **Accessibility:** Header-knap med `aria-expanded="true|false"`. Kritiske fejl/advarsler må aldrig foldes sammen.
* **Mobile & Print:** Starter collapsed på mobil medmindre opmærksomhed kræves. Foldes ud ved print eller skjules efter behov.
* **Relaterede komponenter:** `AttentionSummary`, `DetailPage`.

##### 7. `DangerZone`
* **Dokumentnavn:** `MgpDangerZone`
* **Formål / Usage:** Isolerede, afgrænsede sektions-cards på indstillingssider til destruktive og uafvendelige handlinger.
* **Variants:** `collapsible` (overskrift: "Arkivering og sletning").
* **States:** `collapsed`, `expanded`.
* **Tokens:** `--mgp-danger-border`, `--mgp-danger-bg`, `--mgp-danger`.
* **Accessibility:** Må aldrig benytte compact density. Handlingsknapper kræver eksplicit advarselstekst.
* **Mobile & Print:** Fuld bredde knapper på mobil. Skjules helt ved print.
* **Relaterede komponenter:** `ConfirmDialog`, `Button`.

##### 8. `FormSection`
* **Dokumentnavn:** `MgpFormSection`
* **Formål / Usage:** Visuel opdeling af lange formularer i tematisk afgrænsede paneler.
* **Variants:** `standard`, `error` (rød venstre-streg ved valideringsfejl).
* **States:** `default`, `invalid`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-danger`.
* **Accessibility:** Forbundet med formularvalidering.
* **Mobile & Print:** 1-kolonne layout på mobil. Skjuler formularknapper ved print.
* **Relaterede komponenter:** `FormField`, `FormActions`.

##### 9. `FilterBar`
* **Dokumentnavn:** `MgpFilterBar`
* **Formål / Usage:** Værktøjslinje over lister til fritekstsøgning, filterknapper, sortering og view-mode skift.
* **Variants:** `inline` (desktop), `drawer-trigger` (mobil).
* **States:** `default`, `active-filters`.
* **Tokens:** `--mgp-surface`, `--mgp-border`.
* **Accessibility:** Tastaturfokus på alle kontrolelementer.
* **Mobile & Print:** Omdannes på mobil til en knap, der åbner `FilterDrawer`. Skjules ved print.
* **Relaterede komponenter:** `SearchInput`, `SortSelect`, `ViewModeToggle`, `FilterDrawer`.

##### 10. `ActiveFilterSummary`
* **Dokumentnavn:** `MgpActiveFilterSummary`
* **Formål / Usage:** Bånd der opsamler aktive filterchips med mulighed for nulstilling.
* **Variants:** `standard`.
* **States:** `visible` (når filtre er aktive), `hidden`.
* **Tokens:** `--mgp-primary-soft`, `--mgp-primary-dark`.
* **Accessibility:** Annoncerer filtre for skærmlæsere.
* **Mobile & Print:** Wrapper linjer pænt på mobil. Skjules ved print.
* **Relaterede komponenter:** `FilterChip`, `FilterBar`.

##### 11. `PublicLayout`
* **Dokumentnavn:** `PublicLayout`
* **Formål / Usage:** Layout-shell for offentligt tilgængelige sider (uden for login/dashboard-kontekst).
* **Variants:** `standard`, `centered-auth` (til login/register).
* **States:** `desktop`, `mobile` (off-canvas navigation).
* **Tokens:** `--mgp-bg`, `--mgp-surface`, `--mgp-border`, `--mgp-mobile-header-height` (56px).
* **Accessibility:** Indeholder `main`-landemærke og "Spring til hovedindhold"-link (`.sr-only`).
* **Mobile & Print:** Skifter til sticky mobile header + `NavDrawer` på skærme < 940px. Skjuler navigation ved print.
* **Relaterede komponenter:** `PublicHeader`, `PublicFooter`, `NavDrawer`.

##### 12. `PublicHeader`
* **Dokumentnavn:** `PublicHeader`
* **Formål / Usage:** Topbar med logo, primær offentlig navigation, skift af abonnementer, og direkte "Log ind / Opret"-knapper.
* **Variants:** `sticky`.
* **States:** `default`, `scrolled`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-primary-dark`, `--mgp-touch-target-min`.
* **Accessibility:** Tastaturnavigation med synlig focus ring. Touch targets min 44px.
* **Mobile & Print:** Skifter til hamburger-menu for `NavDrawer` på mobil.
* **Relaterede komponenter:** `PublicLayout`, `Button`, `NavDrawer`.

##### 13. `PublicFooter`
* **Dokumentnavn:** `PublicFooter`
* **Formål / Usage:** Offentlig bundsektion med ophavsret, hurtige links, juridisk info og abonnement-genveje.
* **Variants:** `standard`.
* **States:** `default`.
* **Tokens:** `--mgp-surface-muted`, `--mgp-text`, `--mgp-text-muted`, `--mgp-border`.
* **Accessibility:** Semantisk `<footer>`-element med strukturerede linklister (`<ul>`).
* **Mobile & Print:** Stabeles i 1 kolonne på mobil. Udskrives som simpel tekst.
* **Relaterede komponenter:** `PublicLayout`.

---

#### C. Feedback Components

##### 1. `StatusMessage`
* **Dokumentnavn:** `MgpStatusMessage`
* **Formål / Usage:** Standard for persistent, kontekstuel feedback på handlinger, valideringsfejl, advarsler og adgangsbegrænsninger.
* **Variants:** `success`, `info`, `warning`, `danger`, `processing`, `restricted`.
* **Scopes:** `page`, `section`, `object`, `form`.
* **States:** `visible`, `dismissed`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-primary`, `--mgp-accent`, `--mgp-danger`.
* **Accessibility:** Anvender `aria-live="polite"` (eller `role="alert"` ved kritiske fejl). Må **ikke** benytte compact density på en måde, der skjuler forklarende tekst.
* **Mobile & Print:** Stabeles vertikalt på mobil. Udskrives hvis beskeden har dokumentationsværdi.
* **Relaterede komponenter:** `StatusBanner`, `Toast`, `EmptyState`.

##### 2. `StatusBanner` / `GlobalBanner`
* **Dokumentnavn:** `MgpStatusBanner`, `MgpGlobalBanner`
* **Formål / Usage:** Tværgående advarsels- eller informationslinje placeret øverst på siden eller under hoved-headeren (fx systemvedligehold eller abonnement).
* **Variants:** `info`, `warning`, `danger`.
* **States:** `persistent`.
* **Tokens:** `--mgp-accent`, `--mgp-danger`, `--mgp-surface`.
* **Accessibility:** Høj kontrast og tydelig tekst.
* **Mobile & Print:** Placeres øverst på mobil. Skjules ved print medmindre kritisk.
* **Relaterede komponenter:** `StatusMessage`.

##### 3. `Toast` / `ToastContainer`
* **Dokumentnavn:** `MgpToast`, `MgpToastContainer`
* **Formål / Usage:** Transient, ikke-kritisk bekræftelse af globale handlinger (fx "Link kopieret"). Må **aldrig** indeholde fejl eller adgangsbegrænsninger.
* **Variants:** `neutral`, `success`.
* **States:** `entering`, `visible`, `exiting`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--shadow-md`.
* **Accessibility:** Autodismiss efter 3–5 sek. Skal kunne lukkes manuelt med tastatur.
* **Mobile & Print:** Placeres nederst i fuld bredde på mobil uden at overlappe bunden. **Skjules altid ved print** (`display: none !important`).
* **Relaterede komponenter:** `StatusMessage`.

##### 4. `ObjectStatus`
* **Dokumentnavn:** `MgpObjectStatus`
* **Formål / Usage:** Kompakt tilstands- og behandlingsindikator placeret direkte i/på et specific kort eller en række (fx "Thumbnail oprettes...").
* **Variants:** `processing`, `info`, `warning`.
* **States:** `active`.
* **Tokens:** `--mgp-primary-dark`, `--mgp-text-muted`.
* **Accessibility:** Ledsaget af tekst eller tooltip.
* **Mobile & Print:** Forbliver kompakt.
* **Relaterede komponenter:** `StatusMessage`, `Card`.

##### 5. `EmptyState`
* **Dokumentnavn:** `MgpEmptyState`
* **Formål / Usage:** Vises når en side, sektion eller søgning mangler data. Guider til næste handling.
* **Variants:** `first-use`, `context`, `filtered`, `search`, `restricted`, `processing`, `error`.
* **States:** `empty` (standard tom visning), `error` (fejltilstand), `processing` (behandlingstilstand). `full-page`, `inline`.
* **Tokens:** `--mgp-surface`, `--mgp-surface-muted`, `--mgp-border` (dashed), `--mgp-primary-soft`, `--mgp-danger-bg`.
* **Accessibility:** Svarer på: Hvad er tomt? Hvorfor? Hvad er næste skridt? Indeholder primær handlingsknap.
* **Mobile & Print:** Reducerer padding på mobil. Skjules ved print.
* **Relaterede komponenter:** `StatusMessage`, `Button`.

##### 6. `UndoStatus`
* **Dokumentnavn:** `MgpUndoStatus`
* **Formål / Usage:** Transient besked med integreret "Fortryd"-knap til reversible relationshandlinger (fx "Plante fjernet fra bed").
* **Variants:** `standard`.
* **States:** `active` (tidstager kører).
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-primary`.
* **Accessibility:** Tastaturfokuserbar "Fortryd"-knap.
* **Mobile & Print:** Nederst på mobil. Skjules ved print.
* **Relaterede komponenter:** `StatusMessage`, `InlineConfirm`.

##### 7. `PermissionHint`
* **Dokumentnavn:** `MgpPermissionHint`
* **Formål / Usage:** Kompakt forklarende tekst under deaktiverede knapper eller felter om manglende adgang.
* **Variants:** `inline`.
* **States:** `visible`.
* **Tokens:** `--mgp-text-muted`.
* **Accessibility:** Forbundet til den deaktiverede knap via `aria-describedby`.
* **Mobile & Print:** Synlig på mobil. Skjules ved print.
* **Relaterede komponenter:** `Button`, `PermissionGate`.

---

#### D. Data Display Components

##### 1. `Card`
* **Dokumentnavn:** `MgpCard`, `MgpEntityCard`
* **Formål / Usage:** Generisk container til browsing og overblik over entiteter (1–20 enheder).
* **Variants:** `entity`, `action`, `attention`, `archived`, `restricted`, `compact`.
* **States:** `default`, `hover`, `focus-within`, `selected`, `archived` (opacity 0.90).
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--shadow-sm`, `--mgp-accent`, `--mgp-surface-muted`.
* **Accessibility:** Ved klikbare kort benyttes et ægte `<a>`-tag (`.card-clickable`).
* **Mobile & Print:** Enkeltkolonne på mobil. Udskrives som flad boks uden skygger.
* **Relaterede komponenter:** `PlantCard`, `MediaCard`, `CompactEntityRow`.

##### 2. `CompactEntityRow`
* **Dokumentnavn:** `MgpCompactEntityRow`
* **Formål / Usage:** Pladseffektiv rækkevisning til lange lister (20+ enheder), søgeresultater og kompakte oversigter.
* **Variants:** `standard`.
* **States:** `default`, `hover`, `selected`.
* **Tokens:** `--mgp-density-compact`, `--mgp-surface`, `--mgp-border`, `--mgp-surface-muted`.
* **Accessibility:** Tastatur-fokusérbar med synlig markeringskant.
* **Mobile & Print:** Omdannes til 2-linjers layout på mobil. Udskrives rent.
* **Relaterede komponenter:** `Card`, `DataTable`.

##### 3. `DataTable`
* **Dokumentnavn:** `MgpDataTable`
* **Formål / Usage:** Semantisk tabel til strukturerede data, sammenligning, målangivelser og materialelister.
* **Variants:** `default`, `compact`.
* **States:** `default`, `sorting`, `hover-row`.
* **Tokens:** `--mgp-surface`, `--mgp-surface-muted`, `--mgp-border`, `--mgp-density-compact`.
* **Accessibility:** Anvender semantiske `<table>`, `<thead>`, `<th>`, `<tbody>` tags.
* **Mobile & Print:** Omdannes til stacked rows på mobil (`.data-table-responsive-stacked`). Udskrives som flad `.print-table`.
* **Relaterede komponenter:** `PrintTable`, `CompactEntityRow`.

##### 4. `ThumbnailGrid`
* **Dokumentnavn:** `MgpThumbnailGrid`
* **Formål / Usage:** Visuelt galleri-raster til tegninger, skitser og fotoarkiver.
* **Variants:** `standard`.
* **States:** `loading` (skeleton grid), `loaded`.
* **Tokens:** `--mgp-space-md`, `--mgp-surface`.
* **Accessibility:** Billeder forsynes med alt-tekster eller filnavne.
* **Mobile & Print:** Færre kolonner på mobil (min 140px pr. kort). Udskrives pænt opstillet.
* **Relaterede komponenter:** `Thumbnail`, `MediaCard`.

##### 5. `PrintTable`
* **Dokumentnavn:** `MgpPrintTable`
* **Formål / Usage:** Flad, papiroptimeret monokrom tabel til print-udskrifter af kort og lister.
* **Variants:** `document-compact`.
* **States:** `print-only`.
* **Tokens:** `--mgp-border` (`#cccccc`), `--mgp-text` (`#111111`).
* **Accessibility:** N/A (fysisk papir/PDF).
* **Mobile & Print:** Kun synlig under `@media print`.
* **Relaterede komponenter:** `DataTable`, `PrintScope`.

##### 6. `SummaryCard`
* **Dokumentnavn:** `MgpSummaryCard`
* **Formål / Usage:** Enkelt nøgletalskort til brug i `SummaryGrid` (fx "Areal: 45 m²", "Planter: 12").
* **Variants:** `standard`, `attention`.
* **States:** `default`, `has-attention`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-text-muted`.
* **Accessibility:** Tydelig label-value struktur.
* **Mobile & Print:** Fuld bredde på mobil hvis nødvendigt. Flad boks ved print.
* **Relaterede komponenter:** `SummaryGrid`.

##### 7. `MediaPreview` / `FullViewer`
* **Dokumentnavn:** `MgpMediaPreview`, `MgpFullViewer`
* **Formål / Usage:** Sidepanel eller full-screen overlay til forhåndsvisning af dokumenter og tegninger uden download af originalfil.
* **Variants:** `side-panel` (desktop), `full-screen` (mobil/tegninger).
* **States:** `open`, `closed`, `loading`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--shadow-lg`.
* **Accessibility:** Focus trap i overlay. Lukkes ved Esc-tast.
* **Mobile & Print:** Åbner altid i full-screen på mobil. Skjules ved print.
* **Relaterede komponenter:** `Thumbnail`, `MediaCard`.

##### 8. `ReadOnlySection`
* **Dokumentnavn:** `MgpReadOnlySection`
* **Formål / Usage:** Viser formulardata som ren tekstrapport for brugere uden redigeringsadgang (erstatter deaktiverede inputfelter).
* **Variants:** `standard`.
* **States:** `read-only`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-text-muted`.
* **Accessibility:** Høj læsbarhed uden formularkontrolder-støj.
* **Mobile & Print:** Pænt opstillet på mobil. Perfekt til udskrift.
* **Relaterede komponenter:** `FormField`, `PermissionGate`.

---

#### E. Interaction Components

##### 1. `ConfirmDialog`
* **Dokumentnavn:** `MgpConfirmDialog`
* **Formål / Usage:** Modal bekræftelsesdialog ved destruktive handlinger, arkivering og sletning.
* **Variants:** `standard`, `strong-confirmation` (kræver indtastning af navnematch).
* **States:** `open`, `closed`, `submitting` (loading spinner på danger-knap).
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--shadow-lg`, `--mgp-danger`.
* **Accessibility:** Må **aldrig** benytte compact density. Focus trap installeret; returnerer fokus til udløser ved luk.
* **Mobile & Print:** Full-width vertikale knapper i bunden på mobil. Skjules ved print.
* **Relaterede komponenter:** `InlineConfirm`, `DangerZone`, `Button`.

##### 2. `InlineConfirm`
* **Dokumentnavn:** `MgpInlineConfirm`
* **Formål / Usage:** Kompakt popover-bekræftelse direkte i kort eller tabelrækker ved mindre destruktive handlinger.
* **Variants:** `popover`.
* **States:** `active`, `inactive`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--shadow-sm`.
* **Accessibility:** Undgår spærrende modal.
* **Mobile & Print:** Tilpasses skærmbredden. Skjules ved print.
* **Relaterede komponenter:** `ConfirmDialog`, `UndoStatus`.

##### 3. `SearchInput`
* **Dokumentnavn:** `MgpSearchInput`
* **Formål / Usage:** Fritekstsøgefelt med indbygget debounce (300ms), loader og slette-knap (`×`).
* **Variants:** `standard`.
* **States:** `empty`, `typing`/`debouncing`, `has-value`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-primary`, `--mgp-text-muted`.
* **Accessibility:** Forbundet med `aria-label="Søg..."`. Sletteknap har `aria-label="Ryd søgning"`.
* **Mobile & Print:** Fuld bredde på mobil. Skjules ved print.
* **Relaterede komponenter:** `FilterBar`, `SortSelect`.

##### 4. `SortSelect`
* **Dokumentnavn:** `MgpSortSelect`
* **Formål / Usage:** Dropdown-komponent til valg af sorteringsfelt og retning (fx "Navn A–Å").
* **Variants:** `standard`.
* **States:** `default`.
* **Tokens:** `--mgp-surface`, `--mgp-border`.
* **Accessibility:** Standard HTML `<select>` for høj mobil- og skærmlæserstøtte.
* **Mobile & Print:** Fuld bredde i mobil filter drawer. Skjules ved print.
* **Relaterede komponenter:** `FilterBar`, `SearchInput`.

##### 5. `ViewModeToggle`
* **Dokumentnavn:** `MgpViewModeToggle`
* **Formål / Usage:** Knapgruppe til at skifte præsentationsform (`cards`, `compact`, `table`, `grid`).
* **Variants:** `icon-group`.
* **States:** `active-mode` pr. knap.
* **Tokens:** `--mgp-surface-muted`, `--mgp-surface`, `--mgp-primary-dark`.
* **Accessibility:** `aria-pressed="true|false"` på hver tilstandsknap.
* **Mobile & Print:** Skjules eller begrænses på mobil ift. tilgængelige visninger. Skjules ved print.
* **Relaterede komponenter:** `FilterBar`, `DataTable`, `ThumbnailGrid`.

##### 6. `FilterChip`
* **Dokumentnavn:** `MgpFilterChip`
* **Formål / Usage:** Visuel chip, der repræsenterer et aktivt valgt filter med en sletteknap (`×`).
* **Variants:** `standard`.
* **States:** `default`, `hover`.
* **Tokens:** `--mgp-primary-soft`, `--mgp-primary-dark`.
* **Accessibility:** Lukkeknap har eksplicit `aria-label="Fjern filter [navn]"`.
* **Mobile & Print:** Flex-wrapping på mobil. Skjules ved print.
* **Relaterede komponenter:** `ActiveFilterSummary`, `FilterBar`.

##### 7. `FilterDrawer`
* **Dokumentnavn:** `MgpFilterDrawer`
* **Formål / Usage:** Bottom-sheet / interaction drawer på mobil (< 640px) til samling af komplekse filtre og sortering.
* **Variants:** `bottom-sheet`.
* **States:** `open`, `closed`.
* **Tokens:** `--mgp-surface`, `--shadow-lg`, `--mgp-touch-target-min`.
* **Accessibility:** Focus trap installeret. Lukkes med Esc eller "Anvend"-knap.
* **Mobile & Print:** Kører kun på mobil/tablet. Skjules ved print.
* **Relaterede komponenter:** `FilterBar`, `NavDrawer`.

##### 8. `NavDrawer`
* **Dokumentnavn:** `NavDrawer`
* **Formål / Usage:** Global off-canvas navigationsskuffe på mobil/tablet (< 940px).
* **Variants:** `left-drawer`.
* **States:** `open`, `closed`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--shadow-lg`.
* **Accessibility:** Focus trap og restore. Togglshåndtag har `aria-expanded`.
* **Mobile & Print:** Kører kun på mobil/tablet. Skjules ved print.
* **Relaterede komponenter:** `AppShell`, `NavMenu`, `MobileHeader`.

##### 9. `NavMenu`
* **Dokumentnavn:** `NavMenu`
* **Formål / Usage:** Genanvendelig navigationsliste (deles mellem desktop sidebar og mobile `NavDrawer`).
* **Variants:** `standard`, `collapsed` (nav rail).
* **States:** `active-link`, `hover`.
* **Tokens:** `--mgp-primary-soft`, `--mgp-primary-dark`, `--mgp-text-muted`.
* **Accessibility:** Semantisk `<nav>` med uorganiseret liste `<ul>`.
* **Mobile & Print:** Vertikal liste. Skjules ved print.
* **Relaterede komponenter:** `AppShell`, `NavDrawer`, `NavBadge`.

##### 10. `ContextTabs`
* **Dokumentnavn:** `MgpContextTabs`
* **Formål / Usage:** Lokal kontekstnavigation øverst på detaljesider, altid bundet til URL'ens `?tab=...` parameter.
* **Variants:** `pill-tabs`.
* **States:** `active-tab`, `default`.
* **Tokens:** `--mgp-primary-soft`, `--mgp-primary-dark`, `--mgp-border`.
* **Accessibility:** Tastaturnavigation med piltaster.
* **Mobile & Print:** Horisontalt scrollbar uden linjeskift (`overflow-x: auto`) på mobil. Skjules ved print.
* **Relaterede komponenter:** `DetailHeader`, `DetailPage`.

##### 11. `ContextBackButton`
* **Dokumentnavn:** `ContextBackButton`
* **Formål / Usage:** Kontekstuel tilbageknap med dynamisk label baseret på `returnUrl` (fx "← Tilbage til plantelisten").
* **Variants:** `text-link`.
* **States:** `default`, `hover`.
* **Tokens:** `--mgp-text-muted`, `--mgp-primary-dark`.
* **Accessibility:** Tydelig linktekst.
* **Mobile & Print:** Placeres øverst på mobil. Skjules ved print.
* **Relaterede komponenter:** `DetailHeader`, `DetailPage`.

##### 12. `UploadZone`
* **Dokumentnavn:** `MgpUploadZone`
* **Formål / Usage:** Interaktiv zone til filupload med drag-and-drop og eksplicit filvælgerknap.
* **Variants:** `standard`.
* **States:** `empty`, `drag-over`, `uploading` (progress bar), `success`, `error`.
* **Tokens:** `--mgp-surface`, `--mgp-primary-soft`, `--mgp-border` (dashed), `--mgp-danger`.
* **Accessibility:** Skal altid indeholde en synlig, fokusérbar `<button>` ("Vælg fil"). Drag-and-drop må ikke stå alene.
* **Mobile & Print:** Stabeles i 1 kolonne på mobil. Skjules ved print.
* **Relaterede komponenter:** `MediaCard`, `Button`.

##### 13. `FormField`
* **Dokumentnavn:** `MgpFormField`
* **Formål / Usage:** Tre-lags feltwrapper der styrer label, required/optional indikator, control-slot og reserveret plads til fejl/hjælpetekst.
* **Variants:** `standard`, `error`.
* **States:** `default`, `focus`, `error` (ugyldig/valideringsfejl).
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-primary`, `--mgp-danger`, `--mgp-focus-ring`.
* **Accessibility:** Eksplicit kobling mellem `<label for>` og input `id` samt `aria-describedby` til fejltekst.
* **Mobile & Print:** 100% bredde og min 44px input-højde på mobil. Udskrives som ren tekst.
* **Relaterede komponenter:** `FormSection`, `FormActions`.

##### 14. `ButtonAligned`
* **Dokumentnavn:** `MgpButtonAligned`
* **Formål / Usage:** Wrapper til knapper placeret i et form-grid ved siden af andre felter, så knappen flugter med feltets input i stedet for dets label.
* **Variants:** `standard`.
* **States:** `default`.
* **Tokens:** N/A (Layout utility).
* **Accessibility:** Bevarer rigtig tab-rækkefølge.
* **Mobile & Print:** Stabeles naturligt på mobil.
* **Relaterede komponenter:** `Button`, `FormField`.

---

#### F. Domain Components

##### 1. `PlantCard`
* **Dokumentnavn:** `MgpPlantCard`
* **Formål / Usage:** Specialiseret enhedskort til botanisk visning med latinsk navn, vækstbetingelser og chips.
* **Variants:** `standard`.
* **States:** `default`, `hover`, `selected`.
* **Tokens:** Konsumerer `Card` tokens + `--mgp-text-muted` (italic latin name).
* **Accessibility:** Latinske navne opmærkes med `.latin-name` (`<i>`).
* **Mobile & Print:** 1 kolonne på mobil. Monokrom ved print.
* **Relaterede komponenter:** `Card`, `PlantName`.

##### 2. `MediaCard`
* **Dokumentnavn:** `MgpMediaCard`
* **Formål / Usage:** Fil- og dokumentkort med thumbnail-preview, levetidsbadges og adgangskontrol.
* **Variants:** `previewable`, `actions-only`, `restricted`.
* **States:** `default`, `hover`, `processing` (thumbnail oprettes), `restricted` (låst download).
* **Tokens:** `--mgp-surface`, `--mgp-surface-muted`, `--mgp-border`, `--mgp-accent`.
* **Accessibility:** Deaktiverede handlinger ved `restricted` forklares med `.permission-hint`.
* **Mobile & Print:** Sammenfoldes til 1 kolonne på mobil. Udskrives uden interaktive knapper.
* **Relaterede komponenter:** `Thumbnail`, `MediaPreview`, `FileBadge`.

##### 3. `PlantName`
* **Dokumentnavn:** `MgpPlantName`
* **Formål / Usage:** Domænekomponent til konsistent visning af dansk og latinsk plantenavn.
* **Variants:** `standard`.
* **States:** `default`.
* **Tokens:** `--mgp-text`, `--mgp-text-muted`.
* **Accessibility:** Korrekt typografisk adskillelse for skærmlæsere.
* **Mobile & Print:** Bevares intakt.
* **Relaterede komponenter:** `PlantCard`.

##### 4. `PermissionGate`
* **Dokumentnavn:** `MgpPermissionGate`
* **Formål / Usage:** Dekoration/wrapper der betinget viser, deaktiverer eller skjuler UI-sektioner baseret på brugerens rolle/permissions.
* **Variants:** `hide` (skjuler helt), `disable` (deaktiverer med hint), `read-only` (viser som `ReadOnlySection`).
* **States:** `authorized`, `unauthorized`.
* **Tokens:** N/A (Logisk komponent).
* **Accessibility:** Forhindrer blindgyder ved at forklare begrænsninger før handling.
* **Mobile & Print:** Reagerer identisk på tværs af enheder.
* **Relaterede komponenter:** `RoleBadge`, `PermissionHint`, `ReadOnlySection`.

##### 5. `RoleBadge`
* **Dokumentnavn:** `MgpRoleBadge`
* **Formål / Usage:** Badge til visning af menneskeligt forståelige roller (fx "Ejer", "Redaktør", "Læser") og invitationsstatusser.
* **Variants:** `primary` (Ejer), `secondary` (Redaktør), `muted` (Læser/Kunde), `accent` (Afventer), `danger-soft` (Udløbet).
* **States:** `default`.
* **Tokens:** `--mgp-surface-muted`, `--mgp-primary-soft`, `--mgp-accent`, `--mgp-danger-bg`.
* **Accessibility:** Oversætter interne enums til dansk klartekst.
* **Mobile & Print:** Bevares synlig i udskrifter.
* **Relaterede komponenter:** `Badge`, `PermissionGate`.

##### 6. `ArchivedBadge`
* **Dokumentnavn:** `MgpArchivedBadge`
* **Formål / Usage:** Indikerer at en entitet er arkiveret (historisk gemt).
* **Variants:** `neutral-archived`.
* **States:** `default`.
* **Tokens:** `--mgp-state-archived` (`#8A8F86`), `--mgp-surface`, `--mgp-border`.
* **Accessibility:** Anvender aldrig fejl- eller advarselsfarver (rød/orange).
* **Mobile & Print:** Skrives ud i dokumentationsprint for at bevare historisk overblik.
* **Relaterede komponenter:** `Card`, `ArchiveBanner`.

##### 7. `AttentionBadge`
* **Dokumentnavn:** `MgpAttentionBadge`
* **Formål / Usage:** Badge til fremvisning af Attention Level 1–3 tilstande (fx "Udløber snart", "Afventer svar").
* **Variants:** `level-1` (muted), `level-2` (accent/orange), `level-3` (danger/rød).
* **States:** `default`.
* **Tokens:** `--mgp-primary-soft`, `--mgp-accent`, `--mgp-danger`.
* **Accessibility:** Ledsages af status-dot og eksplicit tekst.
* **Mobile & Print:** Synlig på mobil og print.
* **Relaterede komponenter:** `AttentionSummary`, `StatusMessage`.

##### 8. `AttentionSummary`
* **Dokumentnavn:** `MgpAttentionSummary`
* **Formål / Usage:** Aggregeret resuménotat i sektionsheaders (fx *"Medlemmer · 2 aktive · 1 afventer"*).
* **Variants:** `standard`.
* **States:** `has-attention` (Level 2/3 highlight).
* **Tokens:** `--mgp-accent`, `--mgp-text-muted`.
* **Accessibility:** Gør det muligt at forstå tilstand i sammenfoldede kort uden at åbne dem.
* **Mobile & Print:** Bevares i headeren.
* **Relaterede komponenter:** `CollapsibleSectionCard`, `AttentionBadge`.

##### 9. `FileBadge`
* **Dokumentnavn:** `MgpFileBadge`
* **Formål / Usage:** Viser filstatus, levetid (`Temporary`, `Permanent`) og udløb.
* **Variants:** `temporary` (accent), `permanent` (muted), `expired` (danger-soft).
* **States:** `default`.
* **Tokens:** `--mgp-accent`, `--mgp-surface-muted`, `--mgp-danger`.
* **Accessibility:** Klar tekstangivelse.
* **Mobile & Print:** Vises på filkort.
* **Relaterede komponenter:** `MediaCard`, `Thumbnail`.

##### 10. `InvitationStatusCard`
* **Dokumentnavn:** `MgpInvitationStatusCard`
* **Formål / Usage:** Dedikeret række/kort til visning af adgangsinvitationer med e-mail, rolle, udløb og tilbagekaldelses handling.
* **Variants:** `standard`.
* **States:** `pending`, `accepted`, `expired`, `revoked`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-accent`.
* **Accessibility:** Indeholder handlingsknap ("Tilbagekald invitation") med klar advarsel.
* **Mobile & Print:** Stackes på mobil. Skjules ved eksterne print.
* **Relaterede komponenter:** `RoleBadge`, `ConfirmDialog`.

##### 11. `PricingCard`
* **Dokumentnavn:** `PricingCard`
* **Formål / Usage:** Præsentation af et enkelt abonnementsniveau hentet fra databasen (f.eks. Free, Havenørd, Pro-Gartner).
* **Variants:** `standard`, `featured` (fremhævet med accent-border og badge), `compact`.
* **States:** `default`, `hover`, `active-plan` (hvis brugeren allerede har abonnementet).
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-primary`, `--mgp-accent`, `--mgp-surface-muted`, `--shadow-md`.
* **Accessibility:** Tydelig prisoppstilling (`.numeric`) og eksplicit handlingsknap ("Vælg abonnement").
* **Mobile & Print:** Fuld bredde på mobil.
* **Relaterede komponenter:** `PricingFeatureMatrix`, `Badge`, `Button`.

##### 12. `PricingFeatureMatrix`
* **Dokumentnavn:** `PricingFeatureMatrix`
* **Formål / Usage:** Detaljeret sammenligningstabel over funktioner pr. abonnementstype (hentes fra DB).
* **Variants:** `table`.
* **States:** `default`, `loading`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-surface-muted`, `--mgp-primary-dark`.
* **Accessibility:** Semantisk `<table>` struktur med `scope="col"` og `scope="row"`.
* **Mobile & Print:** Konverteres til stacked kort pr. tier på mobilskærme (< 768px).
* **Relaterede komponenter:** `DataTable`, `PricingCard`, `Badge`.

##### 13. `HeroBanner`
* **Dokumentnavn:** `HeroBanner`
* **Formål / Usage:** Hovedfokussektion på landing page med overskrift, underoverskrift, opfordring til handling (CTA) og visuel illustration.
* **Variants:** `standard`, `split-media`.
* **States:** `default`.
* **Tokens:** `--mgp-primary-soft`, `--mgp-primary-dark`, `--mgp-text`, `--mgp-space-2xl`.
* **Accessibility:** Indeholder sidens `<h1>`. Primær knap benytter `.btn-primary`.
* **Mobile & Print:** Tekst og CTA stabeles vertikalt over illustration på mobil.
* **Relaterede komponenter:** `Button`, `Stack`.

##### 14. `FeatureCard`
* **Dokumentnavn:** `FeatureCard`
* **Formål / Usage:** Præsentation af platformens nøglefunktioner (Layer 1 Master Data, Layer 2 Design, Layer 3 Eksekvering).
* **Variants:** `standard`, `icon-top`.
* **States:** `default`, `hover`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--shadow-sm`.
* **Accessibility:** Strukturerede overskrifter (`<h3>`).
* **Mobile & Print:** 1 kolonne på mobil.
* **Relaterede komponenter:** `Card`, `Stack`.

##### 15. `PricingMatrixTable`
* **Dokumentnavn:** `PricingMatrixTable`
* **Formål / Usage:** Visning af basistakster pr. bruger baseret på Niveau (Lag 1-3) og Adgangskategori (Admin, Editor, Viewer+, Viewer) med cyklus-velger (Årlig, Månedlig, Perpetual).
* **Variants:** `read-only`, `interactive-select`.
* **States:** `default`, `loading`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-primary-soft`, `--mgp-density-compact`.
* **Accessibility:** Semantisk `<table>` med `scope="col"` og `scope="row"`.
* **Mobile & Print:** Skifter til harmonika/kortudgave pr. lag på skærme < 768px.
* **Relaterede komponenter:** `DataTable`, `PricingCalculator`, `Badge`.

##### 16. `GardenVolumeDiscountTable`
* **Dokumentnavn:** `GardenVolumeDiscountTable`
* **Formål / Usage:** Præsentation af volumintrappen for antal haver samt forklarende regler for arkiverede havers vægtning (0,25 have for Admin vs. 1,0 for øvrige).
* **Variants:** `standard`, `compact`.
* **States:** `default`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-accent`, `--mgp-surface-muted`.
* **Accessibility:** Tydelige procentangivelser med `.numeric` klassen.
* **Mobile & Print:** Stabeles som compact rows på mobil.
* **Relaterede komponenter:** `DataTable`, `PricingCalculator`, `PermissionHint`.

##### 17. `SubscriptionAddOnCard`
* **Dokumentnavn:** `SubscriptionAddOnCard`
* **Formål / Usage:** Præsentation af tilkøbsmoduler (Bedforslag, Bede, Artefaktpakke A & B) med angivelse af enhed, månedlig og årlig pris.
* **Variants:** `standard`, `selectable`.
* **States:** `default`, `hover`, `selected`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-primary`, `--shadow-sm`.
* **Accessibility:** Inkluderer synlig label og pris.
* **Mobile & Print:** Fuld bredde på mobil.
* **Relaterede komponenter:** `Card`, `PricingCalculator`, `Badge`.

##### 18. `PricingCalculator`
* **Dokumentnavn:** `PricingCalculator`
* **Formål / Usage:** Interaktiv beregner hvor brugeren kan sammensætte sin konfiguration (Lag, Rolle, Betalingsfrekvens, Aktive/Arkiverede haver samt Tilkøb) og se den beregnede samlede pris samt besparelse.
* **Variants:** `widget`, `full-page`.
* **States:** `calculating`, `ready`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-primary-soft`, `--mgp-primary-dark`, `--mgp-accent`.
* **Accessibility:** Formularfelter bundet op med ARIA-labels og live region for opdateret totalsum.
* **Mobile & Print:** 1-kolonne formular-flow på mobil.
* **Relaterede komponenter:** `FormField`, `PricingMatrixTable`, `SubscriptionAddOnCard`, `Button`.

##### 19. `BasePriceMatrixEditor`
* **Dokumentnavn:** `BasePriceMatrixEditor`
* **Formål / Usage:** Admin-komponent til inline redigering af priser pr. Niveau, Adgangskategori og Betalingsfrekvens.
* **Variants:** `editable-table`.
* **States:** `default`, `editing`, `saving`, `error`.
* **Tokens:** `--mgp-surface`, `--mgp-border`, `--mgp-danger`, `--mgp-focus-ring`.
* **Accessibility:** Hvert inputfelt har eksplicit `aria-label` bestående af Lag + Rolle + Frekvens.
* **Mobile & Print:** Kun tilgængelig på desktop/tablet i admin-panelet.
* **Relaterede komponenter:** `DataTable`, `FormField`, `Button`, `StatusMessage`.

---

## 2. Design Tokens Arkitektur

Design token-systemet i MyGardenPlanner er opbygget i **tre strengt adskilte lag**:

```text
┌───────────────────────────────────────────────────────────┐
│ 1. GLOBAL TOKENS (Primitiver: Hex, px, rem, raw values)  │
└─────────────────────────────┬─────────────────────────────┘
                              │
                              ▼
┌───────────────────────────────────────────────────────────┐
│ 2. SEMANTIC TOKENS (Intention: Surface, Text, States, WCAG)│
└─────────────────────────────┬─────────────────────────────┘
                              │
                              ▼
┌───────────────────────────────────────────────────────────┐
│ 3. COMPONENT TOKENS (Scoped CSS variables til komponenter)│
└───────────────────────────────────────────────────────────┘
```

---

### 2.1 Konsolideret Token-katalog

#### A. Color Tokens

```css
:root {
  /* --- 1. GLOBAL COLOR TOKENS --- */
  --mgp-color-green-500:   #3F6B4A; /* Primær grøn */
  --mgp-color-green-700:   #2F5138; /* Mørk grøn (hover) */
  --mgp-color-green-100:   #DDE8D8; /* Lys salvie */
  --mgp-color-terracotta:  #B86B4B; /* Terracotta accent */
  --mgp-color-charcoal:    #243128; /* Mørk tekstgrøn */
  --mgp-color-sand-100:    #FAF8F2; /* Varm sand baggrund */
  --mgp-color-sand-200:    #EFEAE0; /* Dæmpet flade */
  --mgp-color-sand-300:    #D8D2C7; /* Border farve */
  --mgp-color-white:       #FFFFFF;
  --mgp-color-gray-500:    #8A8F86; /* Arkiveret neutral */
  --mgp-color-gray-600:    #6F766D; /* Muted tekst */
  --mgp-color-red-600:     #9F3A38; /* Danger rød */
  --mgp-color-red-100:     #FFF4F3; /* Danger soft bg */

  /* --- 2. SEMANTIC COLOR TOKENS --- */
  --mgp-primary:           var(--mgp-color-green-500);
  --mgp-primary-dark:      var(--mgp-color-green-700);
  --mgp-primary-soft:      var(--mgp-color-green-100);
  --mgp-accent:            var(--mgp-color-terracotta);

  --mgp-bg:                var(--mgp-color-sand-100);
  --mgp-surface:           var(--mgp-color-white);
  --mgp-surface-muted:     var(--mgp-color-sand-200);

  --mgp-text:              var(--mgp-color-charcoal);
  --mgp-text-muted:        var(--mgp-color-gray-600);

  --mgp-border:            var(--mgp-color-sand-300);
  --mgp-border-hover:      rgba(63, 107, 74, 0.28);

  /* States & Feedback */
  --mgp-danger:            var(--mgp-color-red-600);
  --mgp-danger-bg:         var(--mgp-color-red-100);
  --mgp-danger-border:      rgba(159, 58, 56, 0.35);
  --mgp-danger-border-hover: rgba(159, 58, 56, 0.55);
  --mgp-warning-bg:        #F8E8D8;
  --mgp-success-bg:        #E4EFE1;
  --mgp-state-archived:    var(--mgp-color-gray-500);

  /* --- 3. COMPONENT COLOR TOKENS (Eksempler) --- */
  --mgp-button-primary-bg:   var(--mgp-primary);
  --mgp-button-primary-text: var(--mgp-surface);
  --mgp-card-bg:             var(--mgp-surface);
  --mgp-card-border:         var(--mgp-border);
}
```

#### B. Spacing & Layout Tokens

```css
:root {
  /* --- GLOBAL SPACING SCALE (4px/8px rhythm) --- */
  --mgp-space-2xs: 0.25rem; /* 4px */
  --mgp-space-xs:  0.5rem;  /* 8px */
  --mgp-space-sm:  0.75rem; /* 12px */
  --mgp-space-md:  1rem;    /* 16px */
  --mgp-space-lg:  1.5rem;  /* 24px */
  --mgp-space-xl:  2rem;    /* 32px */
  --mgp-space-2xl: 3rem;    /* 48px */
  --mgp-space-3xl: 4rem;    /* 64px */

  /* --- SEMANTIC LAYOUT TOKENS --- */
  --mgp-page-x: var(--mgp-space-lg);
  --mgp-page-y: var(--mgp-space-xl);
  --mgp-sidebar-width: 280px;
  --mgp-sidebar-collapsed-width: 72px;
  --mgp-mobile-header-height: 56px;
}

@media (max-width: 640px) {
  :root {
    --mgp-page-x: var(--mgp-space-md);
    --mgp-page-y: var(--mgp-space-lg);
  }
}
```

#### C. Radius & Shadow Tokens

```css
:root {
  /* Radius */
  --radius-sm: 4px;
  --radius-md: 8px;
  --radius-lg: 12px;
  --radius-pill: 999px;

  /* Shadows */
  --shadow-sm: 0 1px 3px rgba(36, 49, 40, 0.06);
  --shadow-md: 0 4px 12px rgba(36, 49, 40, 0.08);
  --shadow-lg: 0 12px 28px rgba(36, 49, 40, 0.12);
}
```

#### D. Typography Tokens

```css
:root {
  --mgp-font-sans: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
  --mgp-font-mono: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;

  --font-size-xs: 0.78rem;
  --font-size-sm: 0.9rem;
  --font-size-base: 1rem;
  --font-size-md: 1.08rem;
  --font-size-lg: 1.25rem;
  --font-size-xl: clamp(1.35rem, 2.2vw, 2rem);
  --font-size-xxl: clamp(2rem, 4vw, 3.25rem);

  --font-weight-normal: 400;
  --font-weight-medium: 500;
  --font-weight-bold: 750;
  --font-weight-heavy: 800;
}
```

#### E. Accessibility & Interaction Tokens

```css
:root {
  /* Touch target tilgængeligheds-baseline */
  --mgp-touch-target-min: 44px;

  /* Focus Ring */
  --mgp-focus-ring: rgba(63, 107, 74, 0.18);
  --mgp-focus-outline-width: 3px;
  --mgp-focus-outline-offset: 2px;
}
```

---

### 2.2 Data Density Overrides via Tokens

Density styres ved at tilsidesætte de scoped komponent-tokens via attributten `data-density="comfortable|default|compact"`:

```css
/* Dynamic Base Component Spacing Tokens */
:root {
  --mgp-card-padding: var(--mgp-space-md);
  --mgp-row-padding-y: var(--mgp-space-sm);
  --mgp-row-padding-x: var(--mgp-space-md);
  --mgp-section-gap: var(--mgp-space-lg);
}

/* Scoped Token Overrides for Density Levels */
[data-density="comfortable"] {
  --mgp-card-padding: var(--mgp-space-lg);
  --mgp-row-padding-y: var(--mgp-space-md);
  --mgp-row-padding-x: var(--mgp-space-lg);
  --mgp-section-gap: var(--mgp-space-xl);
}

[data-density="default"] {
  --mgp-card-padding: var(--mgp-space-md);
  --mgp-row-padding-y: var(--mgp-space-sm);
  --mgp-row-padding-x: var(--mgp-space-md);
  --mgp-section-gap: var(--mgp-space-lg);
}

[data-density="compact"] {
  --mgp-card-padding: var(--mgp-space-sm);
  --mgp-row-padding-y: 0.375rem;
  --mgp-row-padding-x: var(--mgp-space-sm);
  --mgp-section-gap: var(--mgp-space-md);
}

/* Bemærk: Compact mode neddrosler spacing, men ALDRIG mobil touch targets */
@media (max-width: 640px) {
  [data-density="compact"] .btn,
  [data-density="compact"] .form-control {
    min-height: var(--mgp-touch-target-min) !important;
  }
}
```

---

### 2.3 CSS Klassestruktur (Komponentorienteret)

Systemet anvender en ren, Bootstrap 5-kompatibel klasseform. Klasser navngives med enkle komponentnavne (`.card`, `.btn`, `.status-message`), og varianter tilføjes som eksplicitte modifiers (`.btn-primary`, `.status-danger`):

```css
/* Eksempel på komponentorienteret klassestruktur */
.status-message { ... }             /* Komponent baseline */
.status-message.status-danger { ... }/* Intention variant */
.status-message.status-page { ... }  /* Scope modifier */
```

---

## 3. Fil- og Mappeorganisation

For at holde koden modulær og nem at navigere i opdeles både Razor-komponenter og CSS-filer i strukturerede mapper.

### 3.1 Blazor / Razor Komponentstruktur

Strukturen placeres i Blazor Web-projektet under `Components/`:

```text
src/MyGardenPlanner.Web/
└── Components/
    ├── Architecture/               # Betinget logik & wrappers
    │   └── PermissionGate.razor
    ├── Domain/                     # Domænespecifikke komponenter
    │   ├── ArchivedBadge.razor
    │   ├── AttentionBadge.razor
    │   ├── AttentionSummary.razor
    │   ├── FileBadge.razor
    │   ├── InvitationStatusCard.razor
    │   ├── MediaCard.razor
    │   ├── PlantCard.razor
    │   ├── PlantName.razor
    │   └── RoleBadge.razor
    ├── Feedback/                   # Status, besked- & empty states
    │   ├── EmptyState.razor
    │   ├── GlobalBanner.razor
    │   ├── InlineLoading.razor
    │   ├── ObjectStatus.razor
    │   ├── PermissionHint.razor
    │   ├── StatusBanner.razor
    │   ├── StatusMessage.razor
    │   ├── Toast.razor
    │   ├── ToastContainer.razor
    │   └── UndoStatus.razor
    ├── Foundation/                 # Basiselementer & Skeletons
    │   ├── Badge.razor
    │   ├── Button.razor
    │   ├── LoadingButton.razor
    │   ├── SkeletonCard.razor
    │   ├── SkeletonMediaCard.razor
    │   ├── SkeletonRow.razor
    │   ├── Stack.razor
    │   └── Thumbnail.razor
    ├── Interaction/                # Modaler, dialoger, filtre & inputs
    │   ├── ButtonAligned.razor
    │   ├── ConfirmDialog.razor
    │   ├── ContextBackButton.razor
    │   ├── ContextTabs.razor
    │   ├── FilterChip.razor
    │   ├── FilterDrawer.razor
    │   ├── FormField.razor
    │   ├── InlineConfirm.razor
    │   ├── NavDrawer.razor
    │   ├── NavMenu.razor
    │   ├── SearchInput.razor
    │   ├── SortSelect.razor
    │   ├── UploadZone.razor
    │   └── ViewModeToggle.razor
    └── Layout/                     # Sider, grids, headers & sektioner
        ├── ActiveFilterSummary.razor
        ├── AppShell.razor
        ├── Card.razor
        ├── CollapsibleSectionCard.razor
        ├── CompactEntityRow.razor
        ├── DangerZone.razor
        ├── DataTable.razor
        ├── DetailHeader.razor
        ├── DetailPage.razor
        ├── FilterBar.razor
        ├── FormSection.razor
        ├── FullViewer.razor
        ├── MediaPreview.razor
        ├── MobileHeader.razor
        ├── PrintFooter.razor
        ├── PrintScope.razor
        ├── PrintTable.razor
        ├── ReadOnlySection.razor
        ├── SummaryCard.razor
        ├── SummaryGrid.razor
        └── ThumbnailGrid.razor
```

---

### 3.2 CSS Arkitektur og Mappestruktur

Stylesheet-arkitekturen følger en flad og performant struktur under `wwwroot/css/`:

```text
wwwroot/css/
├── main.css                        # Hovedfil der importerer de tre lag below
├── 01-tokens/
│   ├── colors.css                  # Global, semantic & component color tokens
│   ├── density.css                 # Density tokens & overrides ([data-density])
│   ├── spacing.css                 # Spacing scale, layout & touch target tokens
│   └── typography.css              # Typography sizes, weights & line heights
├── 02-base/
│   ├── accessibility.css           # Focus-visible, reduced-motion & sr-only rules
│   ├── bootstrap-overrides.css     # Tilpasning af Bootstraps baseline
│   └── reset.css                   # Global layout reset & body baseline
└── 03-components/
    ├── cards.css                   # Card, PlantCard, MediaCard & SummaryCard
    ├── dialogs-drawers.css         # ConfirmDialog, NavDrawer, FilterDrawer
    ├── feedback.css                # StatusMessage, Toast, EmptyState, Banners
    ├── forms.css                   # FormField, FormSection, UploadZone, SearchInput
    ├── layout-shell.css            # AppShell, MobileHeader, DetailPage, Stack
    ├── navigation.css              # NavMenu, ContextTabs, FilterBar
    ├── print.css                   # Pure @media print styling & PrintTable
    └── tables-lists.css            # DataTable, CompactEntityRow, ThumbnailGrid
```