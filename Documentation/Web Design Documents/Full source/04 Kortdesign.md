# 4. Kortdesign

## Designmål for cards

Cards skal ikke bare være “bokse med skygge”. De skal hjælpe brugeren med at forstå:

* Hvad er dette objekt?
* Hvilken status har det?
* Hvad er den vigtigste metadata?
* Hvad kan jeg gøre herfra?
* Er det et overblikskort, et listekort eller et handlingskort?

Min anbefaling er, at vi definerer et lille, tydeligt card-system i stedet for én generisk card-stil til alt.

***

# Anbefalet card-system

Jeg foreslår disse card-varianter:

| Card-type               | Brug                                                                   |
| ----------------------- | ---------------------------------------------------------------------- |
| **Standard card**       | Almindelig indholdsgruppe, fx sektioner, formularblokke, statusbokse   |
| **Summary card**        | Dashboard/overblik, fx aktiv have, antal bede, seneste filer           |
| **Entity card**         | Haver, planter, materialer, butikker                                   |
| **Media card**          | Vedhæftede filer, thumbnails, tegninger, PDF’er                        |
| **Compact list card**   | Listevisning med mange elementer                                       |
| **Action card**         | Opret/invitér/upload/næste handling                                    |
| **Alert/accent card**   | Afventer, udløber snart, kræver opmærksomhed                           |
| **Status message card** | Korte beskeder om systemtilstand, arbejdsstatus eller brugerhandlinger |

Det giver os et fleksibelt system, uden at designet bliver komplekst.

***

# 4.1 Standard card

Dette er grundkortet, som alt andet bygger på.

```css
.card {
  background: var(--mgp-surface);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
  padding: var(--space-md);
}
```

Det er allerede tæt på det, vi har i demo3.

## Anbefaling

Behold standardkortet ret neutralt:

* hvid baggrund
* diskret border
* meget let skygge
* 16px padding
* ingen kraftig farve som standard

Kortet skal ikke råbe. Indholdet skal være i fokus.

***

# 4.2 Card-struktur

Jeg vil anbefale en fast intern struktur:

```html
<article class="card card-entity">
  <div class="card-main">
    <div class="card-header">
      <div>
        <h3>Villa Solbakken</h3>
        <p class="meta">Kundehave · opdateret for nylig</p>
      </div>

      <span class="badge badge-primary">Aktiv</span>
    </div>

    <p class="card-description">
      Rolig villahave med fokus på staudebede og nem vedligeholdelse.
    </p>

    <div class="card-meta-row">
      <span class="meta numeric">6 bede</span>
      <span class="meta">3 filer</span>
      <span class="meta">2 medlemmer</span>
    </div>
  </div>

  <div class="card-actions">
    #Åbn</a>
    #Redigér</a>
  </div>
</article>
```

Og CSS:

```css
.card-main {
  display: grid;
  gap: var(--space-sm);
}

.card-header {
  display: flex;
  align-items: start;
  justify-content: space-between;
  gap: var(--space-md);
}

.card-description {
  color: var(--mgp-text);
  margin: 0;
  max-width: 68ch;
}

.card-meta-row {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
}

.card-actions {
  margin-top: var(--space-sm);
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-xs);
}
```

Det giver en ensartet struktur på tværs af haver, bede, planter og materialer.

***

# 4.3 Entity card

Entity cards er til objekter som:

* `Have`
* `Bed`
* `Plante`
* `Materiale`
* `Butik`

Disse modeller har typisk navn, beskrivelse, metadata og nogle få centrale handlinger. `Have` har navn, beskrivelse, adresse, kontakt, bede, medlemmer og arkiveringsstatus; `Bed` har navn, beskrivelse, dimensioner og arkiveringsstatus; `Plante` og `Materiale` har navne og faglige egenskaber.

## Have-card

Eksempel:

```html
<article class="card card-entity">
  <div class="card-header">
    <div>
      <h3>Villa Solbakken</h3>
      <p class="meta">Kundehave · Aarhus N</p>
    </div>
    <span class="badge badge-primary">Aktiv</span>
  </div>

  <p class="card-description">
    Rolig villahave med fokus på staudebede, struktur og nem vedligeholdelse.
  </p>

  <div class="card-meta-row">
    <span class="meta numeric">6 bede</span>
    <span class="meta numeric">3 filer</span>
    <span class="meta numeric">2 medlemmer</span>
  </div>

  <div class="card-actions">
    #Åbn have</a>
    #Redigér</a>
  </div>
</article>
```

