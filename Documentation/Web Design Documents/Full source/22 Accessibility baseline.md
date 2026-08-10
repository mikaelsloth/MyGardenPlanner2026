# 22. Accessibility baseline

## Overordnet princip

> **Accessibility skal være en baseline i komponenterne — ikke en eftermontering på enkelte sider.**

For MyGardenPlanner betyder det, at alle standardkomponenter bør være tilgængelige som udgangspunkt:

* knapper
* links
* cards
* formularfelter
* filter drawer
* modals
* tabs
* detail pages
* statusbeskeder
* filpreview
* uploadflows
* tables
* collapsible sections

Det er især vigtigt, fordi appen får mange interaktive mønstre: upload, preview, filters, drawers, confirmations, role-restricted actions og detail pages.

***

## 22.1 Accessibility baseline er ikke “perfektion”

Jeg ville definere baseline som:

> **Et minimumsniveau, der forhindrer de mest almindelige UI-barrierer.**

Det betyder ikke, at vi skal overdesigne alt fra dag ét. Men vi skal undgå de klassiske fejl:

* knapper uden tekst eller label
* formularfelter uden label
* focus outline fjernet
* handlinger der kun findes ved hover
* farve som eneste status
* statusbeskeder uden tekst
* modals uden fokusstyring
* ikoner uden accessible name
* images uden relevant alt-tekst
* tabeller uden rigtige headers
* drawers der ikke kan bruges med tastatur

***

# 22.2 Semantic HTML først

Den bedste accessibility starter med korrekt HTML.

## Brug rigtige elementer

| Situation       | Brug                                     |
| --------------- | ---------------------------------------- |
| Handling        | `<button>`                               |
| Navigation/link | \`\`                                     |
| Formularlabel   | `<label>`                                |
| Tabulære data   | `<table>`, `<thead>`, `<th>`, `<tbody>`  |
| Sektion         | `<section>` med heading                  |
| Sidebar/nav     | `<nav>` / `<aside>`                      |
| Statusbesked    | tekst + passende live-region senere      |
| Collapsible     | `<details>` / `<summary>` når det passer |

## Designregel

> **Brug semantisk HTML før ARIA. ARIA skal supplere — ikke kompensere for forkert markup.**

Eksempel:

```html
<button type="button">Upload fil</button>
```

er bedre end:

```html
<div onclick="upload()">Upload fil</div>
```

***

# 22.3 Synlige labels på formularfelter

Formularfelter skal have synlige labels.

## Godt

```html
<label for="gardenName">Havens navn</label>
<input id="gardenName" name="gardenName" />
```

## Undgå

```html
<input placeholder="Havens navn" />
```

som eneste label.

Placeholder kan hjælpe, men bør ikke være eneste identifikation af feltet.

## Designregel

> **Placeholder er ikke label. Alle formularfelter skal have synlig label eller en tydelig programmatisk label.**

Det passer direkte med den baseline, der allerede er noteret i designbeslutningerne: “synlige labels”.

***

# 22.4 Focus states

Alle interaktive elementer skal have synligt fokus.

Det gælder:

* knapper
* links
* inputfelter
* select
* tabs
* accordion/collapsible headers
* drawer controls
* modal actions
* upload dropzone
* file cards hvis de kan fokuseres

W3C’s WCAG 2.2 Understanding-side for Focus Appearance beskriver målet som at gøre keyboard focus lettere at se og angiver, at fokusindikatoren skal have tilstrækkelig størrelse og kontrast.

## CSS-forslag

```css
:focus-visible {
  outline: 3px solid var(--mgp-primary-dark);
  outline-offset: 3px;
}
```

For cards/rows:

```css
.card-link:focus-visible,
.row-action:focus-visible {
  outline: 3px solid var(--mgp-primary-dark);
  outline-offset: 4px;
}
```

## Designregel

> **Fjern aldrig browserens focus outline uden at erstatte den med en tydelig focus state.**

***

# 22.5 Tastaturnavigation

Alle vigtige flows skal kunne bruges med tastatur.

## Skal kunne tabbes til

* primary actions
* filter controls
* tabs
* upload controls
* preview actions
* drawer close/apply
* modal cancel/confirm
* collapsible section headers
* table actions
* pagination

WCAG 2.2 Understanding-listen indeholder bl.a. kriterier for keyboard, no keyboard trap, focus order, focus visible og focus not obscured.

## Designregel

> **Hvis noget kan gøres med mus eller touch, skal det også kunne gøres med tastatur — med en forståelig focus order.**

***

# 22.6 Ingen keyboard traps

Modals, drawers og full-screen previews må ikke fange brugeren uden en vej ud.

## Gælder især

* filter drawer
* mobile bottom sheet
* confirmation dialog
* full-screen media preview
* upload modal
* navigation drawer

## Regler

* Escape/luk skal være muligt, hvis det er relevant.
* Fokus skal flyttes ind i dialog/drawer.
* Fokus skal ikke kunne forsvinde bag overlayet.
* Fokus skal returnere til den knap, der åbnede dialogen.
* Der skal være en tydelig luk/annullér-handling.

## Designregel

> **Modals og drawers skal have fokusstyring og tydelig lukkevej.**

Dette matcher også den baseline, der er nævnt i designbeslutningerne: “modals/drawers med fokusstyring”.

***

# 22.7 Icon-only buttons skal have labels

Hvis en knap kun viser ikon, skal den have et accessible name.

## Eksempel

```html
<button type="button" aria-label="Luk preview">
  ×
