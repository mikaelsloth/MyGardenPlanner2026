# 19. Tables vs. cards vs. compact lists

## Overordnet princip

> **Vælg visning efter brugerens opgave — ikke efter datatypen alene.**

Det betyder, at “planter” ikke altid skal være cards, og “materialer” ikke altid skal være tabel. Det afhænger af, hvad brugeren prøver at gøre:

* få overblik
* sammenligne
* finde hurtigt
* scanne visuelt
* arbejde med mange rækker
* printe/dokumentere
* åbne detaljer
* vælge noget til et bed/projekt

***

## 19.1 De vigtigste visningstyper

Jeg vil arbejde med fem primære visningsmønstre:

| Visning            | Primær brug                                         |
| ------------------ | --------------------------------------------------- |
| **Cards**          | Overblik, få objekter, visuel scanning              |
| **Compact rows**   | Lange lister, hurtig navigation, effektiv scanning  |
| **Tables**         | Sammenligning, tal, print, strukturerede data       |
| **Thumbnail grid** | Billeder, filer, visuelle dokumenter                |
| **Detail view**    | Når ét objekt er valgt og skal undersøges/redigeres |

Denne opdeling matcher den retning, der allerede er noteret i designbeslutningerne.

***

# 19.2 Cards

## Brug cards når

Cards passer godt, når brugeren skal:

* få hurtigt overblik
* genkende objekter visuelt
* scanne få eller mellemstore mængder
* se metadata + badges + handlinger samlet
* vælge mellem objekter med forskellig karakter
* arbejde på dashboard/landingpage/detailsektioner

## Gode steder til cards

* Haveliste med få/mellem mange haver
* Dashboard summary cards
* Bedoversigt
* Plantekort i card view
* Materialekort i card view
* Filkort med thumbnail
* Invitation/medlemskort, hvis der er få

## Eksempel

```text
Lavendel
Lavandula angustifolia

[Sol] [Staude] [30–60 cm]

[Vis] [Tilføj]
```

## Fordele

* Godt visuelt hierarki
* Plads til badges og status
* Godt til responsive layouts
* Let at gøre rolige og indbydende
* Godt til “garden theme”

## Ulemper

* Kan blive pladskrævende
* Dårligt til meget lange lister
* Dårligt til præcis sammenligning på mange kolonner
* Kan gøre sortering/sammenligning tungere

## Designregel

> **Cards bruges, når overblik og kontekst er vigtigere end tæt data-sammenligning.**

***

# 19.3 Compact rows

Compact rows er en mellemting mellem cards og tables.

## Brug compact rows når

Brugeren skal:

* navigere i mange objekter
* hurtigt finde et element
* scanne navn + få nøgledata
* arbejde med resultater fra søgning/filter
* have tydelige handlinger uden for meget visuel støj

## Gode steder til compact rows

* Store plantelister
* Materialelister
* Filer i listevisning
* Haver i kompakt view
* Invitationer
* Medlemmer
* Søgeresultater
* “Seneste aktivitet” eller “seneste filer”

## Eksempel

```text
Lavendel
Lavandula angustifolia · Sol · 30–60 cm

[Staude] [Vis]
```

## Fordele

* Mere kompakt end cards
* Bedre til store mængder data
* Stadig mere fleksibelt end tabel
* Godt på mobil
* Giver plads til badges og handlinger
* Kan bruges med virtualisering/lazy loading

## Ulemper

* Mindre visuelt end cards
* Ikke lige så godt til kolonne-sammenligning
* Kan blive monotont uden god typografi/spacing

## Designregel

> **Compact rows bruges som standard for lange lister og søge-/filterresultater.**

***

# 19.4 Tables

Tables bør bruges mere selektivt.

## Brug tables når

Brugeren skal:

* sammenligne mange værdier
* se tal, mål, priser eller mængder
* sortere på kolonner
* printe/dokumentere
* eksportere eller kopiere data
* arbejde med materialelister
* kontrollere data systematisk

## Gode steder til tables

