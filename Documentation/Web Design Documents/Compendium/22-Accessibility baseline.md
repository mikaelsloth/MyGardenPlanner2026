### 📄 22-Accessibility baseline.md

# 22 - Accessibility baseline

## 📘 Grundregel
- Accessibility er en integreret baseline i alle komponenter fra starten — ikke en eftermontering. Semantisk HTML prioriteres altid før ARIA, farve må aldrig stå alene som eneste signal, og alle interaktive elementer skal understøtte tastaturnavigation, synlig fokus-ring og tilstrækkelige touch targets.

---

## 🔍 Anvendelse / varianter / typer

| Tilgængelighedsområde | Baseline-krav | Implementering / Markup |
| :--- | :--- | :--- |
| **Semantisk HTML** | Brug altid rigtige HTML5-elementer til deres tilsigtede formål frem for uspecifikke `div`/`span`-wrappers. | `<button>`, `<a>`, `<label>`, `<table>`, `<section>`, `<nav>`, `<aside>`, `<details>`, `<summary>`. |
| **Focus & Tastatur** | Alle interaktive elementer skal kunne tabbes til og have en synlig focus state. Modals/drawers skal have fokusstyring. | `:focus-visible` med høj kontrast. Focus trap i dialoger og automatic focus-restore ved lukning. |
| **Formular-labels** | Alle inputfelter skal have en synlig eller programmatisk identifikation. | Synlige `<label for="...">` tilknyttet `id`. Placeholder må ikke stå alene. |
| **Touch Targets** | Finger-venlige interaktionsarealer på mobile enheder og touchskærme. | Minimum 44×44 px (`--mgp-touch-target-min`). Ikoner indkapsles i større rammer. |
| **Ikoner & Billeder** | Undgå "støj" for skærmlæsere og forsyn informative billeder og ikonknapper med navne. | Informative billeder har `alt="beskrivelse"`. Dekorative har `alt=""`. Icon-only knapper har `aria-label`. |
| **Status & Attention Levels** | Attention Levels (0–3) skal altid kommunikeres via kombinationen af tekst, ikon/form, og farve. Farve må aldrig stå alene som signal. Placeringen skal være forudsigelig og placeret tæt på kilde-objektet. |
| **Reduced Motion** | Animationer og loading-shimmer skal respektere brugerens OS-præferencer. | `@media (prefers-reduced-motion: reduce)` deaktiverer shimmer, spin og overgange. |
| **Alternativ til Drag/Drop** | Ingen vigtig handling må kun kunne udføres ved træk-og-slip. | Altid et supplerende tastatur- og knapbaseret alternativ (fx "Flyt op/ned" eller "Vælg fil"). |
| **Data Density & Compactness** | Compact mode må aldrig forringe a11y-baseline. | Kontrollér at: 1. Touch targets forblive min 44×44px på mobil. 2. Focus rings (`:focus-visible`) forbliver fuldt synlige. 3. Tekststørrelser overholder minimumskrav (min 12px for sekundær meta). 4. Badges og statusbeskeder bevarer tekstlig tydelighed. |

---

## 🚫 Regler (Do / Don't)
- **Do:** Brug altid semantisk HTML før ARIA. ARIA skal supplere og berige — aldrig kompensere for forkert markup.
- **Do:** Forsyn altid icon-only knapper (`.btn-icon`) med en eksplicit og beskrivende tekst via `aria-label` eller skjult tilgængelig tekst.
- **Do:** Sikr at alle formularfelter har en synlig label. Placeholder-tekst må kun bruges som udfyldningseksempel (fx "fx Solbakken 12").
- **Do:** Bevar altid en synlig focus state (`:focus-visible`) med tilstrækkelig kontrast på alle interaktive elementer (knapper, links, inputs, tabs, collapsible headers).
- **Do:** Implementér streng fokusstyring i modals, drawers og previews (fokus flyttes ind ved åbning, fanges inde i elementet, og returneres til udløsende knap ved lukning).
- **Do:** Sikr at tekst altid bærer meningen i statusbeskeder, badges, fejl og adgangsbegrænsninger. Farve må aldrig stå alene.
- **Do:** Tilbyd altid et knapbaseret alternativ til drag-and-drop i upload-zoner og sorteringslister.
- **Do:** Sikr at informative billeder og thumbnails har meningsfuld `alt`-tekst i konteksten, mens dekorative billeder skjules med `alt=""`.
- **Don't:** Fjern aldrig browserens focus outline (`outline: none`) uden øjeblikkeligt at erstatte den med en tydelig, høj-kontrast focus state.
- **Don't:** Brug ikke `role="alert"` til almindelige bekræftelser eller informationer; reserver det til reelle fejl og kritiske advarsler.
- **Don't:** Gør aldrig funktioner, værktøjstips eller informationer afhængige af hover-states.
- **Edge cases:** Hvis fokus styres i komplekse modaler/drawers, og det udløsende element fjernes fra DOM'en under handlingen (fx sletning af et card), skal fokus tvinges tilbage til den nærmeste overordnede container eller sidens hovedoverskrift (`<h1>`) for at undgå tab af fokus.