</button>
```

```html
<button type="button" aria-label="Åbn flere handlinger">
  ⋯
</button>
```

## Designregel

> **Icon-only buttons skal altid have en tekstlig label via `aria-label` eller synlig tekst.**

Dette er også nævnt direkte i dine designbeslutninger.

***

# 22.8 Farve må ikke stå alene

Status skal altid have tekst, ikke kun farve.

## Dårligt

```text
Rød prik
```

uden forklaring.

## Godt

```text
Upload mislykkedes
Filen er for stor. Vælg en mindre fil.
```

eller:

```text
[Arkiveret]
```

med badge-tekst.

## Gælder

* badges
* status dots
* validation
* no access
* danger actions
* upload status
* invitation status
* archived state
* file lifetime

## Designregel

> **Farve må understøtte betydning, men teksten skal bære betydningen.**

Dette matcher designnotatet, som nævner, at farve ikke må stå alene, og at statusbeskeder skal have tekst.

***

# 22.9 Kontrast

Vi bør bruge farverne roligt, men stadig med nok kontrast.

## Særligt vigtigt for

* tekst på badges
* muted text
* knapper
* danger state
* focus ring
* form borders
* disabled state
* statusbeskeder
* table headers
* links

WCAG 2.2 Understanding-listen indeholder kriterier for bl.a. Contrast Minimum og Non-text Contrast under “Distinguishable”.

## Designregel

> **Design tokens skal testes for kontrast, især muted text, badges, focus states og danger/attention states.**

Jeg ville især teste:

```text
--mgp-text på --mgp-bg
--mgp-text-muted på --mgp-bg
--mgp-primary-dark på --mgp-primary-soft
--mgp-danger på --mgp-danger-soft
white på --mgp-primary
```

***

# 22.10 Touch targets

Vi har allerede talt mobil, men det hører også hjemme i accessibility baseline.

NN/g skriver, at interaktive elementer på touchscreens bør være mindst **1 cm × 1 cm** for at understøtte hurtig og præcis interaktion og reducere fejltryk.

## Praktisk baseline i MyGardenPlanner

```css
.btn,
.icon-button,
.tab,
.nav-link {
  min-height: 44px;
}
```

For ikonknapper:

```css
.icon-button {
  width: 44px;
  height: 44px;
}
```

## Designregel

> **Det klikbare område skal være finger-venligt, også når ikonet visuelt er lille.**

***

# 22.11 Statusbeskeder og feedback

Statusbeskeder skal være:

* tekstlige
* placeret tæt på årsagen
* persistent hvis vigtige
* ikke kun toast
* ikke kun farve
* forståelige uden ikon

## Eksempel

```html
<div class="status-message status-danger" role="alert">
  <strong>Upload mislykkedes</strong>
  <p>Filen er for stor. Vælg en mindre fil.</p>