## Bed-card

```html
<article class="card card-entity">
  <div class="card-header">
    <div>
      <h3>Staudehaven mod syd</h3>
      <p class="meta">Bed #4</p>
    </div>
    <span class="badge badge-muted">Planlagt</span>
  </div>

  <p class="card-description">
    Solrigt bed med lavendel, salvie og prydgræsser.
  </p>

  <div class="card-meta-row">
    <span class="meta numeric">3,50 × 1,20 m</span>
    <span class="meta">Stauder</span>
  </div>

  <div class="card-actions">
    #Åbn bed</a>
    #Redigér</a>
  </div>
</article>
```

Her er `numeric` relevant, fordi bed-modellen understøtter dimensioner.

***

# 4.4 Plante-card

Planter bør have lidt mere fagligt præg, fordi modellen indeholder almindeligt navn, latinsk navn, højde, bredde, lys, blomstring, farve, jordforhold og plantekategori.

```html
<article class="card card-plant">
  <div class="card-header">
    <div>
      <h3>Lavendel</h3>
      <p class="latin-name">Lavandula angustifolia</p>
    </div>
    <span class="badge badge-primary">Staude</span>
  </div>

  <p class="card-description">
    Duftende, tørketålende plante til solrige bede.
  </p>

  <div class="badge-row">
    <span class="badge badge-muted">Sol</span>
    <span class="badge badge-muted numeric">30–60 cm</span>
    <span class="badge badge-muted">Lilla</span>
    <span class="badge badge-muted">Juni–august</span>
  </div>

  <div class="card-actions">
    #Vælg plante</a>
    #Detaljer</a>
  </div>
</article>
```

## Anbefaling for plantecards

Plantecards bør ikke være for billedtunge i masterdataoversigten. Brug hellere:

* navn
* latinsk navn
* 3-5 små chips
* evt. thumbnail senere

Ellers bliver oversigten visuelt støjende.

***

# 4.5 Materiale-card

Materialer er mere praktiske og bør være lidt mere “nøgterne” end plantecards.

`Materiale` har bl.a. materialnavn, standarddimensioner, standardmængde, materialeinfo, farve og kommentar.

```html
<article class="card card-entity">
  <div class="card-header">
    <div>
      <h3>Chaussésten</h3>
      <p class="meta">Belægning · granit</p>
    </div>
    <span class="badge badge-muted">Materiale</span>
  </div>

  <div class="card-meta-row">
    <span class="meta numeric">10 × 10 × 10 cm</span>
    <span class="meta">Grå</span>
  </div>

  <div class="card-actions">
    #Vælg materiale</a>
    #Detaljer</a>
  </div>
</article>
```

***

# 4.6 Media card / thumbnail card

Dette er vigtigt, fordi `VedhaeftetFil` og `VedhaeftetThumbnail` indgår i Layer 1. Filer har filnavn, content type, oprettet dato, ejertype, ejer-id, lifetime og udløbsdato; thumbnails har størrelse, bredde, højde, content type og data.

## Media card med thumbnail

```html
<article class="card card-media">
  <div class="media-preview">
    <div class="thumbnail">PDF</div>
  </div>

  <div class="card-main">
    <div>
      <h3>Haveskitse maj.pdf</h3>
      <p class="meta">PDF · tegning</p>
    </div>

    <div class="badge-row">
      <span class="badge badge-accent">Midlertidig</span>
      <span class="badge badge-muted">Thumbnail small</span>
    </div>

    <div class="card-actions">
      #Åbn</a>
      #Download</a>
      #Slet</a>
    </div>
  </div>
</article>
```

CSS:

```css
.card-media {
  display: grid;
  grid-template-columns: 120px 1fr;
  gap: var(--space-md);
  align-items: start;
}

.media-preview {
  min-width: 0;
}

.card-media .thumbnail {
  min-height: 120px;
}
```

På mobil:

```css
@media (max-width: 640px) {
  .card-media {
    grid-template-columns: 1fr;
  }
}
```

## Anbefaling

Media cards bør tydeligt vise:

* filtype
* filnavn
* status
* primær handling: åbn
* sekundær: download
* destruktiv: slet

Destruktive handlinger bør visuelt holdes sekundære, ikke som rød fyldt knap.

***

# 4.7 Compact list card

Når brugeren har mange planter, materialer eller filer, skal vi ikke altid bruge store cards. Derfor bør vi have en compact variant.

