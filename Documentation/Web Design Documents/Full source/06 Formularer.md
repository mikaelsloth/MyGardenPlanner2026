# 6. Formularer

## Overordnet designprincip

Jeg vil anbefale denne grundregel:

> **Formularer skal føles som guidet arbejde — ikke som databaseindtastning.**

Det betyder, at vi ikke bare skal vise alle felter i én lang teknisk formular. Vi skal gruppere felter efter brugerens mentale model:

* Hvad er dette?
* Hvor hører det til?
* Hvilke oplysninger er nødvendige nu?
* Hvad kan udfyldes senere?
* Hvad sker der, når jeg gemmer?

Formularer skal derfor være:

* rolige
* tydeligt grupperede
* ikke for kompakte
* gode på mobil
* nemme at scanne
* tydelige med fejl og hjælp
* konsekvente på tværs af objekter

***

# 6.1 Formular-varianter

Jeg foreslår, at vi definerer flere formular-typer i designsystemet.

| Formulartype           | Brug                                              |
| ---------------------- | ------------------------------------------------- |
| **Simple form**        | Korte formularer med få felter                    |
| **Entity form**        | Opret/rediger have, bed, plante, materiale, butik |
| **Sectioned form**     | Lange formularer opdelt i logiske sektioner       |
| **Inline form**        | Hurtig redigering i kort/liste                    |
| **Dialog/modal form**  | Små fokuserede handlinger, fx invitation          |
| **Upload form**        | Filer, thumbnails, dokumenter                     |
| **Filter/search form** | Oversigter og lister                              |

Vi skal undgå én formularstil til alt. Planteformularen og “Invitér kunde” har ikke samme kompleksitet.

***

# 6.2 Grundstruktur for formularer

En standard formular bør have denne struktur:

```html
<form class="form">
  <div class="form-header">
    <h2>Opret have</h2>
    <p class="form-intro">Angiv de vigtigste oplysninger om haven.</p>
  </div>

  <div class="form-section">
    <h3>Grundoplysninger</h3>

    <div class="field">
      <label for="gardenName">Havens navn</label>
      <input id="gardenName" />
      <div class="help">Brug et navn kunden kan genkende.</div>
    </div>
  </div>

  <div class="form-actions">
    <button class="btn btn-primary" type="submit">Gem ændringer</button>
    <button class="btn btn-secondary" type="button">Annullér</button>
  </div>
  </br>
</form>
```

Det passer til de knapprincipper, vi lige har besluttet: én tydelig primary, sekundære handlinger ved siden af, og handlingerne samlet i `.form-actions`.

***

# 6.3 Entity forms

De vigtigste formularer i Layer 1 bliver sandsynligvis:

* Have
* Bed
* Plante
* Materiale
* Butik
* Kontakt
* Invitation
* Vedhæftet fil