* Materialelister
* Plantelister i print
* Indkøbslister
* Mængdeoversigter
* Priser/tilbud
* Dimensioner
* Sammenligning af planter/materialer
* Printvenlige lister

## Eksempel

```text
Navn        Type      Mål          Note
Lavendel    Staude    30–60 cm     Sol
Salvie      Staude    40–70 cm     Sol
```

## Fordele

* God til sammenligning
* God til print
* God til tal og strukturerede data
* Kendt mønster for mange brugere
* Understøtter sortering på kolonner

## Ulemper

* Kan blive tung på mobil
* Mindre venligt visuelt
* Kræver klare kolonneprioriteter
* Ikke godt til billeder/thumbnails
* Dårligt hvis hver række kræver mange handlinger

## Designregel

> **Tables bruges, når sammenligning og struktur er vigtigere end visuel scanning.**

***

# 19.5 Thumbnail grid

Thumbnail grid er en specialiseret visning til visuelle ressourcer.

## Brug thumbnail grid når

* brugeren skal genkende billeder
* filer har thumbnails
* visuel scanning er vigtig
* preview er central
* dokumenter/skitser skal kunne genkendes visuelt

## Gode steder

* Billeder
* Tegninger
* PDF-forsider
* Referencefotos
* Galleri-lignende filvisning

## Fordele

* God til visuel genkendelse
* Passer til billeder/dokumenter
* God til preview-first flows

## Ulemper

* Dårlig til mange metadatafelter
* Ikke velegnet til sortering/sammenligning
* Kan være tungt performance-mæssigt
* Kræver thumbnails og lazy loading

## Designregel

> **Thumbnail grid bruges kun, når det visuelle indhold hjælper brugeren med at vælge.**

***

# 19.6 Detail view

Detail view er ikke et listeformat, men en vigtig del af mønsteret.

## Brug detail view når

* brugeren har valgt ét objekt
* der er mange felter eller relationer
* der skal redigeres
* der er tilknyttede filer, noter eller metadata
* der er behov for context tabs

## Eksempelstruktur

```text
Titel + metadata
Primary actions
Context tabs
Summary cards
Main content
Related files
Activity/status
```

Dette ligger også tæt op ad næste punkt i listen, “Detail page layout”, som er noteret i dine designbeslutninger.

***

# 19.7 Beslutningsmodel

Jeg ville lave denne simple beslutningsmodel:

## Spørgsmål 1: Hvor mange objekter vises?

* Få/mellem mange → cards kan være fint
* Mange → compact rows eller table

## Spørgsmål 2: Skal brugeren sammenligne kolonner?

* Ja → table
* Nej → cards eller rows

## Spørgsmål 3: Er det visuelle vigtigt?

* Ja → cards eller thumbnail grid
* Nej → rows/table

## Spørgsmål 4: Skal det printes?

* Ja → table eller printvenlig liste
* Nej → cards/rows efter behov

## Spørgsmål 5: Er brugeren på mobil?

* Ja → cards/rows
* Nej → alle muligheder

## Spørgsmål 6: Er det en arbejds-/resultatliste?

* Ja → compact rows
* Nej → cards kan være bedre

***

# 19.8 Default pr. område

Her er mit forslag:

## Haver

**Default:** Cards  
**Alternativ:** Compact rows

Hvorfor:

* Haver er relativt få og kontekstuelle.
* Kort kan vise status, lokation, antal bede/filer og senest opdateret.
* Compact rows kan bruges senere ved mange haver.

## Bede

**Default:** Cards  
**Alternativ:** Compact rows

Hvorfor:

* Bede har ofte visuel/kontekstuel betydning.
* Cards kan vise mål, lysforhold, status og relation til have.
* Rows ved større bedoversigter.

## Planter

**Default:** Compact rows eller cards afhængigt af listevolumen  
**Alternativ:** Table for sammenligning/print

Hvorfor:

* Plantedata kan blive stor.
* Cards er gode til browsing.
* Rows er bedre til søgning/filter.
* Tables er gode til sammenligning og print.