```html
<article class="card card-compact">
  <div>
    <h3>Lavendel</h3>
    <p class="latin-name">Lavandula angustifolia</p>
  </div>

  <div class="card-meta-row">
    <span class="badge badge-muted">Sol</span>
    <span class="badge badge-muted numeric">30–60 cm</span>
  </div>

  #Detaljer</a>
</article>
```

CSS:

```css
.card-compact {
  display: grid;
  grid-template-columns: 1fr auto auto;
  gap: var(--space-md);
  align-items: center;
  padding: var(--space-sm) var(--space-md);
}
```

På mobil:

```css
@media (max-width: 640px) {
  .card-compact {
    grid-template-columns: 1fr;
  }
}
```

## Brug compact cards til

* planteoversigt i listevisning
* materialeoversigt
* butiksoversigt
* invitationer
* medlemmer
* filer i sidepanel

***

# 4.8 Action card

Action cards er gode på dashboardet og på tomme sider.

Eksempel:

```html
<article class="card card-action">
  <div class="action-icon">＋</div>

  <div>
    <h3>Opret ny have</h3>
    <p class="meta">Start en ny kundehave med adresse, kontakt og bede.</p>
  </div>

  #Opret have</a>
</article>
```

CSS:

```css
.card-action {
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: var(--space-md);
  align-items: center;
}

.action-icon {
  width: 2.75rem;
  height: 2.75rem;
  border-radius: 999px;
  display: grid;
  place-items: center;
  background: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
  font-weight: var(--font-weight-bold);
}
```

På mobil:

```css
@media (max-width: 640px) {
  .card-action {
    grid-template-columns: 1fr;
  }

  .action-icon {
    width: 2.5rem;
    height: 2.5rem;
  }
}
```

***

# 4.9 Accent / alert card

Vi skal bruge en variant til ting, der kræver opmærksomhed, men uden at råbe.

Eksempler:

* invitation afventer
* fil udløber snart
* arkiveret have
* manglende kontaktinfo
* midlertidig fil

```css
.card-attention {
  border-left: 5px solid var(--mgp-accent);
  background: var(--mgp-surface);
}

.card-muted {
  background: var(--mgp-surface-muted);
  color: var(--mgp-text-muted);
}

.card-selected {
  border-color: var(--mgp-primary);
  box-shadow: 0 0 0 4px rgba(63, 107, 74, .14);
}
```

## Vigtig regel

Farve må ikke stå alene. Brug altid tekst eller badge:

```html
<span class="badge badge-accent">Udløber snart</span>
```

***

# 4.10 Status message card

Et kompakt kort med ikon/dot, titel og kort forklarende tekst.
Bruges til at vise systemstatus, opmærksomhedspunkter og resultat af handlinger.

Det skal bruges, når UI’et skal kommunikere:

* at noget er gemt eller opdateret
* at noget kræver brugerens opmærksomhed
* at en invitation afventer
* at en fil udløber snart
* at noget mangler
* at en handling ikke kunne gennemføres
* at der findes relevant information, uden at det nødvendigvis er en fejl

## Foreslået struktur

```html
<div class="status-message status-success">
  <span class="status-dot"></span>
  <div>
    <strong>Alt ser fint ud</strong>
    <p class="meta">Haveprofilen er opdateret.</p>
  </div>
</div>
```

Med handling:

```html
<div class="status-message status-warning">
  <span class="status-dot"></span>
  <div class="status-message-content">
    <div>
      <strong>Handling kræves</strong>
      <p class="meta">En invitation afventer svar.</p>
    </div>

    #Se invitation</a>
  </div>
</div>
```

## CSS-Forslag

```css
.status-message {
  display: flex;
  gap: var(--space-sm);
  align-items: start;
  padding: var(--space-sm);
  border-radius: var(--radius-md);
  border: 1px solid var(--mgp-border);
  background: var(--mgp-surface);
}

.status-message-content {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
  align-items: center;
  width: 100%;
}

.status-dot {
  width: .75rem;
  height: .75rem;
  border-radius: 50%;
  margin-top: .42rem;
  flex: 0 0 auto;
  background: var(--mgp-text-muted);
}

.status-success .status-dot {
  background: var(--mgp-primary);
}

.status-info .status-dot {
  background: var(--mgp-text-muted);
}

.status-warning .status-dot {
  background: var(--mgp-accent);
}

.status-danger .status-dot {
  background: var(--mgp-danger);
}

.status-message strong {
  display: block;
  font-weight: var(--font-weight-bold);
}

.status-message .meta {
  margin-top: var(--space-2xs);
}
```

På mobil:

```css
@media (max-width: 640px) {
  .status-message-content {
    align-items: start;
    flex-direction: column;
  }
}
```

***

# Varianter

| Variant          | Brug                                         | Farve         |
| ---------------- | -------------------------------------------- | ------------- |
| `status-success` | Alt OK, gemt, opdateret                      | Primær grøn   |
| `status-info`    | Neutral information                          | Muted/grågrøn |
| `status-warning` | Kræver opmærksomhed, afventer, udløber snart | Terracotta    |
| `status-danger`  | Fejl, sletning fejlede, kritisk problem      | Rød           |

***

# Hvornår skal man bruge status message card?

## Brug den til

* inline-status på dashboard
* status i have-detaljeside
* upload-resultater
* invitationer
* midlertidige filer
* valideringsopsummeringer
* “seneste hændelser”
* små arbejdspåmindelser

## Brug den ikke til

* almindelige entity cards
* store dashboard-metrics
* modal-fejl
* formularfelt-validering direkte under et input
* toast-notifikationer, hvis de senere implementeres

***

# Designbeslutning jeg vil anbefale

Jeg vil tilføje den som en officiel korttype i systemet:

```text
Status message card
```

Og definere den sådan:

> Et kompakt, roligt beskedkort med dot-indikator, titel og forklarende tekst. Bruges til systemstatus, opmærksomhedspunkter og korte arbejdsbeskeder. Farve må ikke stå alene; den skal altid ledsages af tekst.

Den passer rigtig godt til resten af designretningen, fordi den giver brugeren tryghed og overblik uden at bruge hårde alert-komponenter.

***

# 4.11 Hover og klikbarhed

Cards bør ikke alle være klikbare. Det kan blive uklart.

## Anbefaling

* **Entity cards** kan være klikbare, hvis hele kortet åbner objektet.
* **Media cards** bør have tydelige handlinger.
* **Action cards** har tydelig knap.
* **Form cards** er ikke klikbare.
* **Compact list cards** bør være hover + clickable, når de repræsenterer entities og klik altid åbner entity-detaljesiden. Hvis kortet har flere konkurrerende handlinger, bør kun den konkrete handling/knap være klikbar.

| Korttype            |                          Clickable? |   Hover? | Primær brug                        |
| ------------------- | ----------------------------------: | -------: | ---------------------------------- |
| Standard card       |                                 Nej |      Nej | Indholdsgruppe                     |
| Summary card        |                Som udgangspunkt nej | Nej/svag | Dashboard info                     |
| Entity card         | Ja, hvis hele kortet åbner detaljer |       Ja | Haver, bede, planter, materialer   |
| Compact entity card |                                  Ja |       Ja | Listevisning                       |
| Media card          |                            Ofte nej |     Svag | Brug knapper til åbn/download/slet |
| Action card         |              Nej, knappen er primær |     Svag | Opret/upload/invitér               |
| Status message card |                                 Nej |      Nej | Beskeder/status                    |
| Form card           |                                 Nej |      Nej | Formularer                         |

Så ja: **compact list cards bør være hover + clickable, når de repræsenterer entities.**\
Jeg ville til gengæld ændre navnet i designsystemet til **Compact entity card**, når det er den klikbare variant.

## CSS-Forslag

Vi kan genbruge card-clickable, men jeg ville give compact cards en lidt mere “row-like” hover:

```css
.card-clickable {
  cursor: pointer;
  transition:
    transform .15s ease,
    box-shadow .15s ease,
    border-color .15s ease;
}

.card-clickable:hover {
  transform: translateY(-1px);
  box-shadow: var(--shadow-md);
  border-color: rgba(63, 107, 74, .28);
}
```

Hvis hele kortet er et link, kan man gøre sådan:

```html
<a class="card card-compact card-clickable compact-entity-link" href="/plants/123">
  <div>
    <h3>Lavendel</h3>
    <p class="latin-name">Lavandula angustifolia</p>
  </div>

  <div class="badge-row" style="margin-top:0;">
    <span class="badge badge-muted">Sol</span>
    <span class="badge badge-muted numeric">30–60 cm</span>
  </div>

  <span class="meta">Detaljer</span>
</a>
```

Og CSS:

```css
.compact-entity-link {
  color: inherit;
  text-decoration: none;
}

.compact-entity-link:hover {
  color: inherit;
}```
,
Jeg vil holde hover-effekten meget diskret. Appen skal stadig være rolig.


## Hvornår bør Compact list cards ikke være clickable?

