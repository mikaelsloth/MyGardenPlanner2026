# 8. Print-principper

## Overordnet princip

Jeg vil anbefale dette som hovedregel:

> **Print skal fokusere på dokumentation og overblik — ikke på navigation, interaktion eller arbejds-UI.**

Det betyder, at printversionen skal fjerne:

* sidebar
* header-navigation
* knapper
* hover-states
* drawer/backdrop
* form actions
* filterbars
* upload-zoner
* interaktive tabs hvis de kun er navigation
* skygger og visuelle effekter

Og bevare:

* titel
* kontekst
* relevante data
* sektioner
* cards som simple bokse
* tabeller/lister
* noter
* status/metadata
* dato/sidefod, hvis relevant

***

# 8.1 Hvad skal kunne printes?

Jeg vil dele printbehov op i tre typer.

## 1. Dokumentationsprint

Eksempler:

* Haveoverblik
* Bedoversigt
* Planteplan
* Materialeliste
* Kundeside
* Filoversigt
* Medlems-/adgangsoversigt

Her skal print være pænt, læsbart og kundevenligt.

## 2. Arbejdsprint

Eksempler:

* Intern plante-/materialeliste
* Checkliste til havebesøg
* Liste over filer eller tegninger
* Bede med dimensioner

Her må det gerne være mere kompakt og praktisk.

## 3. Formular-/redigeringssider

Disse skal som udgangspunkt **ikke** printes som formularer. Hvis en redigeringsside printes, bør den helst fremstå som en læsevisning — ikke med inputfelter, knapper og kontroller.

***

# 8.2 Print skal have egen informationsarkitektur

På skærm kan man godt have:

```text
Sidebar
Header
Tabs
Cards
Buttons
Filters
Forms
Actions
```

På print bør det nærmere være:

```text
Titel
Kontekst / metadata
Indhold
Sektioner
Lister / tabeller
Noter
Footer
```

Eksempel:

```text
Villa Solbakken
Kundehave · Aarhus N

Bede
- Staudehaven mod syd · 3,50 × 1,20 m
- Skyggebed ved terrasse · 2,20 × 1,00 m

Planter
- Lavendel · Lavandula angustifolia · Sol · 30–60 cm
- Salvie · Salvia officinalis · Sol · 40–70 cm
```

Det skal føles som et dokument — ikke et screenshot af appen.

***

# 8.3 Hvad skjules i print?

Jeg vil anbefale denne standard:

```css
@media print {
  .sidebar,
  .header,
  .footer,
  .drawer,
  .drawer-backdrop,
  .btn-row,
  .btn,
  .card-actions,
  .form-actions,
  .filter-bar,
  .upload-zone,
  .nav-drawer,
  .mobile-header,
  .context-tabs.no-print {
    display: none !important;
  }
}
```

Bemærk: Jeg ville ikke altid skjule `.context-tabs`. Hvis tabs repræsenterer relevante sektioner på siden, kan de godt være skjult, fordi printet alligevel bør vise alle relevante sektioner samlet.

***

# 8.4 Cards i print

På skærm bruger vi cards med:

* border
* radius
* let shadow
* white surface
* spacing

I print bør cards være enklere:

```css
@media print {
  .card,
  .hero,
  .status-message {
    box-shadow: none !important;
    border: 1px solid #ccc;
    break-inside: avoid;
    background: white !important;
  }
}
```

## Vigtig regel

Cards skal ikke være afhængige af skygger for at give struktur. Det er vi allerede godt på vej med, fordi vi bruger border.

***

# 8.5 Farver i print

Print bør fungere både med og uden farve.

Det betyder:

* farver må ikke være eneste informationsbærer
* badges skal have tekst
* statusbeskeder skal have tekst
* attention cards skal have tekst, ikke kun venstre border
* selected state er sjældent relevant i print

Eksempel:

```html
<span class="badge badge-accent">Udløber snart</span>
```

er bedre end bare en orange prik.

## Printfarver

Jeg ville undgå at tvinge baggrundsfarver for meget i print. Mange printere/browserindstillinger fjerner baggrunde.

```css
@media print {
  body {
    background: white !important;
    color: #111 !important;
  }

  .badge {
    border: 1px solid #999;
    background: white !important;
    color: #111 !important;
  }
}
```

