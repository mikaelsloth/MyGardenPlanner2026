# 23. Data density / compact mode

## Overordnet princip

> **Tæthed skal følge brugerens opgave. Browsing kræver luft. Arbejde med mange data kræver kompakthed.**

Det betyder, at MyGardenPlanner ikke skal have én fast “luftig” eller én fast “kompakt” stil overalt. Appen skal kunne skifte tæthed afhængigt af kontekst:

* dashboard og overblik må gerne være rolige og rummelige
* lange lister skal kunne være mere kompakte
* print og tabeller skal være tætte og dokumentationsvenlige
* mobil skal bevare touch targets, selv hvis layoutet bliver kompakt

Material Design beskriver også, at tættere UI kan hjælpe, når brugere skal se, scanne, navigere og sammenligne store mængder information — men at tæthed ikke bør øges på komponenter til fokuserede opgaver eller alerts/dialogs, fordi det kan forringe brugervenligheden og beskedens tydelighed.

***

## 23.1 Tre density levels

Jeg ville definere tre niveauer:

| Density         | Brug                                          | Følelse       |
| --------------- | --------------------------------------------- | ------------- |
| **Comfortable** | Dashboard, detail overblik, first-use, cards  | Rolig, luftig |
| **Default**     | Almindelige sider, cards/rows, formularer     | Balanceret    |
| **Compact**     | Lange lister, tabeller, søgeresultater, print | Effektiv, tæt |

## Designregel

> **Default er standard. Comfortable bruges til overblik og onboarding. Compact bruges til dataarbejde.**

***

# 23.2 Comfortable mode

Comfortable er den mest rummelige visning.

## Brug comfortable når

* brugeren skal orientere sig
* der er få eller mellem mange objekter
* visuel scanning er vigtig
* UI’et skal føles roligt
* siden er en landing page eller detail overview
* indholdet har forklarende karakter

## Gode steder

* Dashboard
* Haveliste, hvis der ikke er mange haver
* Beddetalje overview
* First-use empty states
* Detail page header
* Summary cards
* Filcards med thumbnail
* Onboarding/introduktion

## Eksempel

```text
Villa Solbakken
Kundehave · Aarhus N

6 bede · 3 filer · 2 medlemmer

[Åbn have]
```

## Designregel

> **Comfortable bruges, når kontekst og læsbarhed er vigtigere end flest mulige rækker på skærmen.**

***

# 23.3 Default mode

Default er den normale tæthed for MyGardenPlanner.

## Brug default når

* der er blandet indhold
* brugeren både skal læse og handle
* siden ikke er meget datatung
* der både er cards, rows og sektioner
* mobil og desktop skal føles ensartet

## Gode steder

* Detail pages
* Almindelige lister
* Filsektioner
* Medlemslister
* Invitationer
* Formularer
* Related sections

## Designregel

> **Default skal være appens normale rytme og bør bruges, medmindre der er en klar grund til comfortable eller compact.**

***

# 23.4 Compact mode

Compact er til høj datatæthed og effektiv scanning.

## Brug compact når

* listen er lang
* brugeren filtrerer/søger
* brugeren skal sammenligne mange resultater
* det primære behov er at finde hurtigt
* data er struktureret
* print/dokumentation kræver flere rækker

Material Design beskriver, at øget density kan gøre store mængder indhold lettere at scanne, se og sammenligne, fx i lister, tabeller og lange formularer.

## Gode steder

* Store plantelister
* Materialelister
* Filer i compact list
* Invitationer/medlemmer ved mange rækker
* Søgeresultater
* Tables
* Print-lister
* Admin-lignende oversigter

## Eksempel

```text
Lavendel
Lavandula angustifolia · Sol · Staude · 30–60 cm
[Vis]
```

## Designregel

> **Compact mode bruges til scanning og dataarbejde — ikke til emotionelle, forklarende eller kritiske flows.**

***

# 23.5 Hvornår må man ikke bruge compact?

Compact er ikke bare “mere effektivt”. Det kan også gøre UI’et mere stressende og sværere at bruge.

Material Design fraråder at øge density på komponenter, der involverer fokuserede opgaver, fx pickers/dropdowns, og på alerts/messaging som dialogs, fordi det kan reducere brugervenlighed, læsbarhed og prominens.

## Undgå compact på

* confirmations
* danger dialogs
* no-access states
* error states
* onboarding/first-use
* empty states
* mobile touch-heavy flows
* media preview
* upload errors
* critical status messages
* filter drawers med mange touch controls, hvis touch targets bliver for små

## Designregel

> **Compact må aldrig gøre kritiske beskeder, touch targets eller beslutninger sværere at forstå.**

***

# 23.6 Density og view modes

Density hænger sammen med #9, men det er ikke det samme.

| View mode      | Typisk density        |
| -------------- | --------------------- |
| Cards          | Comfortable / Default |
| Compact rows   | Compact               |
| Tables         | Compact               |
| Thumbnail grid | Default / Comfortable |
| Detail view    | Default               |
| Print table    | Compact               |