Jeg ville **ikke** gøre hele compact card’et klikbart, hvis:

* kortet indeholder flere lige vigtige handlinger
* der er checkbox/select i kortet
* der er inline edit
* der er drag-and-drop reorder
* kortet primært bruges til statusbesked
* der er destruktive handlinger tæt på resten af klikfladen
* brugeren kan komme til at klikke ved et uheld

I de tilfælde bør der være tydelige knapper/links i højre side i stedet.

```css
.card-compact.card-clickable {
  cursor: pointer;
  transition:
    background-color .15s ease,
    border-color .15s ease,
    box-shadow .15s ease,
    transform .15s ease;
}

.card-compact.card-clickable:hover {
  background: var(--mgp-surface);
  border-color: rgba(63, 107, 74, .28);
  box-shadow: var(--shadow-sm);
  transform: translateY(-1px);
}

.card-compact.card-clickable:focus-within {
  border-color: var(--mgp-primary);
  box-shadow: 0 0 0 4px rgba(63, 107, 74, .14);
}
```

Hvis hele kortet er et link, kan man gøre sådan:

```css
.compact-entity-link {
  color: inherit;
  text-decoration: none;
}

.compact-entity-link:hover {
  color: inherit;
}
```

## Men der er en vigtig accessibility-detalje

Hvis hele card’et er klikbart, er det bedst at bruge et rigtigt `<a>`-element, når handlingen navigerer til en detaljeside.


```html
<a class="card card-compact card-clickable compact-entity-link" href="/plants/123">
  ...
</a>
```

Så får man bedre tastatur-navigation og semantik uden ekstra JavaScript.

***

# 4.12 Print

Nogle sider skal kunne printes ifølge opgaven.  Cards skal derfor ikke være afhængige af skygger for struktur.

Print-CSS:

```css
@media print {
  .card {
    box-shadow: none;
    border: 1px solid #ccc;
    break-inside: avoid;
  }

  .card-actions,
  .btn-row {
    display: none !important;
  }
}
```

Det matcher allerede delvist demo3.

***

# Konkrete CSS-tilføjelser til demo3

Jeg foreslår, at vi tilføjer følgende card-CSS til demoen:

```css
.card-main {
  display: grid;
  gap: var(--space-sm);
}

.card-header {
  display: flex;
  align-items: start;
  justify-content: space-between;
  gap: var(--space-md);
}

.card-description {
  margin: 0;
  color: var(--mgp-text);
  max-width: 68ch;
}

.card-meta-row {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
}

.card-entity {
  display: grid;
  gap: var(--space-sm);
}

.card-media {
  display: grid;
  grid-template-columns: 120px 1fr;
  gap: var(--space-md);
  align-items: start;
}

.card-media .thumbnail {
  min-height: 120px;
}

.card-compact {
  display: grid;
  grid-template-columns: 1fr auto auto;
  gap: var(--space-md);
  align-items: center;
  padding: var(--space-sm) var(--space-md);
}

.card-action {
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: var(--space-md);
  align-items: center;
}

.action-icon {
  width: 2.75rem;
  height: 2.75rem;
  border-radius: 999px;
  display: grid;
  place-items: center;
  background: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
  font-weight: var(--font-weight-bold);
}

.card-attention {
  border-left: 5px solid var(--mgp-accent);
}

.card-selected {
  border-color: var(--mgp-primary);
  box-shadow: 0 0 0 4px rgba(63, 107, 74, .14);
}

.card-clickable {
  cursor: pointer;
  transition:
    transform .15s ease,
    box-shadow .15s ease,
    border-color .15s ease;
}

.card-clickable:hover {
  transform: translateY(-1px);
  box-shadow: var(--shadow-md);
  border-color: rgba(63, 107, 74, .28);
}

@media (max-width: 640px) {
  .card-media,
  .card-compact,
  .card-action {
    grid-template-columns: 1fr;
  }
}
```

***

# Min anbefalede beslutning

Jeg vil anbefale, at vi låser følgende card-principper:

1. **Cards er primært hvide med diskret border**
2. **Skygger er små og sekundære**
3. **Farve bruges kun til status, selection og attention**
4. **Entity cards bruger fast struktur: header, description, metadata, actions**
5. **Media cards har thumbnail til venstre på desktop og stacked layout på mobil**
6. **Compact cards bruges til lange lister**
7. **Action cards bruges til “opret/upload/invitér”**
8. **Destruktive handlinger vises som outline, ikke rød primær knap**
9. **Alle kort skal fungere uden skygge i print**