Layer 1-modellerne viser fx `Have` med navn, beskrivelse, adresse, kontakt, bede og medlemmer; `Bed` med navn, beskrivelse og dimensioner; `Plante` med almindeligt navn, latinsk navn, beskrivelse, højde, bredde, vandingsinfo, gødningsinfo, lys, blomstring, farve, jordforhold og kommentar; og `Materiale` med navn, dimensioner, mængde, type, farve og kommentar. [\[DesignLayer1 | Txt\]](https://onedrive.live.com?cid=64031C85D39BC2D5\&id=64031C85D39BC2D5!sce72053c44684babbac826e82ca45611)

Derfor bør vi tænke i **feltgrupper**.

## Have-formular

```text
Grundoplysninger
- Havens navn
- Beskrivelse

Kunde / kontakt
- Kontakt
- Adresse

Status
- Aktiv / arkiveret
```

## Bed-formular

```text
Grundoplysninger
- Bednavn
- Beskrivelse

Placering / reference
- Bedreference

Dimensioner
- Længde
- Bredde
- Højde
- Enhed

Status
- Arkiveret
```

## Plante-formular

```text
Navn
- Almindeligt navn
- Latinsk navn

Beskrivelse
- Kort beskrivelse
- Kommentar

Vækst og størrelse
- Højde
- Bredde
- Densitet

Forhold
- Lys
- Jordforhold
- Zone
- Vinterhårdfør

Pleje
- Vanding
- Gødning

Blomstring og farve
- Blomstring start
- Blomstring slut
- Farve
```

## Materiale-formular

```text
Grundoplysninger
- Materialenavn
- Materialetype

Standarder
- Standarddimensioner
- Standardmængde

Udtryk
- Farve

Noter
- Kommentar
```

***

# 6.4 Lange formularer skal opdeles

Planteformularen bliver hurtigt for lang, hvis alt vises som én flad formular. Derfor anbefaler jeg **sectioned forms**.

```html
<div class="form-section card">
  <div class="form-section-header">
    <div>
      <h3>Vækst og størrelse</h3>
      <p class="meta">Angiv plantens typiske mål og tæthed.</p>
    </div>
  </div>

  <div class="form-grid form-grid-3">
    ...
  </div>
</div>
```

## Beslutning

Jeg vil anbefale:

* Korte formularer kan være ét card.
* Lange formularer bør være flere cards.
* Hver sektion skal have titel og evt. hjælpetekst.
* Gem/Annullér bør være nederst, ikke efter hver sektion.
* På meget lange formularer kan en sticky footer/action bar overvejes senere.

***

# 6.5 Felt-layout

## Én kolonne som standard

På mobil og smalle visninger bør alle formularer være én kolonne.

```css
.form-grid {
  display: grid;
  gap: var(--space-md);
}
```

## To eller tre kolonner på desktop

Bruges kun til korte, relaterede felter.

```css
.form-grid-2 {
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.form-grid-3 {
  grid-template-columns: repeat(3, minmax(0, 1fr));
}
```

Eksempel for dimensioner:

```html
<div class="form-grid form-grid-4">
  <div class="field">
    <label for="length="width" inputmode="decimal" />    <label for="length">Længde</label>
  </div>

  <div class="field">
    <label for="height">Højde</label>
    <input id="height" inputmode="decimal" />
  </div>

  <div class="field">
    <label for="unit">Enhed</label>
    <select id="unit">
      <option>cm</option>
      <option>m</option>
    </select>
  </div>
</div>
    <input id="length" inputmode="decimal" />
  </div>

  <div class="field">
    <label for="width">Bredde</label>
```

Da modellerne bruger dimensioner flere steder, giver det god mening at have en genbrugelig dimension-komponent senere.

***

# 6.6 Labels, hjælpetekst og placeholder

## Labels

Labels skal altid være synlige. Jeg vil ikke bruge placeholder som label.

Godt:

```html
<label for="name">Havens navn</label>
<input id="name" placeholder="Fx Villa Solbakken" />
```

Mindre godt:

```html
<input placeholder="Havens navn" />
```

## Hjælpetekst

Hjælpetekst skal bruges, hvor feltet kan misforstås.

```html
<div class="help">Brug et navn kunden kan genkende.</div>
```

## Placeholder

Placeholder bør bruges som eksempel — ikke som instruktion.

```html
<input placeholder="Fx Villa Solbakken" />
```

***

# 6.7 Required og optional

Jeg vil anbefale, at vi ikke plastre formularerne til med `*`.

Bedre:

```html
<label for="name">
  Påkrævet</span>  Havens navn
</label>
```

Og for optional:

```html
<label for="latinName">
  Latinsk navn
  <span class="optional-label">Valgfri</span>
</label>```

CSS:

```css
.required-label,
.optional-label {
  margin-left: var(--space-xs);
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-medium);
  color: var(--mgp-text-muted);
}
```

## Designregel

* Brug “Påkrævet” på få kritiske felter.
* Brug “Valgfri” kun hvor det hjælper.
* Undgå at markere alt.

***

# 6.8 Validering

Validering skal være tydelig, men rolig.

## Felt med fejl

```html
<div class="field field-error">
  <label for="gardenName">Havens navn</label>
  <input id="gardenName" aria-invalid="true" />
  <div class="field-message">Havens navn skal udfyldes.</div>
</div>
```

CSS:

```css
.field-error input,
.field-error select,
.field-error textarea {
  border-color: var(--mgp-danger);
  box-shadow: 0 0 0 4px rgba(159, 58, 56, .10);
}

.field-message {
  margin-top: var(--space-2xs);
  color: var(--mgp-danger);
  font-size: var(--font-size-sm);
}
```

## Succes-besked

Ikke på hvert felt. Brug hellere status message card efter gem:

```html
<div class="status-message status-success"><div class="status-message>
  <div>
    <strong>Ændringer gemt</strong>
    <p class="meta">Haveprofilen er opdateret.</p>
  </div>
</div>
```

Det passer godt med statusbesked-kortene, vi netop har defineret.

***

# 6.9 Formularsektioner med fejl

Ved lange formularer skal brugeren kunne se, hvilken sektion der har fejl.

```html
<div class="form-section card form-section-error"><div class="form-section card">
    <div>
      <h3>Grundoplysninger</h3>
      <p class="meta">Ret felterne markeret med fejl.</p>
    </div>
    <span class="badge badge-danger">2 fejl</span>
  </div>
</div>

```

CSS:

```css
.form-section-error {
  border-left: 5px solid var(--mgp-danger);
}
```

Det skal bruges forsigtigt — kun når det hjælper overblikket.

***

# 6.10 Input-typer

Jeg vil definere styles for:

* text
* textarea
* select
* number/decimal
* date
* checkbox
* radio
* file upload
* search/filter
* segmented options/chips senere

## Standard input

```css
.form-control,
.form-select,
.form-textarea {
  width: 100%;
  border: 1px solid var(--mgp-border);
  background: var(--mgp-surface);
  color: var(--mgp-text);
  border-radius: .65rem;
  padding: var(--space-sm) var(--space-md);
  font: inherit;
  outline: none;
}
```

## Focus

```css
.form-control:focus,
.form-select:focus,
.form-textarea:focus {
  border-color: var(--mgp-primary);
  box-shadow: 0 0 0 4px rgba(63, 107, 74, .16);
}
```

Det matcher allerede den retning, vi har i demoerne.

***

# 6.11 Textarea

Textarea skal ikke være for lille.

```css
textarea.form-control {
  min-height: 7rem;
  resize: vertical;
}
```

Bruges til:

* havebeskrivelse
* bedbeskrivelse
* plantebeskrivelse
* materiale-kommentar
* butiksnoter

Modellerne har flere beskrivelse-/kommentarfelter, så textarea bliver hyppig.

***

# 6.12 Checkbox og toggles

Til boolske felter som fx arkiveret, vinterhårdfør eller aktiv medlemsstatus bør vi bruge tydelige checkbox/toggle-lignende felter. `Have`, `Bed` og `HaveMedlem` har boolske statusfelter som arkiveret/aktiv, og `Plante` har bl.a. vinterhårdfør som boolsk nullable felt. [\[DesignLayer1 | Txt\]](https://onedrive.live.com?cid=64031C85D39BC2D5\&id=64031C85D39BC2D5!sce72053c44684babbac826e82ca45611)

```html
<label class="check-row">
  <input type="checkbox" />
  <span>
    <strong>Arkivér have</strong>
    <span class="meta">Skjuler haven fra aktive oversig
```

CSS:

```css
.check-row {
  display: flex;
  gap: var(--space-sm);
  align-items: start;
  padding: var(--space-sm);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
  background: var(--mgp-surface);
}
```

Jeg vil ikke starte med custom toggles, medmindre de har en klar værdi. Native checkbox er mere robust.

***

# 6.13 Selects og enum-felter

Mange af modellerne har enum-lignende felter, fx roller, invitationstatus, plantekategori, lysforhold, måneder, butikskategori, bestillings- og leveringsmåder.

For disse anbefaler jeg:

* almindelig `<select>` til korte lister
* radio/chips til 2-4 meget vigtige valg
* multi-select først senere, hvis behovet er klart

Eksempel:

```html
<div class="field">
  <label for="light">Lysforhold</label>
  <select id="light" class="form-select">
    <option>Sol</option>
    <option>Halvskygge</option>
    <option>Skygge</option>
  </select>
</div>
```

***

# 6.14 Upload-formular

Filer er vigtige, fordi appen skal håndtere tegninger og dokumenter, og modellerne har både vedhæftet fil, fil-data og thumbnails.

Jeg vil foreslå en upload zone:

```html
<div class="upload-zone">
  <div class=" tegning eller dokument</strong>  <div class="upload-icon">⇧</div>
    <p class="meta">PDF, JPG eller PNG. Thumbnail oprettes automatisk.</p>
  </div>
  <button class="btn btn-secondary" type="button">Vælg fil</button>
</div>
```

CSS:

```css
.upload-zone {
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: var(--space-md);
  align-items: center;
  padding: var(--space-lg);
  border: 1px dashed rgba(63, 107, 74, .35);
  border-radius: var(--radius-lg);
  background: var(--mgp-surface);
}
```

På mobil:

```css
@media (max-width: 640px) {
  .upload-zone {
    grid-template-columns: 1fr;
  }
}
```

***

# 6.15 Filter- og søgeformularer

Oversigtssider får sandsynligvis brug for søgning/filter:

* planter
* materialer
* haver
* filer
* butikker

Jeg vil definere en kompakt filterbar:

```html
<div class="filter-bar card">
  <div class="field field-search">
    <label for">    <label for="search">Søg</label>
    <label for="type">Type</label>
    <select id="type">
      <option>Alle</option>
    </select>
  </div>

  <button class="btn btn-secondary" type="button">Nulstil</button>
</div>
<div>
    <input id="search" placeholder="Søg efter navn..." />
</div>
```

```css
.filter-bar {
  display: grid;
  grid-template-columns: 1fr auto auto;
  gap: var(--space-md);
  align-items: end;
}
```

Filterbars skal være kompakte, men ikke klemt.

***

# 6.16 Inline forms

Inline forms bør bruges sparsomt.

Gode steder:

* hurtigt omdøbe et bed
* ændre status
* tilføje en kort note
* hurtig mængde/dimension

Ikke gode steder:

* komplet planteformular
* komplet haveformular
* uploadflow med metadata

Designregel:

> Inline edit må kun bruges til små, reversible ændringer.

***

# 6.17 Form actions

Vi har allerede besluttet knapprincipper. For formularer bør det være:

```html
<div class="form-actions">
  <button class="btn btn-primary" type="submit">Gem ændringer</button>
  <button class="btn btn-secondary" type="button">Annullér</button>
</div>
```

Ved destruktiv handling i formular:

```html
<div class="form-actions form-actions-split">
  <div class="btn-row">
    <buttonAnnullér</button>    <button class="btn btn-primary" type="submit">Gem ændringer</button>
  </div>

  <button class="btn btn-danger" type="button">Slet</button>
</div>
```

CSS:

```css
.form-actions-split {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
  align-items: center;
}
```

***

# 6.18 Responsiv adfærd

På mobil:

* formularer én kolonne
* form actions fuld bredde
* upload-zone stacked
* filterbar stacked
* ingen små klikmål
* help/error tekst direkte under feltet

```css
@media (max-width: 640px) {
  .form-grid-2,
  .form-grid-3,
  .form-grid-4,
  .filter-bar,
  .upload-zone {
    grid-template-columns: 1fr;
  }

  .form-actions .btn {
    width: 100%;
    justify-content: center;
  }
}
```

***

# 6.19 Print

Nogle sider skal kunne printes ifølge opgaven.  Formularer er sjældent printmål i sig selv, men detailvisninger med formularlignende struktur kan være det. [\[onedrive.live.com\]](https://onedrive.live.com?cid=64031C85D39BC2D5\&id=64031C85D39BC2D5!s6043866348544816bb7b2b0d5911f5c2)

Printprincip:

```css
@media print {
  .form-actions,
  .upload-zone,
  .filter-bar {
    display: none !important;
  }

  input,
  textarea,
  select {
    border: 0;
    background: transparent;
    padding: 0;
  }
}
```

Men jeg vil først indføre printversioner, når vi ved hvilke sider der skal printes.

***

# 6.20 Alignment af felter

Jeg vil anbefale, at alle form fields reserverer plads til hjælpetekst — også når der ikke vises en reel hjælpetekst.

Det giver den mest stabile og rolige formularrytme.

## CSS

Tilføj dette:

```css
.help {
  color: var(--mgp-text-muted);
  font-size: .84rem;
  min-height: 1.25em;
}
```

Og når et felt ikke har hjælpetekst, tilføjer vi en tom placeholder:

```html
<div class="help help-empty" aria-hidden="true">&nbsp;</div>
```

Med CSS:

```css
.help-empty {
  visibility: hidden;
}
```

Så får alle felter samme interne struktur:

```html
<div class="field">
  <label for="materialName">
    Materialenavn
    <span class="required-label">Påkrævet</span>
  </label>
  <input class="form-control" id="materialName" value="Chaussésten" />
  <div class="help">Brug et kort og genkendeligt navn.</div>
</div>

<div class="field">
  <label for="materialType">Materialetype</label>
  <select class="form-select" id="materialType">
    <option>Belægning</option>
    <option>Jord</option>
    <option>Træ</option>
  </select>
  <div class="help help-empty" aria-hidden="true">&nbsp;</div>
</div>
```

Det er den løsning, jeg ville vælge til designsystemet.

***

# Endnu bedre label-struktur

Jeg vil også foreslå, at labels med `Påkrævet` og `Valgfri` får en lidt mere stabil struktur:

```css
.field-label {
  display: flex;
  gap: var(--space-xs);
  align-items: baseline;
  flex-wrap: wrap;
}
```

Så kan label skrives sådan:

```html
<label class="field-label" for="materialName">
  <span>Materialenavn</span>
  <span class="required-label">Påkrævet</span>
</label>
```

Det gør layoutet mere robust, især hvis teksten bliver længere.

***

# Beslutning jeg vil anbefale

Til designsystemet:

> Alle `.field`-komponenter bør have tre logiske lag: label, control og message/help area. Hvis der ikke er hjælpetekst eller fejlbesked, reserveres pladsen med en skjult placeholder.

Det vil også hjælpe senere, når valideringsbeskeder dukker op, fordi layoutet ikke hopper lige så meget.

Konkret CSS:

```css
.field {
  display: grid;
  gap: var(--space-xs);
}

.help,
.field-message {
  min-height: 1.25em;
}

.help-empty {
  visibility: hidden;
}
```

Og så skal felter uden hjælpetekst have:

```html
<div class="help help-empty" aria-hidden="true">&nbsp;</div>
```

Det er en lille ændring, men den vil gøre formularerne mærkbart mere professionelle.


# 6.21 CSS-forslag til demo6

Jeg foreslår, at vi tilføjer disse formular-klasser:

```css
.form {
  display: grid;
  gap: var(--space-lg);
}

.form-header {
  display: grid;
  gap: var(--space-xs);
}

.form-intro {
  color: var(--mgp-text-muted);
  max-width: 68ch;
  margin: 0;
}

.form-section {
  display: grid;
  gap: var(--space-md);
}

.form-section-header {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
  align-items: start;
}

.form-grid {
  display: grid;
  gap: var(--space-md);
}

.form-grid-2 {
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.form-grid-3 {
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.form-grid-4 {
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.field {
  display: grid;
  gap: var(--space-xs);
}

.field label {
  font-size: .92rem;
  font-weight: var(--font-weight-bold);
}

.form-control,
.form-select,
.form-textarea {
  width: 100%;
  border: 1px solid var(--mgp-border);
  background: var(--mgp-surface);
  color: var(--mgp-text);
  border-radius: .65rem;
  padding: var(--space-sm) var(--space-md);
  font: inherit;
  outline: none;
}

.form-control:focus,
.form-select:focus,
.form-textarea:focus {
  border-color: var(--mgp-primary);
  box-shadow: 0 0 0 4px rgba(63, 107, 74, .16);
}

.form-textarea {
  min-height: 7rem;
  resize: vertical;
}

.help {
  color: var(--mgp-text-muted);
  font-size: .84rem;
}

.field-message {
  color: var(--mgp-danger);
  font-size: var(--font-size-sm);
}

.field-error .form-control,
.field-error .form-select,
.field-error .form-textarea {
  border-color: var(--mgp-danger);
  box-shadow: 0 0 0 4px rgba(159, 58, 56, .10);
}

.required-label,
.optional-label {
  margin-left: var(--space-xs);
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-medium);
  color: var(--mgp-text-muted);
}

.check-row {
  display: flex;
  gap: var(--space-sm);
  align-items: start;
  padding: var(--space-sm);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
  background: var(--mgp-surface);
}

.upload-zone {
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: var(--space-md);
  align-items: center;
  padding: var(--space-lg);
  border: 1px dashed rgba(63, 107, 74, .35);
  border-radius: var(--radius-lg);
  background: var(--mgp-surface);
}

.upload-icon {
  width: 2.75rem;
  height: 2.75rem;
  border-radius: 999px;
  display: grid;
  place-items: center;
  background: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
  font-weight: var(--font-weight-bold);
}

.filter-bar {
  display: grid;
  grid-template-columns: 1fr auto auto;
  gap: var(--space-md);
  align-items: end;
}

@media (max-width: 640px) {
  .form-grid-2,
  .form-grid-3,
  .form-grid-4,
  .filter-bar,
  .upload-zone {
    grid-template-columns: 1fr;
  }

  .form-actions .btn {
    width: 100%;
    justify-content: center;
  }
}
```

***

# Min anbefalede beslutning for punkt 6

Jeg vil låse formularprincipperne sådan:

1. **Formularer grupperes efter brugerens opgave — ikke efter databasefelter.**
2. **Synlige labels altid. Placeholder er kun eksempel.**
3. **Hjælpetekst bruges målrettet, ikke overalt.**
4. **Lange formularer opdeles i cards/sektioner.**
5. **Validering vises roligt, men tydeligt.**
6. **Status efter gem vises som status message card.**
7. **Form actions er konsekvente: Gem + Annullér.**
8. **Destruktive handlinger adskilles visuelt.**
9. **Upload og filter får egne formular-varianter.**
10. **Alle formularer skal fungere godt i én kolonne på mobil.**