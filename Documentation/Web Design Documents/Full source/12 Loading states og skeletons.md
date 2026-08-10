# 12. Loading states og skeletons

## Overordnet princip

> **Loading states skal gøre ventetid forståelig, rolig og forudsigelig — uden at få appen til at føles langsom eller dramatisk.**

Det betyder, at vi ikke bare skal vise en spinner overalt. Vi skal vælge loading pattern efter situationen:

| Situation                      | Bedste loading pattern  |
| ------------------------------ | ----------------------- |
| Kendt layout, data hentes      | Skeleton                |
| Kort knap-handling             | Disabled button + tekst |
| Upload/download med fremdrift  | Progress bar            |
| Thumbnail under behandling     | Processing placeholder  |
| Ukendt ventetid i lille område | Inline loading message  |
| Hele side hentes               | Page skeleton           |
| Fejl efter loading             | Error state             |
| Tomt resultat efter loading    | Empty state             |

***

## 12.1 Loading må ikke forveksles med empty state

Dette er måske det vigtigste princip.

En bruger må ikke først se:

```text
Ingen filer endnu
```

og derefter, et øjeblik senere:

```text
3 filer vises
```

Det føles som en fejl.

## Designregel

> **Vis loading state, indtil data er afklaret. Vis først empty state, når vi ved, at der faktisk ikke er data.**

Flowet bør være:

```text
Loading → data findes → vis indhold
Loading → ingen data → vis empty state
Loading → fejl → vis error state
```

Det passer direkte sammen med Demo10, hvor vi adskilte skeleton/loading fra empty states.

***

## 12.2 Skeletons er bedst, når layoutet er kendt

Skeletons giver mest mening, når vi allerede ved, hvordan indholdet kommer til at se ud:

* kort
* rækker
* tabeller
* media cards
* thumbnails
* dashboard widgets
* detailsektioner

Skeleton loading screen design — How to improve perceived performance beskriver skeleton loading som en teknik, hvor UI’et viser placeholders, der efterligner den endelige struktur, mens indholdet stadig loader. Kilden skelner også mellem skeletons, spinners og progress bars, hvor skeletons især hjælper brugeren med at forstå, hvordan indholdet kommer til at blive placeret.

## Godt eksempel

```text
[ skeleton thumbnail ]  [ skeleton title        ]
                       [ skeleton meta         ]
                       [ skeleton buttons      ]
```

## Dårligt eksempel

```text
Loading...
```

Hvis vi kan vise formen på det kommende indhold, bør vi gøre det.

***

## 12.3 Skeletons skal ligne den endelige komponent

Skeletons bør ikke være tilfældige grå bokse. De skal ligne komponentens endelige struktur.

## Eksempler

### Card skeleton

Bruges til:

* havekort
* plantekort
* materialekort
* summary cards

```html
<article class="card skeleton-card" aria-hidden="true">
  <div class="skeleton-line skeleton-title"></div>
  <div class="skeleton-line skeleton-meta"></div>
  <div class="skeleton-block"></div>
</article>
```

### Compact row skeleton

Bruges til:

* lange plantelister
* materialelister
* filrækker

```html
<div class="skeleton-row" aria-hidden="true">
  <div class="skeleton-icon"></div>
  <div class="skeleton-row-content">
    <div class="skeleton-line skeleton-title"></div>
    <div class="skeleton-line skeleton-meta"></div>
  </div>
  <div class="skeleton-pill"></div>
</div>
```

### Media skeleton

Bruges til:

* thumbnails
* dokumentkort
* preview cards

```html
<article class="card media-card">
  <div class="skeleton-thumbnail"></div>
  <div class="media-main">
    <div class="skeleton-line skeleton-title"></div>
    <div class="skeleton-line skeleton-meta"></div>
    <div class="skeleton-line skeleton-actions"></div>
  </div>
</article>
```

## Designregel

> **Skeleton skal matche layoutet, ikke bare signalere ventetid.**

***

## 12.4 Hvornår skal vi bruge spinner?

Spinner er stadig nyttig, men kun i bestemte situationer.

## Spinner passer til

* meget kort ventetid
* knap-handlinger
* små inline actions
* “Gemmer…”
* “Uploader…”
* “Sletter…”

Eksempel:

```html
<button class="btn btn-primary" disabled>
  <span class="btn-spinner"></span>
  Gemmer…
</button>
```

## Spinner passer dårligt til

* hele sider
* store lister
* dashboards
* filgrids
* thumbnails
* detailvisninger

Der bør vi bruge skeleton eller processing placeholders.

## Designregel

> **Spinner bruges til handlinger. Skeleton bruges til indhold.**

***

## 12.5 Progress bars bruges kun når der faktisk er progress

Progress bar giver mening, når vi kan vise reel fremdrift:

* upload
* download
* thumbnail generation, hvis progress kan måles
* batch processing, hvis antal elementer kendes

Eksempel:

```html
<div class="progress" aria-label="Uploadstatus">
  <div class="progress-bar" style="width:62%"></div>
</div>
```

Hvis vi ikke kender progress, skal vi ikke fake det. Så er en statusbesked bedre:

```text
Thumbnail oprettes…
Filen er uploadet, men forhåndsvisningen er ikke klar endnu.
```

## Designregel

> **Brug progress bar til målbar fremdrift. Brug status/skeleton til ukendt ventetid.**

***

## 12.6 Loading states skal være rolige

Vi skal undgå loading UI, der føles hektisk.

Brug:

* bløde neutrale flader
* lav kontrast
* diskret shimmer eller pulse
* samme spacing som rigtige komponenter
* ingen store blinkende indikatorer

Skeleton Screens 101 fra NN/g beskriver skeleton screens som placeholders, der efterligner sidens layout under load, og nævner også at animationer i skeleton screens kan være distraherende eller skabe tilgængelighedsproblemer for nogle brugere.

## Designregel

> **Loading-animationer skal være subtile og må aldrig dominere brugerens opmærksomhed.**

***

## 12.7 Accessibility: respekter reduced motion

Skeleton shimmer kan være fint, men vi bør understøtte `prefers-reduced-motion`.

```css
@media (prefers-reduced-motion: reduce) {
  .skeleton-line,
  .skeleton-block,
  .skeleton-thumbnail,
  .skeleton-icon {
    animation: none;
  }
}
```

## Designregel

> **Skeleton animation må slås fra for brugere, der foretrækker reduceret bevægelse.**

Det er en lille ting, men vigtig.

***

## 12.8 Loading skal være lokalt, ikke altid globalt

Vi skal ikke blokere hele appen, bare fordi én sektion loader.

Eksempel:

På haveoverblik kan disse sektioner loade uafhængigt:

* summary cards
* bede
* filer
* medlemmer
* seneste aktivitet

Hvis filer loader, skal hele siden ikke være låst.

## Godt

```text
Haveoverblik vises
Filer-sektionen viser skeleton
Bede-sektionen er allerede klar
```

## Dårligt

```text
Hele siden viser spinner, fordi filsektionen loader
```

## Designregel

> **Vis loading så tæt på den komponent, der loader, som muligt.**

***

## 12.9 Staged loading

For tunge sider kan vi bruge staged loading:

1. Vis shell/header/kontekst
2. Vis primær metadata
3. Vis summary cards
4. Vis lister/cards
5. Lazy load thumbnails og preview

Eksempel for haveoverblik:

```text
1. Titel: Villa Solbakken
2. Metadata: Kundehave · Aarhus N
3. Summary cards skeleton → data
4. Bede skeleton → data
5. Filer skeleton → thumbnails lazy load
```

## Designregel

> **Indhold med høj kontekstværdi skal vises før tungt sekundært indhold.**

For MyGardenPlanner betyder det typisk:

* have-/bednavn først
* metadata hurtigt
* thumbnails senere
* originalfiler aldrig automatisk

***

## 12.10 Billeder og dokumenter har særlige loading states

Vi har allerede talt om media/dokumenter, men loading states her er vigtige nok til at gentage.

## Mulige states

| State                | UI                           |
| -------------------- | ---------------------------- |
| Thumbnail loader     | Skeleton thumbnail           |
| Thumbnail processing | “Thumbnail oprettes…”        |
| Preview loading      | Preview skeleton/stage       |
| Preview unavailable  | Forklaring + download        |
| Original loading     | Kun efter eksplicit handling |
| Uploading            | Progress bar                 |
| Upload complete      | Status message               |
| Upload failed        | Error status                 |

## Designregel

> **Thumbnail-loading må ikke blokere metadata. Filnavn og status skal kunne vises før billedet er klar.**

Eksempel:

```text
Haveskitse maj.pdf
PDF · 2,4 MB · Permanent
[Thumbnail oprettes…]
```

***

## 12.11 Buttons under loading

Når en bruger klikker “Gem”, “Upload”, “Slet” eller “Download”, skal knappen tydeligt skifte state.

## Godt

```html
<button class="btn btn-primary" disabled>
  <span class="btn-spinner"></span>
  Gemmer…
</button>
```

## Dårligt

```html
<button class="btn btn-primary">Gem</button>
```

uden feedback efter klik.

## Designregel

> **Den handling brugeren netop har startet, skal vise loading state direkte på handlingen.**

For knapper:

* disable knappen
* skift label til aktiv handling
* vis evt. spinner
* undgå dobbeltklik
* lad sekundære handlinger være tilgængelige kun hvis det er sikkert

Eksempler:

```text
Gemmer…
Uploader…
Sletter…
Sender invitation…
Opretter plante…
```