## Vigtig skelnen

* **View-mode** ændrer præsentationsform.
* **Density** ændrer spacing, padding og informationsmængde.
* En table er ofte compact, men kan stadig have comfortable spacing.
* Et card kan være default eller compact, men bør sjældent blive meget tæt.

## Designregel

> **Density er en skala inden for en visning — ikke det samme som at skifte fra cards til table.**

***

# 23.7 Density og spacing tokens

Jeg ville ikke lave helt separate komponenter til hvert niveau. Jeg ville hellere lade komponenterne reagere på density via tokens/classes.

## Eksempel

```css
:root {
  --card-padding: 1rem;
  --row-padding-y: .75rem;
  --row-padding-x: 1rem;
  --section-gap: 1.5rem;
}

[data-density="comfortable"] {
  --card-padding: 1.5rem;
  --row-padding-y: 1rem;
  --row-padding-x: 1.25rem;
  --section-gap: 2rem;
}

[data-density="compact"] {
  --card-padding: .75rem;
  --row-padding-y: .45rem;
  --row-padding-x: .75rem;
  --section-gap: 1rem;
}
```

MUI beskriver, at højere density kan anvendes via reduceret spacing eller reduceret komponentstørrelse, afhængigt af komponenten.

## Designregel

> **Density bør styres via tokens, så spacing ændres konsistent på tværs af komponenter.**

***

# 23.8 Hvad må density ændre?

Density må justere:

* card padding
* row padding
* gap mellem elementer
* table cell padding
* list item spacing
* font-size i sekundær metadata, forsigtigt
* antal synlige metadatafelter
* thumbnail størrelse
* section spacing
* toolbar spacing

Density bør ikke ødelægge:

* minimum touch target
* focus states
* læsbarhed
* kontrast
* label visibility
* statusbeskedernes tydelighed
* hit area på icon buttons

## Designregel

> **Density må reducere visuel luft, men ikke funktionel tilgængelighed.**

***

# 23.9 Metadata-prioritering ved compact mode

Compact mode handler ikke kun om mindre padding. Det handler også om at vise færre ting.

## Cards/default

```text
Lavendel
Lavandula angustifolia
Sol · Staude · 30–60 cm · juni–august
[Duft] [Tørketålende]
[Vis] [Tilføj]
```

## Compact row

```text
Lavendel
Lavandula angustifolia · Sol · Staude · 30–60 cm
[Vis]
```

## Designregel

> **Jo mere kompakt visningen er, desto hårdere skal metadata prioriteres.**

Det hænger direkte sammen med #9: jo tættere visning, desto færre synlige handlinger og metadata.

***

# 23.10 Density og handlinger

Handlinger skal også tilpasses tæthed.

## Comfortable

* 2–3 synlige handlinger kan være ok
* knapper må have tekst
* badges og metadata kan fylde mere

## Default

* 1–2 primære handlinger
* sekundære handlinger i “Flere”
* normal spacing

## Compact

* 1 synlig primær handling
* sekundære handlinger i menu
* færre badges
* ingen lange beskrivelser

## Designregel

> **Compact rows bør have få synlige handlinger, ellers bliver de ikke kompakte.**

***

# 23.11 Density og mobil

På mobil skal vi være forsigtige.

Selv hvis en liste er compact, skal touch targets stadig være store nok. I vores #11 og #12 har vi allerede besluttet, at mobile touch targets skal være finger-venlige og at knapper i flows er fuld bredde.

## Mobil compact betyder derfor

* mindre tekstmængde
* færre metadatafelter
* kortere rows
* færre synlige actions
* men stadig store trykflader

## Ikke

* 28 px høje rækker
* små ikonknapper uden hit area
* tæt stablede destructive actions

## Designregel

> **På mobil reducerer compact primært informationsmængde — ikke touch target-størrelse.**

***

# 23.12 Density og print

Print er ofte naturligt compact.

## Print bør typisk bruge

* tabeller
* kompakte rows
* færre visuelle dekorationer
* mindre spacing
* ingen navigation/actions
* tydelige kolonner
* dokumentationsvenlige overskrifter

## Eksempel

Skærm:

```text
Materiale cards
```

Print:

```text
Materialeliste som tabel
```

## Designregel

> **Print bruger sin egen density, typisk compact/document mode.**

***

# 23.13 Density og brugerpræference

I designbeslutningerne nævnes, at density senere kan gemmes som brugerpræference, ligesom collapsed sidebar.

Jeg er enig, men jeg ville ikke starte med global density preference i første version.

## Første version

* brug kontekstbestemt density
* cards/detail = default/comfortable
* lange lister = compact
* print = compact

## Senere version

Tilføj evt.:

```text
Visningstæthed:
[Comfortable] [Default] [Compact]
```

Men kun hvis brugere faktisk har behov for det.

## Designregel

> **Start med kontekstbestemt density. Gør det først til brugerpræference, hvis behovet viser sig.**

***

# 23.14 Density som URL-state eller preference?

## View mode