## Materialer

**Default:** Compact rows  
**Alternativ:** Table

Hvorfor:

* Materialer er ofte mere strukturerede.
* Mål, type, pris, mængde og leverandør egner sig til table.
* Rows er god standard for appvisning.

## Filer

**Default:** Media cards eller compact rows  
**Alternativ:** Thumbnail grid

Hvorfor:

* Filer kan være dokumenter eller billeder.
* Billeder/skitser passer til thumbnails.
* Lange filoversigter passer til rows.

## Medlemmer/invitationer

**Default:** Compact rows  
**Alternativ:** Cards ved få medlemmer

Hvorfor:

* Navn/email, rolle, status og handlinger passer godt i rows.
* Invitationer er ofte statusorienterede.

***

# 19.9 View-mode toggle

For nogle sider bør brugeren kunne skifte visning.

## Gode kandidater

* Planter
* Materialer
* Filer
* Haver, hvis mange haver
* Måske butikker

## Eksempel

```text
[Cards] [Liste] [Tabel]
```

eller for filer:

```text
[Liste] [Grid]
```

## Designregel

> **View-mode toggle skal kun bruges, når flere visninger giver reel værdi.**

Vi skal ikke tilføje view toggles overalt bare fordi vi kan.

***

# 19.10 View-mode som state

View-mode bør bevares.

Det kan være:

* URL-state, hvis det er del af visningen
* brugerpræference, hvis det er generel præference

Eksempel:

```text
/planter?view=compact
/filer?view=grid
```

## Designregel

> **Hvis brugeren vælger en visning, bør appen huske den i return-flow.**

***

# 19.11 Responsiv adfærd

Tables er mest problematiske på mobil.

## Anbefaling

På mobil bør tables typisk blive til:

* compact rows
* stacked rows
* cards
* eller horisontalt scroll kun hvis det er meget datatungt og bevidst

## Dårligt mobilmønster

```text
Tabel med 8 kolonner mast ind på 375 px
```

## Bedre

```text
Materiale
Chaussésten

Type: Belægning
Mål: 10 × 10 × 10 cm
Farve: Grå
```

## Designregel

> **Tables skal have en mobilstrategi: stacked rows, cards eller kontrolleret horisontal scroll.**

***

# 19.12 Print

Fra print-principperne har vi allerede besluttet, at print ofte bør bruge tables/lister frem for cards.

## Designregel

> **Cards på skærm kan godt blive til tables i print.**

Eksempel:

Skærm:

```text
Materiale-card
```

Print:

```text
Materialeliste-tabel
```

Det betyder, at visningsmønsteret på skærm og print ikke behøver være det samme.

***

# 19.13 Data density

Dette hænger sammen med punkt #13, men vi kan formulere det her:

## Comfortable

* Cards
* Detail overblik
* Dashboard

## Default

* Cards/rows med normal spacing

## Compact

* Lange lister
* Søgeresultater
* Materialer/filer
* Administrative lister

## Designregel

> **Tæthed skal afhænge af opgaven: browsing kræver luft, arbejde med mange data kræver kompakthed.**

***

# 19.14 Actions i forskellige visninger

## Cards

Har plads til flere synlige handlinger:

```text
[Vis] [Redigér] [Arkivér]
```

Men bør stadig holdes nede.

## Compact rows

Bør have få handlinger:

```text
[Vis]
```

Sekundære handlinger kan ligge i menu senere.

## Tables

Bør have minimal actions:

```text
Vis
```

eller én action-kolonne.

## Designregel

> **Jo tættere visningen er, desto færre synlige handlinger pr. række.**

***

# 19.15 Metadata-prioritering

De forskellige visninger bør vise forskellige mængder metadata.

## Cards

* navn
* undertitel
* 2–4 badges
* kort beskrivelse
* 1–3 handlinger

## Compact rows

* navn
* én metatekstlinje
* 1–2 badges
* én primær handling

## Tables

* kolonner med præcise værdier
* minimale badges
* få actions

## Thumbnail grid