***

## 12.12 Optimistic UI — brug forsigtigt

Optimistic UI betyder, at vi viser resultatet før serveren har bekræftet.

Det kan være godt til:

* markér som valgt
* fold sektion ud/ind
* lokal UI-state
* små ikke-destruktive handlinger

Men jeg ville være forsigtig ved:

* upload
* sletning
* invitationer
* betaling/entitlement
* store dataændringer

## Designregel

> **Brug optimistic UI til ufarlige, reversible ændringer. Brug bekræftet loading for dataændringer med konsekvens.**

For MyGardenPlanner:

* `collapsed sidebar` kan være instant
* `vælg view-mode` kan være instant
* `slet fil` bør vente på bekræftelse
* `upload fil` skal vise progress/status
* `send invitation` skal vise afsendelse og resultat

***

## 12.13 Timeout og stalled states

Hvis noget loader længe, skal UI’et ikke bare blive ved med skeleton for evigt.

Vi bør have en “stalled loading” state.

Eksempel:

```text
Det tager længere tid end forventet
Du kan prøve igen eller fortsætte med andre dele af siden.

[Prøv igen]
```

## Designregel

> **Langvarig loading skal skifte fra passiv skeleton til forklarende status.**

Det er især relevant for:

* filpreview
* upload
* thumbnail generation
* store lister
* netværksproblemer

***

## 12.14 Loading og navigation state

Loading må ikke nulstille brugerens kontekst.

Eksempel:

Brugeren kommer tilbage til plantelisten efter oprettelse:

* samme filter
* samme sortering
* samme scroll/anchor
* skeleton vises kun hvor data opdateres
* det nye element highlightes, når det findes

## Designregel

> **Loading må ikke “ryste” brugerens arbejdsposition.**

Det betyder:

* reserver plads
* undgå layout jumps
* brug skeleton med samme højde som endeligt indhold
* brug anchor/highlight efter reload
* bevar filterbarens værdier

***

## 12.15 Skeleton og layout stability

Skeletons skal minimere layout shifts.

## Brug

```css
.skeleton-thumbnail {
  aspect-ratio: 1 / 1;
}
```

eller faste højder for kendte komponenter:

```css
.skeleton-card {
  min-height: 11rem;
}
```

## Designregel

> **Skeletons skal reservere omtrent samme plads som det endelige indhold.**

Hvis skeleton er meget lavere end det endelige card, hopper layoutet, når data kommer ind.

***

# Komponenter vi bør definere

Jeg ville lave disse komponenter/patterns:

## `SkeletonCard`

Til cards.

```html
<article class="card skeleton-card" aria-hidden="true">
  <div class="skeleton-line skeleton-title"></div>
  <div class="skeleton-line skeleton-meta"></div>
  <div class="skeleton-block"></div>
</article>
```

## `SkeletonRow`

Til compact rows.

```html
<div class="skeleton-row" aria-hidden="true">
  <div class="skeleton-icon"></div>
  <div class="skeleton-row-content">
    <div class="skeleton-line skeleton-title"></div>
    <div class="skeleton-line skeleton-meta"></div>
  </div>
  <div class="skeleton-pill"></div>
</div>
```

## `SkeletonMediaCard`

Til billeder/filer.

```html
<article class="card media-card" aria-hidden="true">
  <div class="skeleton-thumbnail"></div>
  <div class="media-main">
    <div class="skeleton-line skeleton-title"></div>
    <div class="skeleton-line skeleton-meta"></div>
    <div class="skeleton-line skeleton-actions"></div>
  </div>
</article>
```

## `InlineLoading`

Til små sektioner.

```html
<div class="inline-loading">
  <span class="btn-spinner"></span>
  Henter filer…
</div>
```

## `LoadingButton`

Til handlinger.

```html
<button class="btn btn-primary" disabled>
  <span class="btn-spinner"></span>
  Gemmer…
</button>
```

***

# CSS-forslag