</div>
```

## Brug `role="alert"` forsigtigt

Jeg ville kun bruge `role="alert"` til fejl eller vigtig feedback, der kræver opmærksomhed. Almindelige success messages kan være mindre påtrængende.

## Designregel

> **Vigtig feedback skal være inline, tekstlig og kunne genlæses.**

Dette passer med både #7 Toasts vs. inline statusbeskeder og accessibility-notatet.

***

# 22.12 Billeder og alt-tekster

Informative billeder skal have alt-tekst. Dekorative billeder skal ikke støje.

## Informativt billede

```html
bed-reference.jpg
```

## Dekorativt billede

```html
leaf-pattern.svg
```

## Fil thumbnails

For filkort kan alt-tekst beskrive filen:

```html
thumbnail.jpg
```

Hvis thumbnail er dekorativ og filnavnet står ved siden af, kan alt være tomt.

## Designregel

> **Alt-tekst skal beskrive formålet med billedet i konteksten — ikke bare gentage filtypen.**

Alt-tekster på informative billeder er også nævnt i dine designbeslutninger.

***

# 22.13 Tabs

Tabs skal være rigtige tabs — ikke bare styling.

## Minimum

* aktiv tab er tydelig
* tabs kan bruges med tastatur
* aktiv tab svarer til synligt panel
* tab labels er korte og forståelige
* på mobil må tabs ikke blive uoverskuelige

## Designregel

> **Tabs skal have tydelig aktiv state, tastaturadgang og stabil struktur.**

Hvis implementeringen bliver kompleks, kan en `<select>`/section switcher på mobil være enklere.

***

# 22.14 Collapsible sections

Vi bruger nu collapsible sections i Demo19b. De skal være tilgængelige.

`<details>` og `<summary>` er et godt udgangspunkt, fordi de har indbygget semantik.

## Regler

* summary skal have meningsfuld tekst
* status skal være synlig i collapsed header
* focus state skal være tydelig
* icon/chevron er supplement, ikke eneste signal
* attention-state må ikke skjules

## Designregel

> **Collapsed headers skal beskrive både indhold og status.**

Eksempel:

```text
Medlemmer
2 aktive · 1 invitation afventer
```

ikke kun:

```text
Medlemmer
```

***

# 22.15 Tables

Tabeller skal kun bruges til ægte tabulære data.

## Baseline

```html
<table>
  <thead>
    <tr>
      <th>Navn</th>
      <th>Type</th>
      <th>Pris</th>
    </tr>
  </thead>
  <tbody>
    ...
  </tbody>
</table>
```

## Regler

* brug `<th>` til headers
* brug ikke tables til layout
* sortering skal være tekstligt forståelig
* mobilstrategi skal være stacked rows eller bevidst scroll
* printtabeller skal være læsbare

## Designregel

> **Brug semantisk table til sammenligning og strukturerede data — ikke til layout.**

***

# 22.16 Drag/drop og alternativer

Hvis vi senere får drag/drop, fx:

* reorder planteplan
* flyt fil til kategori
* flyt plante mellem grupper
* upload via drag/drop

så skal der være et alternativ.

WCAG 2.2 Understanding-listen indeholder “Dragging Movements” under Input Modalities.

## Designregel

> **Drag/drop må aldrig være eneste måde at udføre en vigtig handling på.**

Eksempel:

```text
Flyt op
Flyt ned
Vælg kategori
```

som alternativ til drag/drop.

***

# 22.17 Upload accessibility

Upload er et centralt flow i MyGardenPlanner.

## Upload baseline

* uploadknap med tekst
* drag/drop er supplement
* filinput har label
* status vises inline
* fejl vises inline
* progress har tekst
* thumbnails har processing state
* retry er tastaturtilgængelig
* fjern fil kræver tydelig handling

## Eksempel

```html
<label for="fileUpload">Upload fil</label>
<input id="fileUpload" type="file" />

<div class="status-message">
  <strong>Uploader Haveskitse maj.pdf</strong>
  <p>62% gennemført</p>
