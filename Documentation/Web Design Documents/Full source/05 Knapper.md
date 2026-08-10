# 5. Knapper

## Overordnet princip

Jeg vil anbefale, at knapperne følger denne regel:

> **Én tydelig primær handling pr. område. Sekundære handlinger skal være rolige. Destruktive handlinger skal være tydelige, men ikke dominerende.**

Det passer til den retning, vi allerede har valgt: professionel, rolig, ikke for “appet” og ikke for spraglet.

***

# 5.1 Knap-hierarki

Jeg foreslår følgende knaptyper:

| Knaptype    | Brug                                                   | Visuel styrke |
| ----------- | ------------------------------------------------------ | ------------: |
| Primary     | Primær handling: Gem, Opret, Åbn, Upload               |           Høj |
| Secondary   | Redigér, Annullér, Download, Detaljer                  |    Medium/lav |
| Accent      | Særlige handlinger: Invitér, gør opmærksom på workflow |        Medium |
| Danger      | Slet, fjern adgang, afvis                              |   Lav/tydelig |
| Ghost/text  | Små sekundære links i tætte layouts                    |           Lav |
| Icon button | Kompakte handlinger i lister/cards                     |    Lav/medium |

Jeg vil undgå for mange farvede knapper. Hvis alt er grønt, mister brugeren overblik.

***

# 5.2 Primary button

Primary button skal være den tydeligste handling.

Bruges til:

* Opret ny have
* Gem ændringer
* Upload fil
* Åbn have, hvis det er hovedhandlingen i et card
* Vælg plante/materiale

```css
.btn-primary {
  background: var(--mgp-primary);
  color: white;
  border-color: var(--mgp-primary);
}

.btn-primary:hover {
  background: var(--mgp-primary-dark);
  border-color: var(--mgp-primary-dark);
}
```

Eksempel:

```html
<button class="btn btn-primary">Gem ændringer</button>
#Opret ny have</a>
```

## Designregel

Brug højst **én primary button pr. card eller sektion**, medmindre der er tale om en toolbar med flere ligeværdige opret-handlinger.

***

# 5.3 Secondary button

Secondary skal være standarden for ikke-primære handlinger.

Bruges til:

* Redigér
* Annullér
* Download
* Detaljer
* Se invitation
* Åbn fil, hvis hovedhandlingen ikke er fremhævet

```css
.btn-secondary {
  background: var(--mgp-surface);
  color: var(--mgp-primary-dark);
  border-color: var(--mgp-border);
}

.btn-secondary:hover {
  background: var(--mgp-surface-muted);
  border-color: rgba(63, 107, 74, .28);
}
```

Eksempel:

```html
#Redigér</a>
#Download</a>
```

Det passer godt til vores card-design, hvor kortene i sig selv er rolige og hvide.

***

# 5.4 Accent button

Accent-knappen bør bruges meget sparsomt. Den er ikke en generel sekundær knap — den er til handlinger, der skal føles varme eller workflow-orienterede.

Bruges til:

* Invitér kunde
* Send invitation igen
* Gør fil permanent
* Fremhæv en særlig næste handling

```css
.btn-accent {
  background: var(--mgp-accent);
  color: white;
  border-color: var(--mgp-accent);
}

.btn-accent:hover {
  filter: brightness(.95);
}
```

Eksempel:

```html
#Invitér kunde</a>
```

## Designregel

Accent må ikke bruges som “anden primærknap” overalt. Den skal reserveres til handlinger med særlig betydning.

***

# 5.5 Danger button

Danger skal bruges til destruktive handlinger, men jeg vil **ikke** anbefale rød fyldt knap som standard.

Slet-handlinger bør være tydelige, men ikke råbe i UI’et.

```css
.btn-danger {
  background: white;
  color: var(--mgp-danger);
  border-color: rgba(159, 58, 56, .35);
}

.btn-danger:hover {
  background: #fff4f3;
  border-color: rgba(159, 58, 56, .55);
}
```

Eksempel:

```html
<button class="btn btn-danger">Slet</button>
```

## Designregel

Destruktive handlinger bør som udgangspunkt placeres sidst i en handlingsgruppe.

Godt:

```html
<div class="card-actions">
  #Åbn</a>
  #Download</a>
  <button class="btn btn-danger">Slet</button>
</div>
```

Mindre godt:

```html
<div class="card-actions">
  <button class="btn btn-danger">Slet</button>
  #Åbn</a>
</div>
```

