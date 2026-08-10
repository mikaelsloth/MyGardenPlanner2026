# 21. Mobile-specific interaction guidelines

## Overordnet princip

> **Mobil er ikke bare desktop i én kolonne. Mobile layouts skal prioritere læsbarhed, touch-interaktion, korte flows og tydelige handlinger.**

På desktop kan vi ofte bruge sidekolonner, hover, brede tabeller, toolbars og mange synlige kontroller. På mobil skal UI’et være mere sekventielt og handlingsorienteret.

***

## 21.1 Mobil er touch-first

Den vigtigste forskel er, at mobil bruges med fingre — ikke mus.

Det betyder:

* ingen hover-afhængighed
* større trykflader
* mere afstand mellem interaktive elementer
* færre små ikonknapper
* tydelig feedback ved tap
* vigtigste handlinger skal være lette at ramme

NN/g skriver, at interaktive elementer på touchscreens bør være mindst **1 cm × 1 cm** for at støtte hurtig og præcis interaktion og reducere “fat-finger errors”.

## Designregel

> **Alle mobile handlinger skal kunne udføres med tydelige, finger-venlige touch targets — uden hover eller præcise musebevægelser.**

***

# 21.2 Ingen hover-afhængighed

På desktop kan vi bruge hover til:

* tooltips
* skjulte actions
* preview
* row actions
* hover highlight
* “mere”-knapper der først vises ved hover

På mobil findes hover ikke som en stabil interaktion. UXPin fremhæver også, at funktionalitet skal være tilgængelig via taps og ikke afhænge af hover states.

## Dårligt på mobil

```text
Handlinger vises først når brugeren hover over et card.
```

## Bedre

```text
Handlinger er synlige som knapper.
Sekundære handlinger ligger i “Flere handlinger”.
```

## Designregel

> **Alle handlinger, som brugeren skal kunne udføre på mobil, skal være synlige eller tilgængelige via tap — ikke hover.**

***

# 21.3 Touch targets og spacing

Vi bør definere en minimumsstandard for MyGardenPlanner.

Mit forslag:

```text
Primære mobile knapper: mindst 44–48 px høj touch target
Ikonknapper: visuel ikonstørrelse må gerne være mindre, men touch target skal være stort
Afstand mellem tætte handlinger: tydelig spacing
```

NN/g anbefaler som nævnt en fysisk minimumsstørrelse på 1 cm × 1 cm for touch targets.   
UXPin nævner også større, finger-venlige targets og tilstrækkelig spacing som centrale hensyn for touch devices.

## Designregel

> **Ikoner må gerne være små visuelt, men det klikbare område må ikke være småt.**

Eksempel:

```css
.icon-button {
  width: 44px;
  height: 44px;
  display: grid;
  place-items: center;
}
```

***

# 21.4 Mobile actions: fuld bredde i flows

På mobil bør actions i formularflows og confirmation flows typisk være fuld bredde.

## Eksempel

```text
[Annullér]
[Gem ændringer]
```

i stedet for:

```text
[Annullér] [Gem ændringer]
```

på en smal skærm.

## Gælder især for

* formularer
* confirmations
* upload
* filter drawer
* destructive actions
* login/signup
* invitation flows

## Designregel

> **I mobile flows skal primære og destruktive handlinger være store, tydelige og lette at ramme.**

***

# 21.5 Primære handlinger skal være tæt på konteksten

På mobil bliver sider lange. Derfor skal handlinger placeres tæt på den sektion, de påvirker.

Eksempler:

## Filer

```text
Filer
[Upload fil]
```

ikke kun en upload-knap øverst på siden.

## Bede

```text
Bede
[Opret bed]
```

## Medlemmer

```text
Medlemmer
[Invitér person]
```

## Designregel

> **På mobil bør sektionens vigtigste handling være synlig i eller tæt på selve sektionen.**

Header actions alene er ikke nok, hvis brugeren skal scrolle langt.

***

# 21.6 Sticky topbar må ikke dominere