---

## 🧩 Komponentpåvirkning
- **Type:** Foundation / Cross-cutting Rule Layer
- **Nye Razor-komponenter:** Ingen direkte nye UI-komponenter, men alle eksisterende komponenter i komponentkataloget skal forsynes med en tilgængelighedsnote (`Accessibility Note`).
- **Ændrede Razor-komponenter:**
  - `MgpButton.razor`: Sikring af `aria-label` ved ikon-knapper og minimering af touch target.
  - `MgpFormField.razor`: Eksplicit kobling mellem `<label for>` og input `id` samt `aria-describedby` til fejl.
  - `MgpConfirmDialog.razor` / `MgpFilterDrawer.razor`: Obligatorisk focus trap og focus restore.
  - `MgpStatusMessage.razor`: Korrekt `aria-live` og tekstlig statusbærer.
  - `MgpDataTable.razor`: Semantisk `<table>` struktur.

---

## 🪙 Tokenpåvirkning
- Genbruger globale farve- og interaktionstokens:
  - `--mgp-primary-dark`: Standard kontrastfarve til focus-rings og fremhævede tekst-labels.
  - `--mgp-focus-ring`: Puls/skygge til fokusmarkering på interaktive elementer.
  - `--mgp-text`, `--mgp-text-muted`, `--mgp-danger`: Tekstfarver der skal overholde WCAG 2.2 kontrastkrav (minimum 4.5:1 for normal tekst, 3:1 for store tekster og UI-komponenter).
- Touch target token:
  - `--mgp-touch-target-min: 44px`: Bruges til at sikre interaktionsarealer på tværs af komponenter.

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   Accessibility Baseline CSS Overrides
   ========================================================================== */

/* 1. Global Focus State (WCAG 2.2 Focus Appearance) */
:focus-visible {
  outline: 3px solid var(--mgp-primary-dark) !important;
  outline-offset: 3px !important;
  box-shadow: 0 0 0 4px var(--mgp-focus-ring) !important;
}

/* Særlige focus offsets for kort og række-handlinger */
.card-link:focus-visible,
.row-action:focus-visible,
.btn:focus-visible {
  outline: 3px solid var(--mgp-primary-dark) !important;
  outline-offset: 2px !important;
}

/* 2. Minimum Touch Targets */
.btn,
.icon-button,
.context-tab,
.nav-link-item,
.form-control,
.form-select {
  min-height: var(--mgp-touch-target-min, 44px);
}

.icon-button {
  width: var(--mgp-touch-target-min, 44px);
  height: var(--mgp-touch-target-min, 44px);
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

/* 3. Visuelt skjult tekst (Screen-reader only helper) */
.sr-only,
.visually-hidden-focusable:not(:focus):not(:focus-within) {
  position: absolute !important;
  width: 1px !important;
  height: 1px !important;
  padding: 0 !important;
  margin: -1px !important;
  overflow: hidden !important;
  clip: rect(0, 0, 0, 0) !important;
  white-space: nowrap !important;
  border: 0 !important;
}

/* 4. Global Reduced Motion Override */
@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
    scroll-behavior: auto !important;
  }

  .skeleton-line,
  .skeleton-block,
  .skeleton-icon,
  .skeleton-pill,
  .skeleton-thumbnail,
  .btn-spinner {
    animation: none !important;
    background: var(--mgp-surface-muted) !important;
  }
}
```