Man kan godt bruge `print-color-adjust`, men jeg ville være forsigtig:

```css
@media print {
  .print-keep-color {
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
  }
}
```

Det skal kun bruges på elementer, hvor farven er vigtig.

***

# 8.6 Layout i print

Print skal være én kolonne som udgangspunkt.

```css
@media print {
  main {
    max-width: none;
    padding: 0;
  }

  .grid,
  .grid-2,
  .grid-3,
  .grid-4,
  .form-grid,
  .form-grid-2,
  .form-grid-3,
  .form-grid-4 {
    display: block;
  }

  .grid > *,
  .form-grid > * {
    margin-bottom: 12pt;
  }
}
```

Men der er undtagelser:

* korte metadatafelter kan stå i rækker
* tabeller/lister bør forblive kompakte
* dimensioner kan stå inline

Jeg vil dog starte simpelt: print = én kolonne, og så tilføje speciallayout senere.

***

# 8.7 Page breaks

Vi bør aktivt styre sideskift.

```css
@media print {
  h1,
  h2,
  h3 {
    break-after: avoid;
  }

  .card,
  .status-message,
  .print-avoid-break {
    break-inside: avoid;
  }

  .print-page-break {
    break-before: page;
  }
}
```

## Brug

```html
<section class="print-avoid-break">
  ...
</section>
```

Til fx:

* et enkelt bed-card
* statusbesked
* kompakt plantekort
* kundedata

Og:

```html
<section class="print-page-break">
  ...
</section>
```

Til fx:

* ny hovedsektion
* materialeliste
* bilag/filoversigt

***

# 8.8 Formularer i print

Vi har lige arbejdet med formularer, og her er det vigtigt:

> Formularer skal ikke printes som redigerbare formularer. De bør printes som læsevisning.

I praksis:

```css
@media print {
  input,
  textarea,
  select {
    border: 0 !important;
    background: transparent !important;
    padding: 0 !important;
    box-shadow: none !important;
    appearance: none;
  }

  .help,
  .help-empty,
  .field-message {
    display: none !important;
  }

  .field {
    margin-bottom: 10pt;
  }
}
```

Men her skal vi være forsigtige: Hvis en formular har fejlbeskeder, er det måske ikke relevant at printe dem. Print bør typisk ske fra en visningsside, ikke en valideringsside.

***

# 8.9 Links i print

I nogle printdesigns vises URL’er efter links. Det er sjældent pænt i et kundevenligt print.

Jeg vil anbefale:

```css
@media print {
  a::after {
    content: "";
  }
}
```

Hvis vi senere laver teknisk dokumentation, kan URL’er være relevante, men for MyGardenPlanner-kundeprint vil jeg ikke vise dem som standard.

***

# 8.10 Footer og metadata

Print bør have en simpel footer, men ikke nødvendigvis appens normale footer.

Eksempel:

```html
<div class="print-footer">
  MyGardenPlanner · Villa Solbakken · Udskrevet 24. juni 2026
</div>
```

CSS:

```css
.print-footer {
  display: none;
}

@media print {
  .print-footer {
    display: block;
    margin-top: 24pt;
    padding-top: 8pt;
    border-top: 1px solid #ccc;
    font-size: 9pt;
    color: #555;
  }
}
```

I appen kan datoen senere genereres dynamisk. Designmæssigt kan vi bare definere pladsen.

***

# 8.11 Print-only og screen-only utilities

Jeg synes, vi bør definere to simple utilities:

```css
.print-only {
  display: none;
}

@media print {
  .print-only {
    display: block !important;
  }

  .screen-only {
    display: none !important;
  }
}
```

Det giver os mulighed for at lave printvenlige overskrifter, metadata og noter uden at forstyrre skærmvisningen.

Eksempel:

```html
<h1 class="print-only">Villa Solbakken – Haveoverblik</h1>
```

***

# 8.12 Printvenlige tabeller/lister

Til materialelister og plantelister bør print sandsynligvis bruge tabel/liste i stedet for cards.

På skærm:

```text
Cards
Compact cards
Filterbar
Actions
```

På print:

```text
Tabel med navn, type, mål, note
```

Eksempel:

```html
<table class="print-table">
  <thead>
    <tr>
      <th>Navn</th>
      <th>Type</th>
      <th>Mål</th>
      <th>Note</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>Lavendel</td>
      <td>Staude</td>
      <td>30–60 cm</td>
      <td>Sol</td>
    </tr>
  </tbody>
</table>
```

CSS:

```css
.print-table {
  width: 100%;
  border-collapse: collapse;
}

.print-table th,
.print-table td {
  text-align: left;
  border-bottom: 1px solid #ddd;
  padding: 6pt 4pt;
  vertical-align: top;
}

.print-table th {
  font-weight: 700;
}
```

Jeg vil anbefale, at Demo8 viser både:

* print cards
* print table

***

# 8.13 Print scope

Ikke alle sider skal nødvendigvis printe alt.

Vi bør tænke i print scopes:

## Hele siden

Standard browserprint af hele visningen.

## Udvalgt sektion

Fx “Print materialeliste”.

```html
<section class="print-section">
  ...
</section>
```

Senere kan appen evt. lave print-knapper, der printer en bestemt view-model. Men designprincipperne kan forberede det.

***

# 8.14 Hvad med browserens page size?

Vi kan sætte forsigtige regler:

```css
@page {
  margin: 16mm;
}
```

Jeg ville ikke låse A4 hårdt fra start, men i Danmark er A4 naturligt. Vi kan lave:

```css
@page {
  size: A4;
  margin: 16mm;
}
```

Fordel:

* mere forudsigeligt print

Ulempe:

* mindre fleksibelt, hvis brugeren vil gemme som PDF med anden størrelse

Min anbefaling:

> Brug A4 i demo/print-styles, men vær opmærksom på at browser/printdialog kan overstyre.

***

# 8.15 Konkret CSS-pakke til print

Jeg foreslår en samlet printpakke:

```css
.print-only {
  display: none;
}

@media print {
  @page {
    size: A4;
    margin: 16mm;
  }

  body {
    background: white !important;
    color: #111 !important;
    font-size: 10.5pt;
    line-height: 1.4;
  }

  .screen-only,
  .sidebar,
  .header,
  .footer,
  .drawer,
  .drawer-backdrop,
  .btn,
  .btn-row,
  .card-actions,
  .form-actions,
  .filter-bar,
  .upload-zone,
  .nav-drawer,
  .mobile-header {
    display: none !important;
  }

  .print-only {
    display: block !important;
  }

  .app-shell {
    display: block !important;
  }

  main {
    max-width: none !important;
    padding: 0 !important;
  }

  .card,
  .hero,
  .status-message {
    box-shadow: none !important;
    border: 1px solid #ccc !important;
    background: white !important;
    break-inside: avoid;
  }

  .grid,
  .grid-2,
  .grid-3,
  .grid-4 {
    display: block !important;
  }

  .grid > * {
    margin-bottom: 12pt;
  }

  h1,
  h2,
  h3 {
    break-after: avoid;
  }

  .print-avoid-break {
    break-inside: avoid;
  }

  .print-page-break {
    break-before: page;
  }

  .badge {
    background: white !important;
    color: #111 !important;
    border: 1px solid #999 !important;
  }

  input,
  textarea,
  select {
    border: 0 !important;
    background: transparent !important;
    padding: 0 !important;
    box-shadow: none !important;
    appearance: none;
  }

  .help,
  .help-empty,
  .field-message {
    display: none !important;
  }

  a::after {
    content: "";
  }
}
```

***

# Min anbefalede beslutning for punkt 8

Jeg vil låse print-principperne sådan:

1. **Print er en dokumentvisning, ikke en screenshot-visning.**
2. **Navigation, actions, filters og upload skjules.**
3. **Cards printes som simple bordered sections uden skygger.**
4. **Print må ikke være afhængig af farver.**
5. **Badges/status skal have tekst, ikke kun farve.**
6. **Lange grids bliver som udgangspunkt én kolonne.**
7. **Tabeller bruges til printvenlige plante-/materialelister.**
8. **Formularer printes som læsevisning, ikke redigerings-UI.**
9. **Page breaks styres aktivt med utility-klasser.**
10. **`print-only` og `screen-only` utilities tilføjes.**