***

# 5.6 Ghost / text button

Vi bør tilføje en meget rolig tekstknap til kompakte layouts.

Bruges til:

* “Vis mere”
* “Skjul”
* “Nulstil filter”
* “Læs mere”
* små sekundære handlinger i statusbeskeder

```css
.btn-ghost {
  background: transparent;
  color: var(--mgp-primary-dark);
  border-color: transparent;
  padding-inline: var(--space-xs);
}

.btn-ghost:hover {
  background: var(--mgp-primary-soft);
}
```

Eksempel:

```html
<button class="btn btn-ghost">Nulstil filter</button>
```

Jeg vil ikke bruge ghost-knapper til vigtige handlinger som Gem/Opret.

***

# 5.7 Icon buttons

Icon buttons giver mening i lister, thumbnails og compact cards, men kun hvis ikonet er letforståeligt — ellers skal der være tekst.

Eksempler:

* Åbn
* Download
* Slet
* Mere-menu
* Luk
* Tilbage

```css
.btn-icon {
  width: 2.5rem;
  height: 2.5rem;
  padding: 0;
  justify-content: center;
}
```

Eksempel:

```html
<button class="btn btn-secondary btn-icon" aria-label="Download fil">↓</button>
<button class="btn btn-danger btn-icon" aria-label="Slet fil">×</button>
```

## Designregel

Icon-only knapper skal altid have `aria-label`.

***

# 5.8 Knapstørrelser

Jeg foreslår tre størrelser:

| Størrelse | Brug                                    |
| --------- | --------------------------------------- |
| Small     | Compact cards, tabeller, lister         |
| Default   | Standardformularer og cards             |
| Large     | Dashboard hero, primære landing actions |

```css
.btn-sm {
  padding: var(--space-xs) var(--space-sm);
  font-size: var(--font-size-sm);
}

.btn {
  padding: var(--space-sm) var(--space-md);
  font-size: .95rem;
}

.btn-lg {
  padding: var(--space-md) var(--space-lg);
  font-size: var(--font-size-base);
}
```

Min anbefaling:

* Brug `btn` som standard.
* Brug `btn-sm` i compact cards.
* Brug `btn-lg` kun i hero/landing.

***

# 5.9 Knapgrupper

Vi har allerede `btn-row`. Den bør fortsætte som standard til knapgrupper.

```css
.btn-row {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  align-items: center;
}
```

Til card-actions vil jeg holde dem en smule tættere:

```css
.card-actions {
  margin-top: var(--space-sm);
  display: flex;
  gap: var(--space-xs);
  flex-wrap: wrap;
}
```

## Placering

### I cards

```text
Titel
Metadata
Beskrivelse
Badges
[Åbn] [Redigér] [Slet]
```

### I formularer

Primær handling først eller sidst?\
Jeg anbefaler nederst i formularer:

```html
<div class="form-actions">
  <button class="btn btn-primary">Gem ændringer</button>
  <button class="btn btn-secondary">Annullér</button>
</div>
```
```css
.form-actions {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  margin-top: var(--space-lg);
}
```

***

# 5.10 Disabled og loading states

Vi bør definere states nu, så knapperne ikke senere bliver inkonsistente.

## Disabled

```css
.btn:disabled,
.btn[aria-disabled="true"] {
  opacity: .55;
  cursor: not-allowed;
  pointer-events: none;
}
```

## Loading

Loading kan være tekstbaseret i første omgang:

```html
<button class="btn btn-primary" disabled>
  Gemmer…
</button>
```

Hvis vi senere vil have spinner:

```css
.btn-spinner {
  width: 1em;
  height: 1em;
  border: 2px solid currentColor;
  border-right-color: transparent;
  border-radius: 999px;
  animation: spin .7s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
```

Eksempel:

```html
<button class="btn btn-primary" disabled>
  <span class="btn-spinner"></span>
  Gemmer…
</button>
```

***

# 5.11 Focus state

Vi bør have tydelig tastaturfokus, men i samme rolige stil.

```css
.btn:focus-visible {
  outline: none;
  box-shadow: 0 0 0 4px rgba(63, 107, 74, .18);
}
```

Det matcher input focus-state fra formularerne.

***

# 5.12 Responsiv adfærd

På mobil bør knapper ofte fylde bredden i vigtige formularflows, men ikke altid i cards.