I dine designbeslutninger er det allerede nævnt, at sticky topbar på mobil ikke må fylde for meget.

Jeg vil formulere det sådan:

> **Sticky elementer på mobil skal være små og nyttige — ellers stjæler de arbejdsflade.**

## God mobil topbar

```text
← Villa Solbakken
[⋯]
```

## Dårlig mobil topbar

```text
Stor logo/header
Lang subtitle
4 knapper
Breadcrumb
Tabs
```

Alt sammen sticky.

## Designregel

> **På mobil bør kun navigation/tilbagevej og evt. én “mere”-menu være sticky. Resten bør scrolle med indholdet.**

***

# 21.7 Tabs på mobil

Tabs fungerer fint på desktop, men kan blive tunge på mobil.

## Muligheder

### 1. Horisontal scroll tabs

Godt hvis der er få og korte tabs:

```text
[Overblik] [Bede] [Filer] [Medlemmer]
```

### 2. Dropdown/section switcher

Godt hvis der er mange tabs:

```text
Vis sektion:
[Overblik ▼]
```

### 3. Sektioner i én side

Godt hvis detail page ikke er for lang:

```text
Overblik
Bede
Filer
Medlemmer
```

## Designregel

> **Mobile tabs skal være lette at overskue. Hvis der er for mange, bør de blive til dropdown eller sektioner.**

***

# 21.8 Filter som drawer eller bottom sheet

Dette har vi allerede berørt under search/filter/sort.

På mobil bør filter ikke være en overfyldt toolbar.

## Mobil pattern

```text
[Søg planter]
[Filtrer] [Sortér]

Aktive filtre:
[Sol ×] [Staude ×]
```

Når brugeren trykker “Filtrer”:

```text
Bottom sheet / drawer:
Filtrer planter
Lys
Type
Blomstring

[Anvend filtre]
[Nulstil]
```

Dette matcher også de mobile punkter i dine designbeslutninger.

## Designregel

> **Flere filtre på mobil skal i drawer/bottom sheet — ikke presses ind i en desktop-filterbar.**

***

# 21.9 Tables skal blive cards/rows

I dine designbeslutninger står også, at tables på mobil skal blive cards/rows.

Det passer godt med vores #9-beslutning:

* desktop table til sammenligning
* mobil stacked rows eller cards
* kontrolleret horisontal scroll kun ved bevidst datatunge tabeller

## Eksempel desktop table

```text
Materiale | Type | Mål | Pris | Note
```

## Mobil stacked row

```text
Chaussésten
Type: Belægning
Mål: 10 × 10 × 10 cm
Pris: 4,50 kr/stk.
Note: Kant og sti
```

## Designregel

> **Tables skal have en mobilstrategi: stacked rows, cards eller bevidst horisontal scroll.**

***

# 21.10 Media preview som full screen

For billeder, PDF-preview og dokumentvisning bør mobil ofte bruge full-screen preview.

## Hvorfor?

* skærmen er lille
* sidepaneler fylder for meget
* preview kræver plads
* download/original actions skal være tydelige
* bruger skal let kunne lukke preview

## Pattern

```text
Filkort
[Vis preview]

Full-screen preview:
← Luk
Haveskitse maj.pdf
[Download]
```

## Designregel

> **På mobil bør media preview åbne i en full-screen visning eller tydelig modal med luk/tilbage.**

***

# 21.11 Forms på mobil

Formularer skal være mere sekventielle.

## Regler

* én kolonne
* synlige labels
* feltgrupper i logisk rækkefølge
* primær handling nederst
* knapper fuld bredde
* validation ved feltet
* undgå to eller tre felter på samme linje
* undgå for mange actions i header

## Godt

```text
Havens navn
[Villa Solbakken]

Type
[Kundehave]

Lokation
[Aarhus N]

[Gem ændringer]
```

## Designregel

> **Mobilformularer skal være én kolonne med tydelige labels og field-level validation.**

***

# 21.12 Confirmation på mobil