* thumbnail
* navn
* type/status
* preview/download

## Designregel

> **Jo mere kompakt visning, desto hårdere skal metadata prioriteres.**

***

# 19.16 Accessibility

## Cards

* hele card må kun være klikbart, hvis det er tydeligt og ikke konflikter med knapper
* headings skal være semantiske
* badges skal have tekst

## Rows

* knapper skal have tydelige labels
* row selection må ikke kun være farve
* keyboard focus skal være tydelig

## Tables

* brug rigtige `<table>`, `<thead>`, `<th>`
* kolonneoverskrifter skal være meningsfulde
* sortering skal angives i tekst/aria senere
* undgå tables kun til layout

## Designregel

> **Brug semantisk table til ægte tabulære data — ikke til almindeligt layout.**

***

# 19.17 Performance

## Cards

Kan være tunge, hvis de har billeder og mange badges.

## Rows

Gode til virtualisering/lazy loading.

## Tables

Kan blive tunge med mange kolonner og interaktion.

## Thumbnail grid

Kræver lazy loading og rigtige thumbnail-størrelser.

## Designregel

> **Store datamængder bør som udgangspunkt bruge compact rows eller table — ikke tunge cards.**

***

# 19.18 Anbefalet beslutningstabel

| Opgave                     | Anbefalet visning   |
| -------------------------- | ------------------- |
| Browse få haver            | Cards               |
| Browse bede i have         | Cards               |
| Søg i mange planter        | Compact rows        |
| Vis planteforslag visuelt  | Cards               |
| Sammenlign planter         | Table               |
| Materialeliste i app       | Compact rows/table  |
| Materialeliste print       | Table               |
| Filoversigt med dokumenter | Compact rows        |
| Billed-/skitseoversigt     | Thumbnail grid      |
| Medlemmer/invitationer     | Compact rows        |
| Dashboard                  | Cards/summary cards |
| Detailvisning              | Detail page         |

***

# 19.19 Komponenter/patterns

Jeg ville definere:

## `EntityCard`

Til haver, bede, planter, materialer.

## `CompactEntityRow`

Til lange lister.

## `DataTable`

Til tabulære data, print og sammenligning.

## `ThumbnailGrid`

Til filer/billeder.

## `ViewModeToggle`

Skift mellem cards/list/table.

## `ResponsiveTableRow`

Stacked row/card på mobil.

## `PrintTable`

Specifik printversion.

***

# 19.20 Anbefalet designbeslutning

Jeg ville låse principperne sådan:

1. **Visning vælges efter brugerens opgave, ikke kun datatypen.**
2. **Cards bruges til overblik, browsing og visuel scanning.**
3. **Compact rows bruges til lange lister og søge-/filterresultater.**
4. **Tables bruges til sammenligning, tal, materialelister og print.**
5. **Thumbnail grid bruges kun, når visuel genkendelse er vigtig.**
6. **Detail view bruges, når ét objekt er valgt og kræver kontekst.**
7. **View-mode toggle bruges kun, hvor flere visninger giver reel værdi.**
8. **View-mode skal bevares i navigation/return-flow.**
9. **Tables skal have en mobilstrategi.**
10. **Cards på skærm kan konverteres til tables i print.**
11. **Jo tættere visning, desto færre synlige handlinger.**
12. **Store datamængder bør ikke vises som tunge cards som standard.**

***

# Kort dokumentationstekst

Du kan bruge denne direkte:

> **Tables vs. cards vs. compact lists:** Visningsform vælges efter brugerens opgave. Cards bruges til overblik, browsing og visuel scanning. Compact rows bruges til lange lister, søge-/filterresultater og hurtig navigation. Tables bruges til sammenligning, tal, materialelister og print. Thumbnail grids bruges til billeder og dokumenter, hvor visuel genkendelse er vigtig. Detail view bruges, når ét objekt er valgt og kræver dybere kontekst. View-mode toggles bør kun bruges, hvor flere visninger giver reel værdi, og brugerens valg bør bevares i navigation state.