```css
.skeleton-card,
.skeleton-row,
.skeleton-media {
  pointer-events: none;
}

.skeleton-line,
.skeleton-block,
.skeleton-icon,
.skeleton-pill,
.skeleton-thumbnail {
  background:
    linear-gradient(
      90deg,
      var(--mgp-surface-muted),
      var(--mgp-primary-soft),
      var(--mgp-surface-muted)
    );
  background-size: 200% 100%;
  animation: skeleton-shimmer 1.3s linear infinite;
}

@keyframes skeleton-shimmer {
  to {
    background-position: -200% 0;
  }
}

.skeleton-line {
  height: .85rem;
  border-radius: 999px;
}

.skeleton-title {
  width: 62%;
}

.skeleton-meta {
  width: 42%;
}

.skeleton-actions {
  width: 35%;
}

.skeleton-block {
  height: 6rem;
  border-radius: var(--radius-md);
}

.skeleton-icon {
  width: 2.75rem;
  height: 2.75rem;
  border-radius: 999px;
}

.skeleton-pill {
  width: 4rem;
  height: 1.5rem;
  border-radius: 999px;
}

.skeleton-thumbnail {
  width: 100%;
  aspect-ratio: 1 / 1;
  border-radius: .8rem;
}

.skeleton-row {
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: var(--space-md);
  align-items: center;
  padding: var(--space-sm) var(--space-md);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
  background: var(--mgp-surface);
}

.skeleton-row-content {
  display: grid;
  gap: var(--space-sm);
}

.inline-loading {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  color: var(--mgp-text-muted);
  font-size: var(--font-size-sm);
}

@media (prefers-reduced-motion: reduce) {
  .skeleton-line,
  .skeleton-block,
  .skeleton-icon,
  .skeleton-pill,
  .skeleton-thumbnail {
    animation: none;
  }
}
```

***

# Loading patterns i MyGardenPlanner

## Dashboard

Brug:

* skeleton summary cards
* skeleton recent activity rows
* ikke global spinner

```text
Dashboard shell vises straks
Summary cards loader som skeletons
Seneste aktivitet loader som rows
```

## Haveliste

Brug:

* skeleton entity cards
* filterbar vises straks
* empty state først efter data er afklaret

## Planteliste/materialeliste

Brug:

* skeleton rows ved compact view
* skeleton cards ved card view
* bevar filterbar-values
* undgå at nulstille scroll

## Filer og billeder

Brug:

* metadata først
* thumbnail skeleton
* processing placeholder
* preview loading separat

## Formularer

Brug:

* loading button ved gem
* disabled submit
* status message efter gem
* undgå at skjule hele formularen ved save

## Preview panel

Brug:

* preview-stage skeleton
* metadata vises hvis klar
* “Preview ikke klar” hvis processing

***

# Beslutningstabel

| Situation               | Pattern                | Bemærkning                    |
| ----------------------- | ---------------------- | ----------------------------- |
| Side loader første gang | Page skeleton          | Ikke blank side               |
| Liste loader            | Row/card skeleton      | Matcher view-mode             |
| Dashboard loader        | Summary skeletons      | Delvis loading per widget     |
| Thumbnail loader        | Thumbnail skeleton     | Metadata må gerne være synlig |
| Thumbnail processing    | Processing placeholder | Ikke error                    |
| Preview loader          | Preview skeleton/stage | Ikke original automatisk      |
| Gem handling            | Loading button         | Disable submit                |
| Upload                  | Progress bar           | Hvis progress kendes          |
| Ukendt lang ventetid    | Status + retry         | Ikke evig skeleton            |
| Datafejl                | Error state            | Adskilt fra empty             |
| Ingen data              | Empty state            | Efter loading er afklaret     |

***

# Hvad vi bør undgå

Undgå:

* global spinner for hele appen
* spinner til store lister
* skeletons der ikke matcher layoutet
* skeleton der vises for længe uden forklaring
* shimmer der er for kraftig
* at vise empty state før loading er færdig
* at nulstille filter/scroll ved reload
* at blokere hele siden, når kun én sektion loader
* at hente originalbilleder/dokumenter som del af loading

***

# Anbefalet designbeslutning

Jeg ville låse loading-principperne sådan:

1. **Loading skal vises før empty state.**
2. **Skeletons bruges til kendte layouts og indholdsrige visninger.**
3. **Spinner bruges primært til korte handlinger og knapper.**
4. **Progress bar bruges kun ved målbar fremdrift.**
5. **Loading skal være lokalt, ikke globalt, når det er muligt.**
6. **Skeletons skal matche den endelige komponent og reservere plads.**
7. **Animationer skal være diskrete og respektere reduced motion.**
8. **Metadata skal vises før tunge ressourcer, især ved filer/billeder.**
9. **Langvarig loading skal skifte til forklarende status med retry.**
10. **Loading må ikke nulstille brugerens navigation state.**
11. **Buttons skal vise aktiv handling: Gemmer, Uploader, Sletter osv.**
12. **Originalfiler og store previews hentes kun ved eksplicit handling.**

***

# Kort dokumentationstekst

Du kan bruge denne direkte:

> **Loading states og skeletons:** Loading skal gøre ventetid forståelig uden at få appen til at føles tung. Skeletons bruges til kendte layouts som cards, rows, dashboards og media cards, mens spinners primært bruges til korte handlinger i knapper. Empty states må først vises, når data er afklaret. Loading bør være lokal for den sektion, der henter data, og skeletons skal reservere omtrent samme plads som det endelige indhold for at undgå layout jumps. Ved upload og andre målbare handlinger bruges progress bar. Ved længere eller fastlåst loading vises en forklarende status med mulighed for retry.