Vi har tidligere besluttet, at confirmations skal være proportionale. På mobil skal de også være lette at gennemføre uden præcise taps.

## Pattern

```text
Slet fil?
“Haveskitse maj.pdf” fjernes fra Villa Solbakken.

[Annullér]
[Slet fil]
```

## Regler

* knapper fuld bredde
* danger action tydeligt label
* tekst kort og konkret
* fokus på objekt og konsekvens
* ingen små close-only controls som eneste cancel

## Designregel

> **Mobile confirmations skal være korte, konkrete og have store knapper.**

***

# 21.13 Toasts på mobil

Toasts må ikke dække primære handlinger eller bundnavigation.

I vores #7-guidelines sagde vi, at toasts kun bruges til korte, ikke-kritiske bekræftelser. På mobil skal placeringen være ekstra forsigtig.

## God mobil toast

```text
Link kopieret.
```

vises kortvarigt nederst, men ikke oven på vigtig action.

## Dårlig mobil toast

```text
Upload fejlede...
```

som forsvinder og dækker uploadknappen.

## Designregel

> **På mobil må toast aldrig være eneste feedback for fejl, no-access, validation eller destructive actions.**

***

# 21.14 Collapsible sektioner på mobil

Efter vores Demo19b giver det mening at sige:

* collapsible sidekort på desktop kan blive sektioner på mobil
* sekundære sektioner kan starte collapsed
* attention-sektioner bør være åbne
* collapsed header skal vise summary

## Eksempel

```text
Medlemmer
2 aktive · 1 invitation afventer
[åben pga. attention]

Print og eksport
Printvenligt haveoverblik
[collapsed]

Arkivering og sletning
Avancerede handlinger
[collapsed]
```

## Designregel

> **Collapsible sektioner er nyttige på mobil, men må ikke skjule attention states eller nødvendige handlinger.**

***

# 21.15 Bottom navigation eller ikke?

Vi har allerede arbejdet med responsiv navigation tidligere. For MyGardenPlanner ville jeg være forsigtig med en tung bottom nav, medmindre de vigtigste topniveauområder er meget stabile.

Mulige topniveauer kunne være:

```text
Haver
Planter
Materialer
Filer
Mere
```

Men hvis appen især er kontekstuel omkring haver og detail pages, kan en enkel topbar + drawer være bedre.

## Min anbefaling

Start med:

* topbar med tilbage/context
* menu/drawer til navigation
* sektionelle actions i indholdet
* ingen tung bottom nav i første version

## Designregel

> **Bottom navigation bør kun bruges, hvis appen har få, stabile topniveauområder.**

***

# 21.16 One-handed use og reachability

Mobile layouts bør ikke placere alle primære handlinger øverst og alle sekundære nederst uden omtanke.

UXPin fremhæver thumb zones og nævner, at centrale interaktive elementer bør placeres, så de er lettere at nå på touch devices.

For MyGardenPlanner betyder det især:

* primær handling i sektionen
* ikke kun øverst i header
* sticky bottom action kan bruges i korte flows
* destructive actions ikke i sticky bottom uden confirmation

## Designregel

> **Vigtige mobile handlinger bør placeres tæt på arbejdsområdet og være lette at nå.**

***

# 21.17 Loading på mobil

Mobile loading skal være lokal og stabil.

## Brug

* skeleton rows/cards
* loading button
* upload progress
* processing status
* ikke store globale spinners

## Designregel

> **Mobil loading må ikke få siden til at hoppe eller nulstille scrollposition.**

Det hænger sammen med vores navigation state og loading guidelines.

***

# 21.18 Mobile detail page

For detail pages bør mobil layout være:

```text
Tilbage
Titel
Metadata/badges
Primary actions
Status
Tabs/dropdown
Summary cards
Main content
Related sections
Collapsible secondary sections
Danger zone
```

## Designregel

> **Detail pages på mobil skal være én kolonne med tydelige sektioner og handlinger tæt på indholdet.**

***

# 21.19 Mobile-specific defaults for MyGardenPlanner