Jeg foreslår en utility:

```css
@media (max-width: 640px) {
  .btn-mobile-full {
    width: 100%;
    justify-content: center;
  }

  .form-actions .btn {
    width: 100%;
    justify-content: center;
  }
}
```

Jeg ville ikke automatisk gøre alle card-actions fuld bredde. Det kan hurtigt blive tungt.

***

# 5.13 Knaptekster

Knaptekster bør være handlingsorienterede og konkrete.

## Gode labels

```text
Opret have
Gem ændringer
Upload tegning
Invitér kunde
Åbn have
Download
Slet
Se invitation
Vælg plante
```

## Undgå

```text
OK
Submit
Klik her
Fortsæt
Udfør
```

Specielt fordi appen har mange forskellige objekter, bør knapperne sige hvad handlingen gælder.

***

# 5.14 Knapper i de korttyper vi lige definerede

## Entity card

```html
<div class="card-actions">
  #Åbn have</a>
  #Redigér</a>
</div>
```

## Compact entity card

Hvis hele kortet er klikbart, bør der **ikke** nødvendigvis være en “Detaljer”-knap.

Bedre:

```html
#
  ...
  <span class="meta">Detaljer</span>
</a>
```

Hvis der er flere handlinger:

```html
<article class="card card-compact">
  ...
  #Detaljer</a>
</article>
```

## Media card

```html
<div class="card-actions">
  #Åbn</a>
  #Download</a>
  <button class="btn btn-danger">Slet</button>
</div>
```

## Status message card

Som udgangspunkt ingen knap. Hvis nødvendig:

```html
#Se invitation</a>
```

## Action card

Action card skal typisk have én tydelig primary button:

```html
#Opret</a>
```

***

# Konkret CSS-tilføjelse til designsystemet

Jeg foreslår at tilføje dette til demo4/demo5:

```css
.btn-sm {
  padding: var(--space-xs) var(--space-sm);
  font-size: var(--font-size-sm);
}

.btn-lg {
  padding: var(--space-md) var(--space-lg);
  font-size: var(--font-size-base);
}

.btn-ghost {
  background: transparent;
  color: var(--mgp-primary-dark);
  border-color: transparent;
  padding-inline: var(--space-xs);
}

.btn-ghost:hover {
  background: var(--mgp-primary-soft);
}

.btn-icon {
  width: 2.5rem;
  height: 2.5rem;
  padding: 0;
  justify-content: center;
}

.btn:focus-visible {
  outline: none;
  box-shadow: 0 0 0 4px rgba(63, 107, 74, .18);
}

.btn:disabled,
.btn[aria-disabled="true"] {
  opacity: .55;
  cursor: not-allowed;
  pointer-events: none;
}

.form-actions {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  margin-top: var(--space-lg);
}

@media (max-width: 640px) {
  .btn-mobile-full {
    width: 100%;
    justify-content: center;
  }

  .form-actions .btn {
    width: 100%;
    justify-content: center;
  }
}
```

***

# 5.15 Aligment af knapper i formularer

Jeg vil indføre en officiel variant:

```text
button-aligned
```

**Definition:**

> En formular-action, der ligger i samme grid som form fields, men visuelt aligner med input/select-controlen — ikke med hele feltblokken.

Det er især relevant for:

* filterbarer
* søgeformularer
* kompakte inline forms
* “tilføj”-knapper ved siden af et input
* “nulstil filter”
* “vælg fil” eller “tilføj række” i en feltgruppe

***

# Anbefalet løsning

Gør knappen til en “field-lignende” komponent med samme tre lag:

1. label-placeholder
2. button/control
3. help-placeholder

Så matcher den inputfelternes rytme.

## CSS

```css
.field-label-placeholder {
  visibility: hidden;
  font-size: .92rem;
  font-weight: var(--font-weight-bold);
  line-height: 1.4;
  min-height: 1.4em;
}

.button-aligned {
  display: grid;
  gap: var(--space-xs);
  align-self: start;
}

.button-aligned .btn {
  align-self: start;
}

.button-aligned .help-empty {
  min-height: 1.25em;
}
```

Vi har allerede:

```css
.help {
  min-height: 1.25em;
}

.help-empty {
  visibility: hidden;
}
```

Så den nye del er primært `field-label-placeholder` og `button-aligned`.

***

# Opdateret filterbar markup

I stedet for:

```html
<div class="card filter-bar">
  <div class="field">
    <label for="search">Søg</label>
    <input class="form-control" id="search" placeholder="Søg efter navn..." />
    <div class="help help-empty" aria-hidden="true">&nbsp;</div>
  </div>

  <div class="field">
    <label for="filterType">Type</label>
    <select class="form-select" id="filterType">
      <option>Alle</option>
      <option>Stauder</option>
      <option>Materialer</option>
    </select>
    <div class="help help-empty" aria-hidden="true">&nbsp;</div>
  </div>

  <button class="btn btn-ghost" type="button">Nulstil</button>
</div>
```

Bør den være:

```html
<div class="card filter-bar">
  <div class="field">
    <label for="search">Søg</label>
    <input class="form-control" id="search" placeholder="Søg efter navn..." />
    <div class="help help-empty" aria-hidden="true">&nbsp;</div>
  </div>

  <div class="field">
    <label for="filterType">Type</label>
    <select class="form-select" id="filterType">
      <option>Alle</option>
      <option>Stauder</option>
      <option>Materialer</option>
    </select>
    <div class="help help-empty" aria-hidden="true">&nbsp;</div>
  </div>

  <div class="button-aligned">
    <div class="field-label-placeholder" aria-hidden="true">Handling</div>
    <button class="btn btn-ghost" type="button">Nulstil</button>
    <div class="help help-empty" aria-hidden="true">&nbsp;</div>
  </div>
</div>
```

Det gør, at knappen står på linje med input/select — ikke med labelen og ikke med hele field-blokken.

***

# Hvis knappen skal være mere tydelig

For filterbars synes jeg faktisk ofte, `btn-secondary` kan være bedre end `btn-ghost`, hvis den står alene:

```html
<div class="button-aligned">
  <div class="field-label-placeholder" aria-hidden="true">Handling</div>
  <button class="btn btn-secondary" type="button">Nulstil</button>
  <div class="help help-empty" aria-hidden="true">&nbsp;</div>
</div>
```

Men i Demo6 kan vi godt vise begge principper:

* `btn-ghost` til lav prioritet
* `btn-secondary` til tydeligere handling

Min anbefaling til filterbaren er:

```html
<button class="btn btn-secondary" type="button">Nulstil</button>
```

fordi knappen står som et faktisk filter-control element.

***

# Opdateret filterbar CSS

Jeg ville også justere filterbaren lidt:

```css
.filter-bar {
  display: grid;
  grid-template-columns: 1fr auto auto;
  gap: var(--space-md);
  align-items: start;
}
```

I stedet for:

```css
align-items: end;
```

For nu håndterer hvert element selv sin interne alignment. Det er mere robust.

***

# Endelig designsystemregel

Jeg vil formulere reglen sådan:

> Når en knap placeres i samme grid-række som formularfelter, skal den bruge `.button-aligned`, så den får samme label/control/help-struktur som almindelige fields.

Det giver os tre relevante patterns:

## 1. Normal formularhandling

Bruges nederst i formularer:

```html
<div class="form-actions">
  <button class="btn btn-primary">Gem ændringer</button>
  <button class="btn btn-secondary">Annullér</button>
</div>
```

## 2. Knap i filterbar/feltgrid

Bruges ved siden af inputs/selects:

```html
<div class="button-aligned">
  <div class="field-label-placeholder" aria-hidden="true">Handling</div>
  <button class="btn btn-secondary">Nulstil</button>
  <div class="help help-empty" aria-hidden="true">&nbsp;</div>
</div>
```

## 3. Knap i upload-zone/action-card

Bruges uden label alignment:

```html
<button class="btn btn-secondary">Vælg fil</button>
```

***

# Min anbefalede beslutning for punkt 5

Jeg vil låse knapdesignet sådan:

1. **Primary** bruges til én hovedhandling.
2. **Secondary** bruges til de fleste øvrige handlinger.
3. **Accent** bruges sparsomt til invitation/workflow-highlights.
4. **Danger** er outline, ikke fyldt rød.
5. **Ghost** bruges til meget lavprioriterede handlinger.
6. **Icon-only** må bruges, men kun med `aria-label`.
7. **Small/default/large** størrelser defineres som faste varianter.
8. **Compact entity cards** kan være hele-card-clickable og behøver ikke altid en knap.
9. **Form actions** samles i en `form-actions` række.
10. **Focus, disabled og loading** defineres fra starten.