</div>
```

## Designregel

> **Upload skal kunne gennemføres uden drag/drop og uden visuel thumbnail.**

***

# 22.18 Modals, confirmations og drawers

Disse er risikoområder.

## Baseline

* tydelig titel
* fokus flyttes ind
* focus trap mens åben
* Escape/luk hvis relevant
* cancel-knap er tydelig
* confirm-knap har specifik tekst
* focus returnerer til udløsende knap
* baggrund er ikke keyboard-aktiv
* mobilknapper er fuld bredde

## Designregel

> **Confirmation-dialoger skal kunne forstås og gennemføres med tastatur og skærmlæser.**

***

# 22.19 Reduced motion

Vi bruger skeleton shimmer og måske animationer. De skal respektere reduced motion.

```css
@media (prefers-reduced-motion: reduce) {
  .skeleton-line,
  .skeleton-block,
  .btn-spinner {
    animation: none;
  }
}
```

## Designregel

> **Animation må aldrig være nødvendig for at forstå UI’et og skal kunne reduceres.**

***

# 22.20 Sprog og labels

Labels skal være konkrete.

## Dårligt

```text
OK
Klik her
Handling
```

## Godt

```text
Slet fil
Arkivér have
Nulstil filtre
Upload fil
Tilbagekald invitation
```

WCAG 2.2 Understanding-listen indeholder “Headings and Labels” og “Label in Name”.

## Designregel

> **Knapper og links skal beskrive handlingen, ikke bare være generiske.**

***

# 22.21 Accessibility i komponentbiblioteket

Jeg ville gøre accessibility til en del af hver komponentdefinition.

## Eksempel: `Button`

Skal definere:

* synligt label
* focus state
* disabled state
* loading state
* icon-only krav
* min touch size

## Eksempel: `Modal`

Skal definere:

* title
* labelled-by
* focus management
* close/cancel
* keyboard behavior

## Eksempel: `StatusMessage`

Skal definere:

* tekst
* variant
* rolle/live behavior
* farve + tekst

## Designregel

> **Hver komponent skal have en accessibility note i komponentordbogen.**

***

# 22.22 Baseline checklist

Jeg ville bruge denne som praktisk checkliste:

## Generelt

* [ ] Siden har semantisk struktur.
* [ ] Headings følger logisk rækkefølge.
* [ ] Links og knapper har tydelige labels.
* [ ] Farve er ikke eneste signal.
* [ ] Focus state er synlig.
* [ ] Tastaturnavigation virker i logisk rækkefølge.
* [ ] Ingen keyboard traps.

## Formularer

* [ ] Alle felter har synlige labels.
* [ ] Fejl vises ved feltet eller formularen.
* [ ] Validation forklarer hvad brugeren skal gøre.
* [ ] Disabled felter forklares, hvis det giver værdi.

## Modals/drawers

* [ ] Fokus flyttes ind.
* [ ] Fokus returneres ved luk.
* [ ] Dialog har titel.
* [ ] Escape/luk fungerer, hvor relevant.
* [ ] Baggrund er ikke keyboard-aktiv.

## Media/filer

* [ ] Informative billeder har alt-tekst.
* [ ] Preview kan lukkes med tastatur.
* [ ] Download/preview actions har tekst.
* [ ] Uploadstatus er tekstlig.

## Mobil/touch

* [ ] Touch targets er finger-venlige.
* [ ] Ingen hover-afhængighed.
* [ ] Knapper i flows er fuld bredde.
* [ ] Toasts dækker ikke vigtige handlinger.

***

# 22.23 Anbefalet designbeslutning

Jeg ville låse principperne sådan:

1. **Accessibility er en baseline i komponenterne, ikke en eftermontering.**
2. **Brug semantisk HTML før ARIA.**
3. **Alle formularfelter skal have synlige labels.**
4. **Alle interaktive elementer skal have synlig focus state.**
5. **Alle primære flows skal kunne bruges med tastatur.**
6. **Icon-only buttons skal have tekstlig label.**
7. **Farve må aldrig være eneste signal.**
8. **Statusbeskeder skal have tekst og placeres kontekstuelt.**
9. **Modals/drawers skal have fokusstyring.**
10. **Informative billeder skal have alt-tekst.**
11. **Tables skal bruge semantisk table markup.**
12. **Drag/drop skal have alternativ handling.**
13. **Animationer skal respektere reduced motion.**
14. **Touch targets skal være finger-venlige.**
15. **Hver komponent i komponentordbogen skal have accessibility-notes.**

***

# Kort dokumentationstekst

Du kan bruge denne direkte:

> **Accessibility baseline:** Accessibility skal være indbygget i komponenterne fra starten. MyGardenPlanner skal bruge semantisk HTML, synlige labels, tydelige focus states, tastaturnavigation, tekstlige statusbeskeder, tilstrækkelig kontrast og alt-tekster på informative billeder. Farve må aldrig være eneste signal. Icon-only buttons skal have tekstlig label, og modals/drawers skal have fokusstyring og tydelig lukkevej. Upload, preview, confirmations og filter drawers skal kunne bruges uden mus, hover eller drag/drop. Hver komponent bør have en accessibility note i komponentordbogen.