## Haver

* cards i én kolonne
* search øverst
* filter i drawer
* “Opret have” fuld bredde eller tydelig primary action

## Bede

* cards/rows i én kolonne
* “Opret bed” i sektionen
* dimensioner vises som metadata

## Planter

* search direkte synlig
* filter drawer
* compact rows som default ved mange resultater
* cards ved browsing/forslag

## Materialer

* compact rows/stacked rows
* table bliver stacked rows
* pris/mål vises som label-value

## Filer

* media cards i én kolonne
* preview full-screen
* upload progress inline
* download kun eksplicit

## Medlemmer/invitationer

* compact rows
* role/status badges
* admin actions under “Flere” eller synlige hvis få

***

# 21.20 CSS-principper

Eksempler på mobile CSS-regler:

```css
@media (max-width: 760px) {
  .detail-layout,
  .summary-grid,
  .grid-2,
  .grid-3 {
    grid-template-columns: 1fr;
  }

  .btn-row,
  .card-actions,
  .dialog-actions {
    flex-direction: column;
    align-items: stretch;
  }

  .btn {
    width: 100%;
    min-height: 44px;
  }

  .icon-button {
    width: 44px;
    height: 44px;
  }

  .filter-bar {
    display: none;
  }

  .mobile-filter-actions {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-sm);
  }
}
```

***

# 21.21 Beslutningstabel

| Situation     | Desktop             | Mobil                   |
| ------------- | ------------------- | ----------------------- |
| Filterbar     | Inline toolbar      | Drawer/bottom sheet     |
| Tables        | Tabel               | Stacked rows/cards      |
| Detail page   | To kolonner         | Én kolonne              |
| Sidepanel     | Sidebar cards       | Sektioner/collapsible   |
| Media preview | Panel/modal         | Full-screen             |
| Formular      | Evt. 2 kolonner     | Én kolonne              |
| Actions       | Knaprække           | Fuld bredde i flows     |
| Tabs          | Horisontale tabs    | Scroll tabs/dropdown    |
| Toast         | Hjørne              | Bund, ikke over actions |
| Navigation    | Sidebar/topbar      | Topbar/drawer           |
| Hover actions | Kan bruges sparsomt | Må ikke bruges          |

***

# Anbefalet designbeslutning

Jeg ville låse principperne sådan:

1. **Mobil er touch-first, ikke desktop i én kolonne.**
2. **Ingen funktioner må afhænge af hover.**
3. **Touch targets skal være store nok og have tydelig spacing.**
4. **Knapper i formular-, upload- og confirmation-flows er fuld bredde.**
5. **Filter bruger drawer/bottom sheet på mobil.**
6. **Tables bliver stacked rows/cards eller får bevidst horisontal scroll.**
7. **Media preview åbnes full-screen på mobil.**
8. **Detail pages bliver én kolonne med handlinger tæt på indholdet.**
9. **Sticky topbar skal være kompakt og må ikke dominere.**
10. **Collapsible sektioner er gode på mobil, men attention states skal være synlige.**
11. **Toasts må ikke dække primære handlinger og må ikke bære kritisk information.**
12. **Mobile loading skal være lokal og stabil uden layout jumps.**
13. **Bottom navigation bruges kun, hvis topniveauområderne er få og stabile.**

***

# Kort dokumentationstekst

Du kan bruge denne direkte:

> **Mobile-specific interaction guidelines:** Mobil UI skal designes touch-first og ikke blot som desktop i én kolonne. Funktioner må ikke afhænge af hover, og interaktive elementer skal have finger-venlige touch targets og tydelig spacing. På mobil vises formularer, confirmations og primære flows i én kolonne med fuldbreddeknapper. Filtre håndteres i drawer/bottom sheet, tabeller bliver til stacked rows/cards, og media preview åbnes full-screen. Detail pages prioriterer titel, metadata, handlinger, status og indhold sekventielt. Sticky topbar skal være kompakt, og sekundære sektioner kan være collapsible, så længe attention states ikke skjules.