Bør ofte være URL-state:

```text
/planter?view=compact
```

## Density

Bør normalt være:

* komponent-/kontekstbestemt
* evt. brugerpræference senere
* sjældent URL-state alene

## Eksempel

```text
/planter?view=rows
```

er bedre end:

```text
/planter?density=compact
```

medmindre density er et eksplicit brugerfilter/præference.

## Designregel

> **View-mode kan være URL-state. Density bør normalt være komponentlogik eller brugerpræference.**

***

# 23.15 Default density pr. område

## Dashboard

**Comfortable**

Fordi det er overblik, status og navigation.

## Haver

**Default / Comfortable**

Cards med rolig kontekst. Compact rows kan være alternativ ved mange haver.

## Bede

**Default**

Cards eller rows med mål, lysforhold og status.

## Planter

**Default for browsing, compact for søgning/filter**

Planter kan blive en stor liste, så compact rows er vigtige.

## Materialer

**Compact/default**

Materialer har strukturerede data og egner sig til rows/tables.

## Filer

**Default for media cards, compact for listevisning**

Thumbnail grid må ikke blive for tæt, fordi visuel genkendelse kræver plads.

## Medlemmer/invitationer

**Compact/default**

Ofte statusorienteret og row-baseret.

## Detail pages

**Default**

Header og main content skal have ro. Sidekolonne kan have compact/collapsible sekundærindhold.

## Print

**Compact**

Dokumentation og tabeller.

***

# 23.16 Density og komponenter

Jeg ville definere density-support på disse komponenter:

## `EntityCard`

* comfortable
* default
* compact, men kun hvis card stadig er læsbart

## `CompactEntityRow`

* default
* compact

## `DataTable`

* default
* compact
* print

## `SummaryCard`

* comfortable/default
* ikke meget compact

## `StatusMessage`

* default
* aldrig meget compact ved warning/error

## `FilterBar`

* default
* compact på desktop
* drawer på mobil

## `DetailSection`

* default
* compact for sekundære related lists

***

# 23.17 Density og accessibility

Density skal altid kontrolleres mod accessibility baseline.

## Check

* Kan man stadig tabbe i logisk rækkefølge?
* Er focus ring tydelig?
* Er touch target stort nok?
* Er tekst stadig læsbar?
* Er badges stadig forståelige?
* Er statusbeskeder stadig tydelige?
* Bliver elementer for tæt på mobil?

## Designregel

> **Compact mode må aldrig bryde accessibility baseline.**

***

# 23.18 Beslutningstabel

| Situation                 | Density             | Visning           |
| ------------------------- | ------------------- | ----------------- |
| Dashboard                 | Comfortable         | Summary cards     |
| Haveliste med få haver    | Comfortable/default | Cards             |
| Haveliste med mange haver | Compact/default     | Compact rows      |
| Plantebrowsing            | Default/comfortable | Cards             |
| Plantesøgning             | Compact             | Rows              |
| Materialeliste            | Compact/default     | Rows/table        |
| Filgalleri                | Default/comfortable | Thumbnail grid    |
| Filliste                  | Compact/default     | Rows              |
| Detail page               | Default             | Sections/cards    |
| Sidekolonne               | Default/compact     | Cards/collapsible |
| Confirmation              | Default/comfortable | Dialog            |
| Error/no-access           | Default/comfortable | Inline status     |
| Print                     | Compact             | Table/list        |

***

# 23.19 Anbefalet designbeslutning

Jeg ville låse principperne sådan:

1. **Density følger brugerens opgave.**
2. **Comfortable bruges til overblik, browsing og forklarende UI.**
3. **Default er appens normale rytme.**
4. **Compact bruges til lange lister, søgeresultater, tabeller og print.**
5. **Compact må ikke bruges til kritiske beskeder, confirmations eller no-access states.**
6. **Density styres via tokens/classes, ikke separate komponentkopier.**
7. **Compact reducerer primært spacing, metadata og synlige actions.**
8. **Compact må ikke reducere touch targets under accessibility baseline.**
9. **På mobil reducerer compact informationsmængde, ikke trykfladestørrelse.**
10. **Print har sin egen dokumentations-density.**
11. **Start med kontekstbestemt density; brugerpræference kan tilføjes senere.**
12. **View-mode kan være URL-state; density bør typisk være komponentlogik eller brugerpræference.**

***

# Kort dokumentationstekst

Du kan bruge denne direkte:

> **Data density / compact mode:** MyGardenPlanner bruger tre density levels: comfortable, default og compact. Comfortable bruges til overblik, browsing og forklarende UI. Default er appens normale rytme. Compact bruges til lange lister, søgeresultater, tabeller og print, hvor brugeren skal scanne eller sammenligne mange data. Compact mode må aldrig gøre kritiske beskeder, confirmations, touch targets eller focus states mindre tydelige. Density styres via tokens/classes, så spacing, row height og metadata prioritering ændres konsistent. På mobil reducerer compact primært informationsmængde — ikke trykfladestørrelse.